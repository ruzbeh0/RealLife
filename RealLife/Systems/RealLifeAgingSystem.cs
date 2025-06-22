
using Colossal;
using Colossal.Collections;
using Game.Agents;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Debug;
using Game.Prefabs;
using Game.Tools;
using Game.Simulation;
using Game;
using RealLife;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Entities.Internal;

#nullable disable
namespace RealLife.Systems
{
    public partial class RealLifeAgingSystem : GameSystemBase
    {
        public static readonly int kUpdatesPerDay = 1;
        private EntityQuery m_HouseholdQuery;
        private EntityQuery m_TimeDataQuery;
        private SimulationSystem m_SimulationSystem;
        private EndFrameBarrier m_EndFrameBarrier;
        public static bool s_DebugAgeAllCitizens = false;
        [DebugWatchValue]
        public NativeValue<int> m_BecomeTeen;
        [DebugWatchValue]
        public NativeValue<int> m_BecomeAdult;
        [DebugWatchValue]
        public NativeValue<int> m_BecomeElder;
        public NativeCounter m_BecomeTeenCounter;
        public NativeCounter m_BecomeAdultCounter;
        public NativeCounter m_BecomeElderCounter;
        private RealLifeAgingSystem.TypeHandle __TypeHandle;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 262144 / (AgingSystem.kUpdatesPerDay * 16);
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            this.m_SimulationSystem = this.World.GetOrCreateSystemManaged<SimulationSystem>();
            this.m_EndFrameBarrier = this.World.GetOrCreateSystemManaged<EndFrameBarrier>();
            this.m_HouseholdQuery = this.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[1]
            {
              ComponentType.ReadOnly<Household>()
            },
                    None = new ComponentType[2]
            {
              ComponentType.ReadOnly<Deleted>(),
              ComponentType.ReadOnly<Temp>()
            }
            });
            this.m_TimeDataQuery = this.GetEntityQuery(ComponentType.ReadOnly<TimeData>());
            this.m_BecomeTeen = new NativeValue<int>(Allocator.Persistent);
            this.m_BecomeAdult = new NativeValue<int>(Allocator.Persistent);
            this.m_BecomeElder = new NativeValue<int>(Allocator.Persistent);
            this.m_BecomeTeenCounter = new NativeCounter(Allocator.Persistent);
            this.m_BecomeAdultCounter = new NativeCounter(Allocator.Persistent);
            this.m_BecomeElderCounter = new NativeCounter(Allocator.Persistent);
            this.RequireForUpdate(this.m_HouseholdQuery);
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.m_BecomeTeen.Dispose();
            this.m_BecomeAdult.Dispose();
            this.m_BecomeElder.Dispose();
            this.m_BecomeTeenCounter.Dispose();
            this.m_BecomeAdultCounter.Dispose();
            this.m_BecomeElderCounter.Dispose();
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnUpdate()
        {
            uint updateFrame = SimulationUtils.GetUpdateFrame(this.m_SimulationSystem.frameIndex, AgingSystem.kUpdatesPerDay, 16);

            RealLifeAgingSystem.AgingJob jobData = new RealLifeAgingSystem.AgingJob()
            {
                m_BecomeTeenCounter = this.m_BecomeTeenCounter.ToConcurrent(),
                m_BecomeAdultCounter = this.m_BecomeAdultCounter.ToConcurrent(),
                m_BecomeElderCounter = this.m_BecomeElderCounter.ToConcurrent(),
                m_UpdateFrameType = InternalCompilerInterface.GetSharedComponentTypeHandle<UpdateFrame>(ref this.__TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle, ref this.CheckedStateRef),
                m_HouseholdCitizenType = InternalCompilerInterface.GetBufferTypeHandle<HouseholdCitizen>(ref this.__TypeHandle.__Game_Citizens_HouseholdCitizen_RO_BufferTypeHandle, ref this.CheckedStateRef),
                m_TravelPurposes = InternalCompilerInterface.GetComponentLookup<TravelPurpose>(ref this.__TypeHandle.__Game_Citizens_TravelPurpose_RO_ComponentLookup, ref this.CheckedStateRef),
                m_Students = InternalCompilerInterface.GetComponentLookup<Game.Citizens.Student>(ref this.__TypeHandle.__Game_Citizens_Student_RO_ComponentLookup, ref this.CheckedStateRef),
                m_Citizens = InternalCompilerInterface.GetComponentLookup<Citizen>(ref this.__TypeHandle.__Game_Citizens_Citizen_RW_ComponentLookup, ref this.CheckedStateRef),
                m_SimulationFrame = this.m_SimulationSystem.frameIndex,
                m_TimeData = this.m_TimeDataQuery.GetSingleton<TimeData>(),
                m_UpdateFrameIndex = updateFrame,
                m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_DebugAgeAllCitizens = AgingSystem.s_DebugAgeAllCitizens,
                child_age_limit = Mod.m_Setting.child_age_limit,
                teen_age_limit = Mod.m_Setting.teen_age_limit,
                men_age_limit = Mod.m_Setting.male_adult_age_limit,
                women_age_limit = Mod.m_Setting.male_adult_age_limit,
                day = TimeSystem.GetDay(this.m_SimulationSystem.frameIndex, this.m_TimeDataQuery.GetSingleton<TimeData>())
            };
            this.Dependency = jobData.ScheduleParallel<RealLifeAgingSystem.AgingJob>(this.m_HouseholdQuery, this.Dependency);
            
            this.m_EndFrameBarrier.AddJobHandleForProducer(this.Dependency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void __AssignQueries(ref SystemState state)
        {
            new EntityQueryBuilder((AllocatorManager.AllocatorHandle)Allocator.Temp).Dispose();
        }

        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            this.__AssignQueries(ref this.CheckedStateRef);
            this.__TypeHandle.__AssignHandles(ref this.CheckedStateRef);
        }

        [UnityEngine.Scripting.Preserve]
        public RealLifeAgingSystem()
        {
        }

        [BurstCompile]
        private struct AgingJob : IJobChunk
        {
            public NativeCounter.Concurrent m_BecomeTeenCounter;
            public NativeCounter.Concurrent m_BecomeAdultCounter;
            public NativeCounter.Concurrent m_BecomeElderCounter;
            [ReadOnly]
            public BufferTypeHandle<HouseholdCitizen> m_HouseholdCitizenType;
            [ReadOnly]
            public SharedComponentTypeHandle<UpdateFrame> m_UpdateFrameType;
            [NativeDisableParallelForRestriction]
            public ComponentLookup<Citizen> m_Citizens;
            [ReadOnly]
            public ComponentLookup<TravelPurpose> m_TravelPurposes;
            [ReadOnly]
            public ComponentLookup<Game.Citizens.Student> m_Students;
            [ReadOnly]
            public uint m_SimulationFrame;
            [ReadOnly]
            public uint m_UpdateFrameIndex;
            [ReadOnly]
            public TimeData m_TimeData;
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            [ReadOnly]
            public bool m_DebugAgeAllCitizens;
            public int child_age_limit;
            public int teen_age_limit;
            public int men_age_limit;
            public int women_age_limit;
            public int day;

            private void LeaveSchool(
              int chunkIndex,
              Entity citizenEntity,
              ComponentLookup<Game.Citizens.Student> students)
            {
                Entity school = students[citizenEntity].m_School;
                this.m_CommandBuffer.RemoveComponent<Game.Citizens.Student>(chunkIndex, citizenEntity);
                this.m_CommandBuffer.AddComponent<StudentsRemoved>(chunkIndex, school);
            }

            public void Execute(
              in ArchetypeChunk chunk,
              int unfilteredChunkIndex,
              bool useEnabledMask,
              in v128 chunkEnabledMask)
            {
                if (!this.m_DebugAgeAllCitizens && (int)chunk.GetSharedComponent<UpdateFrame>(this.m_UpdateFrameType).m_Index != (int)this.m_UpdateFrameIndex)
                    return;
                BufferAccessor<HouseholdCitizen> bufferAccessor = chunk.GetBufferAccessor<HouseholdCitizen>(ref this.m_HouseholdCitizenType);

                for (int index1 = 0; index1 < bufferAccessor.Length; ++index1)
                {
                    DynamicBuffer<HouseholdCitizen> dynamicBuffer = bufferAccessor[index1];
                    for (int index2 = 0; index2 < dynamicBuffer.Length; ++index2)
                    {
                        Entity citizen1 = dynamicBuffer[index2].m_Citizen;
                        // ISSUE: reference to a compiler-generated field
                        Citizen citizen2 = this.m_Citizens[citizen1];
                        CitizenAge age = citizen2.GetAge();
                        int num1 = day - (int)citizen2.m_BirthDay;
                        int num2 = 0;

                        switch (age)
                        {
                            case CitizenAge.Child:
                                num2 = child_age_limit;
                                break;
                            case CitizenAge.Teen:
                                num2 = teen_age_limit;
                                break;
                            case CitizenAge.Adult:
                                num2 = women_age_limit;
                                if ((citizen2.m_State & CitizenFlags.Male) != CitizenFlags.None)
                                {
                                    num2 = men_age_limit;
                                }
                                break;
                            default:
                                continue;
                        }

                        if (num1 >= num2)
                        {
                            switch (age)
                            {
                                case CitizenAge.Child:
                                    if (this.m_Students.HasComponent(citizen1))
                                    {
                                        this.LeaveSchool(unfilteredChunkIndex, citizen1, this.m_Students);
                                    }
                                    this.m_BecomeTeenCounter.Increment();
                                    citizen2.SetAge(CitizenAge.Teen);
                                    this.m_Citizens[citizen1] = citizen2;
                                    continue;
                                case CitizenAge.Teen:
                                    if (this.m_Students.HasComponent(citizen1))
                                    {
                                        this.LeaveSchool(unfilteredChunkIndex, citizen1, this.m_Students);
                                    }
                                    this.m_CommandBuffer.AddComponent<LeaveHouseholdTag>(unfilteredChunkIndex, citizen1);
                                    this.m_BecomeAdultCounter.Increment();
                                    citizen2.SetAge(CitizenAge.Adult);
                                    this.m_Citizens[citizen1] = citizen2;
                                    continue;
                                case CitizenAge.Adult:
                                    if (this.m_TravelPurposes.HasComponent(citizen1) && (this.m_TravelPurposes[citizen1].m_Purpose == Purpose.GoingToWork || this.m_TravelPurposes[citizen1].m_Purpose == Purpose.Working))
                                    {
                                        this.m_CommandBuffer.RemoveComponent<TravelPurpose>(unfilteredChunkIndex, citizen1);
                                    }
                                    this.m_CommandBuffer.RemoveComponent<Worker>(unfilteredChunkIndex, citizen1);
                                    this.m_BecomeElderCounter.Increment();
                                    citizen2.SetAge(CitizenAge.Elderly);
                                    this.m_Citizens[citizen1] = citizen2;
                                    continue;
                                default:
                                    continue;
                            }
                        }
                    }
                }
            }

            void IJobChunk.Execute(
              in ArchetypeChunk chunk,
              int unfilteredChunkIndex,
              bool useEnabledMask,
              in v128 chunkEnabledMask)
            {
                this.Execute(in chunk, unfilteredChunkIndex, useEnabledMask, in chunkEnabledMask);
            }
        }

        private struct TypeHandle
        {
            public SharedComponentTypeHandle<UpdateFrame> __Game_Simulation_UpdateFrame_SharedComponentTypeHandle;
            [ReadOnly]
            public BufferTypeHandle<HouseholdCitizen> __Game_Citizens_HouseholdCitizen_RO_BufferTypeHandle;
            [ReadOnly]
            public ComponentLookup<TravelPurpose> __Game_Citizens_TravelPurpose_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<Game.Citizens.Student> __Game_Citizens_Student_RO_ComponentLookup;
            public ComponentLookup<Citizen> __Game_Citizens_Citizen_RW_ComponentLookup;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                this.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle = state.GetSharedComponentTypeHandle<UpdateFrame>();
                this.__Game_Citizens_HouseholdCitizen_RO_BufferTypeHandle = state.GetBufferTypeHandle<HouseholdCitizen>(true);
                this.__Game_Citizens_TravelPurpose_RO_ComponentLookup = state.GetComponentLookup<TravelPurpose>(true);
                this.__Game_Citizens_Student_RO_ComponentLookup = state.GetComponentLookup<Game.Citizens.Student>(true);
                this.__Game_Citizens_Citizen_RW_ComponentLookup = state.GetComponentLookup<Citizen>();
            }
        }
    }
}
