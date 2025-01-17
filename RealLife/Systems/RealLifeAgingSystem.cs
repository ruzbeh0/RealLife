
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
using Unity.Mathematics;

#nullable disable
namespace RealLife.Systems
{
    public partial class RealLifeAgingSystem : GameSystemBase
    {
        public static readonly int kUpdatesPerDay = 1;
        public static readonly int kMoveFromHomeResource = 1000;
        private EntityQuery m_CitizenGroup;
        private EntityQuery m_TimeDataQuery;
        private EntityQuery m_HouseholdPrefabQuery;
        private SimulationSystem m_SimulationSystem;
        private EndFrameBarrier m_EndFrameBarrier;
        private NativeQueue<Entity> m_MoveFromHomeQueue;
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
            this.m_CitizenGroup = this.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[1]
              {
          ComponentType.ReadOnly<Citizen>()
              },
                None = new ComponentType[2]
              {
          ComponentType.ReadOnly<Deleted>(),
          ComponentType.ReadOnly<Temp>()
              }
            });
            this.m_TimeDataQuery = this.GetEntityQuery(ComponentType.ReadOnly<TimeData>());
            this.m_HouseholdPrefabQuery = this.GetEntityQuery(ComponentType.ReadOnly<Game.Prefabs.ArchetypeData>(), ComponentType.ReadOnly<Game.Prefabs.HouseholdData>(), ComponentType.ReadOnly<DynamicHousehold>());
            this.m_MoveFromHomeQueue = new NativeQueue<Entity>((AllocatorManager.AllocatorHandle)Allocator.Persistent);
            this.m_BecomeTeen = new NativeValue<int>(Allocator.Persistent);
            this.m_BecomeAdult = new NativeValue<int>(Allocator.Persistent);
            this.m_BecomeElder = new NativeValue<int>(Allocator.Persistent);
            this.m_BecomeTeenCounter = new NativeCounter(Allocator.Persistent);
            this.m_BecomeAdultCounter = new NativeCounter(Allocator.Persistent);
            this.m_BecomeElderCounter = new NativeCounter(Allocator.Persistent);
            this.RequireForUpdate(this.m_CitizenGroup);
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.m_MoveFromHomeQueue.Dispose();
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
            this.__TypeHandle.__Game_Citizens_TravelPurpose_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_Student_RO_BufferLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Citizen_RW_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref this.CheckedStateRef);

            RealLifeAgingSystem.AgingJob jobData1 = new RealLifeAgingSystem.AgingJob()
            {
                m_BecomeTeenCounter = this.m_BecomeTeenCounter.ToConcurrent(),
                m_BecomeAdultCounter = this.m_BecomeAdultCounter.ToConcurrent(),
                m_BecomeElderCounter = this.m_BecomeElderCounter.ToConcurrent(),
                m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle,
                m_CitizenType = this.__TypeHandle.__Game_Citizens_Citizen_RW_ComponentTypeHandle,
                m_StudentType = this.__TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle,
                m_UpdateFrameType = this.__TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle,
                m_Students = this.__TypeHandle.__Game_Buildings_Student_RO_BufferLookup,
                m_Purposes = this.__TypeHandle.__Game_Citizens_TravelPurpose_RO_ComponentLookup,
                m_MoveFromHomeQueue = this.m_MoveFromHomeQueue.AsParallelWriter(),
                m_SimulationFrame = this.m_SimulationSystem.frameIndex,
                m_TimeData = this.m_TimeDataQuery.GetSingleton<TimeData>(),
                m_UpdateFrameIndex = updateFrame,
                m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_DebugAgeAllCitizens = AgingSystem.s_DebugAgeAllCitizens,
                child_age_limit = Mod.m_Setting.child_age_limit,
                teen_age_limit = Mod.m_Setting.teen_age_limit,
                adult_age_limit = Mod.m_Setting.adult_age_limit,
                day = TimeSystem.GetDay(this.m_SimulationSystem.frameIndex, this.m_TimeDataQuery.GetSingleton<TimeData>())
            };
            this.Dependency = jobData1.ScheduleParallel<RealLifeAgingSystem.AgingJob>(this.m_CitizenGroup, this.Dependency);
            this.m_EndFrameBarrier.AddJobHandleForProducer(this.Dependency);
            this.__TypeHandle.__Game_Prefabs_ArchetypeData_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_HouseholdCitizen_RW_BufferLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_HouseholdMember_RW_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Household_RW_ComponentLookup.Update(ref this.CheckedStateRef);
            JobHandle outJobHandle;

            RealLifeAgingSystem.MoveFromHomeJob jobData2 = new RealLifeAgingSystem.MoveFromHomeJob()
            {
                m_Households = this.__TypeHandle.__Game_Citizens_Household_RW_ComponentLookup,
                m_HouseholdMembers = this.__TypeHandle.__Game_Citizens_HouseholdMember_RW_ComponentLookup,
                m_HouseholdCitizens = this.__TypeHandle.__Game_Citizens_HouseholdCitizen_RW_BufferLookup,
                m_ArchetypeDatas = this.__TypeHandle.__Game_Prefabs_ArchetypeData_RO_ComponentLookup,
                m_HouseholdPrefabs = this.m_HouseholdPrefabQuery.ToEntityListAsync((AllocatorManager.AllocatorHandle)this.World.UpdateAllocator.ToAllocator, out outJobHandle),
                m_RandomSeed = RandomSeed.Next(),
                m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer(),
                m_MoveFromHomeQueue = this.m_MoveFromHomeQueue,
                m_BecomeTeen = this.m_BecomeTeen,
                m_BecomeAdult = this.m_BecomeAdult,
                m_BecomeElder = this.m_BecomeElder,
                m_BecomeTeenCounter = this.m_BecomeTeenCounter,
                m_BecomeAdultCounter = this.m_BecomeAdultCounter,
                m_BecomeElderCounter = this.m_BecomeElderCounter
            };
            this.Dependency = jobData2.Schedule<RealLifeAgingSystem.MoveFromHomeJob>(JobHandle.CombineDependencies(outJobHandle, this.Dependency));

            this.m_EndFrameBarrier.AddJobHandleForProducer(this.Dependency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void __AssignQueries(ref SystemState state)
        {
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
        private struct MoveFromHomeJob : IJob
        {
            public NativeQueue<Entity> m_MoveFromHomeQueue;
            public EntityCommandBuffer m_CommandBuffer;
            public ComponentLookup<Household> m_Households;
            public ComponentLookup<HouseholdMember> m_HouseholdMembers;
            public BufferLookup<HouseholdCitizen> m_HouseholdCitizens;
            [ReadOnly]
            public ComponentLookup<Game.Prefabs.ArchetypeData> m_ArchetypeDatas;
            [ReadOnly]
            public NativeList<Entity> m_HouseholdPrefabs;
            public RandomSeed m_RandomSeed;
            public NativeCounter m_BecomeTeenCounter;
            public NativeCounter m_BecomeAdultCounter;
            public NativeCounter m_BecomeElderCounter;
            public NativeValue<int> m_BecomeTeen;
            public NativeValue<int> m_BecomeAdult;
            public NativeValue<int> m_BecomeElder;

            public void Execute()
            {
                this.m_BecomeTeen.value = this.m_BecomeTeenCounter.Count;
                this.m_BecomeAdult.value = this.m_BecomeAdultCounter.Count;
                this.m_BecomeElder.value = this.m_BecomeElderCounter.Count;
                Random random = this.m_RandomSeed.GetRandom(62347);
                Entity entity1;

                while (this.m_MoveFromHomeQueue.TryDequeue(out entity1))
                {
                    Entity householdPrefab = this.m_HouseholdPrefabs[random.NextInt(this.m_HouseholdPrefabs.Length)];
                    Game.Prefabs.ArchetypeData archetypeData = this.m_ArchetypeDatas[householdPrefab];
                    HouseholdMember householdMember = this.m_HouseholdMembers[entity1];
                    Entity household1 = householdMember.m_Household;
                    if (this.m_HouseholdCitizens.HasBuffer(household1))
                    {
                        DynamicBuffer<HouseholdCitizen> householdCitizen = this.m_HouseholdCitizens[household1];
                        if (householdCitizen.Length > 1)
                        {
                            Household household2 = this.m_Households[household1];
                            Entity entity2 = this.m_CommandBuffer.CreateEntity(archetypeData.m_Archetype);
                            int num = math.min(RealLifeAgingSystem.kMoveFromHomeResource, household2.m_Resources / 2);
                            this.m_CommandBuffer.SetComponent<Household>(entity2, new Household()
                            {
                                m_Flags = household2.m_Flags,
                                m_Resources = num
                            });
                            household2.m_Resources -= num;
                            this.m_Households[household1] = household2;
                            this.m_CommandBuffer.AddComponent<PropertySeeker>(entity2, new PropertySeeker());
                            this.m_CommandBuffer.SetComponent<PrefabRef>(entity2, new PrefabRef()
                            {
                                m_Prefab = householdPrefab
                            });
                            householdMember.m_Household = entity2;
                            this.m_CommandBuffer.SetComponent<HouseholdMember>(entity1, householdMember);
                            this.m_CommandBuffer.SetBuffer<HouseholdCitizen>(entity2).Add(new HouseholdCitizen()
                            {
                                m_Citizen = entity1
                            });
                            for (int index = 0; index < householdCitizen.Length; ++index)
                            {
                                if (householdCitizen[index].m_Citizen == entity1)
                                {
                                    householdCitizen.RemoveAt(index);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        [BurstCompile]
        private struct AgingJob : IJobChunk
        {
            public NativeCounter.Concurrent m_BecomeTeenCounter;
            public NativeCounter.Concurrent m_BecomeAdultCounter;
            public NativeCounter.Concurrent m_BecomeElderCounter;
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            public ComponentTypeHandle<Citizen> m_CitizenType;
            [ReadOnly]
            public ComponentTypeHandle<Game.Citizens.Student> m_StudentType;
            [ReadOnly]
            public SharedComponentTypeHandle<UpdateFrame> m_UpdateFrameType;
            [ReadOnly]
            public BufferLookup<Game.Buildings.Student> m_Students;
            [ReadOnly]
            public ComponentLookup<TravelPurpose> m_Purposes;
            public TimeData m_TimeData;
            public NativeQueue<Entity>.ParallelWriter m_MoveFromHomeQueue;
            public uint m_SimulationFrame;
            public uint m_UpdateFrameIndex;
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            public bool m_DebugAgeAllCitizens;
            public int child_age_limit;
            public int teen_age_limit;
            public int adult_age_limit;
            public int day;

            private void LeaveSchool(
              int chunkIndex,
              int i,
              Entity student,
              NativeArray<Game.Citizens.Student> students)
            {
                Entity school = students[i].m_School;
                this.m_CommandBuffer.RemoveComponent<Game.Citizens.Student>(chunkIndex, student);
                if (!this.m_Students.HasBuffer(school))
                    return;
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
                NativeArray<Entity> nativeArray1 = chunk.GetNativeArray(this.m_EntityType);
                NativeArray<Citizen> nativeArray2 = chunk.GetNativeArray<Citizen>(ref this.m_CitizenType);
                NativeArray<Game.Citizens.Student> nativeArray3 = chunk.GetNativeArray<Game.Citizens.Student>(ref this.m_StudentType);

                for (int index = 0; index < nativeArray1.Length; ++index)
                {
                    Citizen citizen = nativeArray2[index];
                    CitizenAge age = citizen.GetAge();
                    int num1 = day - (int)citizen.m_BirthDay;
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
                            num2 = adult_age_limit;
                            break;
                        default:
                            continue;
                    }

                    if (num1 >= num2)
                    {
                        //Mod.log.Info($"num1:{num1},num2:{num2},AGE:{age}");
                        Entity entity = nativeArray1[index];
                        
                        switch (age)
                        {
                            case CitizenAge.Child:
                                if (chunk.Has<Game.Citizens.Student>(ref this.m_StudentType))
                                {
                                    this.LeaveSchool(unfilteredChunkIndex, index, entity, nativeArray3);
                                }
                                this.m_BecomeTeenCounter.Increment();
                                citizen.SetAge(CitizenAge.Teen);
                                nativeArray2[index] = citizen;
                                continue;
                            case CitizenAge.Teen:
                                if (chunk.Has<Game.Citizens.Student>(ref this.m_StudentType))
                                {
                                    this.LeaveSchool(unfilteredChunkIndex, index, entity, nativeArray3);
                                }
                                this.m_MoveFromHomeQueue.Enqueue(entity);
                                this.m_BecomeAdultCounter.Increment();
                                citizen.SetAge(CitizenAge.Adult);
                                nativeArray2[index] = citizen;
                                continue;
                            case CitizenAge.Adult:
                                this.m_BecomeElderCounter.Increment();
                                if (this.m_Purposes.HasComponent(entity) && (this.m_Purposes[entity].m_Purpose == Game.Citizens.Purpose.GoingToWork || this.m_Purposes[entity].m_Purpose == Game.Citizens.Purpose.Working))
                                {
                                    this.m_CommandBuffer.RemoveComponent<TravelPurpose>(unfilteredChunkIndex, entity);
                                }
                                this.m_CommandBuffer.RemoveComponent<Worker>(unfilteredChunkIndex, entity);
                                citizen.SetAge(CitizenAge.Elderly);
                                nativeArray2[index] = citizen;
                                continue;
                            default:
                                continue;
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
            [ReadOnly]
            public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;
            public ComponentTypeHandle<Citizen> __Game_Citizens_Citizen_RW_ComponentTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<Game.Citizens.Student> __Game_Citizens_Student_RO_ComponentTypeHandle;
            public SharedComponentTypeHandle<UpdateFrame> __Game_Simulation_UpdateFrame_SharedComponentTypeHandle;
            [ReadOnly]
            public BufferLookup<Game.Buildings.Student> __Game_Buildings_Student_RO_BufferLookup;
            [ReadOnly]
            public ComponentLookup<TravelPurpose> __Game_Citizens_TravelPurpose_RO_ComponentLookup;
            public ComponentLookup<Household> __Game_Citizens_Household_RW_ComponentLookup;
            public ComponentLookup<HouseholdMember> __Game_Citizens_HouseholdMember_RW_ComponentLookup;
            public BufferLookup<HouseholdCitizen> __Game_Citizens_HouseholdCitizen_RW_BufferLookup;
            [ReadOnly]
            public ComponentLookup<Game.Prefabs.ArchetypeData> __Game_Prefabs_ArchetypeData_RO_ComponentLookup;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                this.__Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                this.__Game_Citizens_Citizen_RW_ComponentTypeHandle = state.GetComponentTypeHandle<Citizen>();
                this.__Game_Citizens_Student_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Citizens.Student>(true);
                this.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle = state.GetSharedComponentTypeHandle<UpdateFrame>();
                this.__Game_Buildings_Student_RO_BufferLookup = state.GetBufferLookup<Game.Buildings.Student>(true);
                this.__Game_Citizens_TravelPurpose_RO_ComponentLookup = state.GetComponentLookup<TravelPurpose>(true);
                this.__Game_Citizens_Household_RW_ComponentLookup = state.GetComponentLookup<Household>();
                this.__Game_Citizens_HouseholdMember_RW_ComponentLookup = state.GetComponentLookup<HouseholdMember>();
                this.__Game_Citizens_HouseholdCitizen_RW_BufferLookup = state.GetBufferLookup<HouseholdCitizen>();
                this.__Game_Prefabs_ArchetypeData_RO_ComponentLookup = state.GetComponentLookup<Game.Prefabs.ArchetypeData>(true);
            }
        }
    }
}
