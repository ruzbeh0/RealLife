using Colossal;
using Colossal.Collections;
using Colossal.Entities;
using Game;
using Game.Simulation;
using Game.Buildings;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Debug;
using Game.Prefabs;
using Game.Tools;
using Game.Triggers;
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
    public partial class RealLifeBirthSystem : GameSystemBase
    {
        public static readonly int kUpdatesPerDay = 16;
        private EndFrameBarrier m_EndFrameBarrier;
        private SimulationSystem m_SimulationSystem;
        private CityStatisticsSystem m_CityStatisticsSystem;
        private TriggerSystem m_TriggerSystem;
        [DebugWatchValue]
        private NativeValue<int> m_DebugBirth;
        private NativeCounter m_DebugBirthCounter;
        private EntityQuery m_CitizenQuery;
        private EntityQuery m_CitizenPrefabQuery;
        private EntityQuery m_CitizenParametersQuery;
        public int m_BirthChance = 20;
        private RealLifeBirthSystem.TypeHandle __TypeHandle;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 262144 / (RealLifeBirthSystem.kUpdatesPerDay * 16);
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            this.m_DebugBirthCounter = new NativeCounter(Allocator.Persistent);
            this.m_DebugBirth = new NativeValue<int>(Allocator.Persistent);
            this.m_EndFrameBarrier = this.World.GetOrCreateSystemManaged<EndFrameBarrier>();
            this.m_SimulationSystem = this.World.GetOrCreateSystemManaged<SimulationSystem>();
            this.m_CityStatisticsSystem = this.World.GetOrCreateSystemManaged<CityStatisticsSystem>();
            this.m_TriggerSystem = this.World.GetOrCreateSystemManaged<TriggerSystem>();
            this.m_CitizenQuery = this.GetEntityQuery(ComponentType.ReadOnly<Citizen>(), ComponentType.ReadOnly<HouseholdMember>(), ComponentType.ReadOnly<UpdateFrame>(), ComponentType.ReadOnly<CurrentBuilding>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Temp>());
            this.m_CitizenPrefabQuery = this.GetEntityQuery(ComponentType.ReadOnly<CitizenData>(), ComponentType.ReadOnly<Game.Prefabs.ArchetypeData>());
            this.m_CitizenParametersQuery = this.GetEntityQuery(ComponentType.ReadOnly<CitizenParametersData>());
            this.RequireForUpdate(this.m_CitizenPrefabQuery);
            this.RequireForUpdate(this.m_CitizenParametersQuery);
            this.RequireForUpdate(this.m_CitizenQuery);
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.m_DebugBirthCounter.Dispose();
            this.m_DebugBirth.Dispose();
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnUpdate()
        {
            uint updateFrame = SimulationUtils.GetUpdateFrame(this.m_SimulationSystem.frameIndex, RealLifeBirthSystem.kUpdatesPerDay, 16);
            this.__TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Student_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_HouseholdCitizen_RO_BufferLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Citizen_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Citizen_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref this.CheckedStateRef);
            JobHandle outJobHandle1;
            JobHandle outJobHandle2;
            JobHandle deps;

            RealLifeBirthSystem.CheckBirthJob jobData1 = new RealLifeBirthSystem.CheckBirthJob()
            {
                m_DebugBirthCounter = this.m_DebugBirthCounter.ToConcurrent(),
                m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle,
                m_CitizenType = this.__TypeHandle.__Game_Citizens_Citizen_RO_ComponentTypeHandle,
                m_MemberType = this.__TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentTypeHandle,
                m_UpdateFrameType = this.__TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle,
                m_Citizens = this.__TypeHandle.__Game_Citizens_Citizen_RO_ComponentLookup,
                m_HouseholdCitizens = this.__TypeHandle.__Game_Citizens_HouseholdCitizen_RO_BufferLookup,
                m_Students = this.__TypeHandle.__Game_Citizens_Student_RO_ComponentLookup,
                m_PropertyRenters = this.__TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup,
                m_CitizenPrefabArchetypes = this.m_CitizenPrefabQuery.ToComponentDataListAsync<Game.Prefabs.ArchetypeData>((AllocatorManager.AllocatorHandle)this.World.UpdateAllocator.ToAllocator, out outJobHandle1),
                m_CitizenPrefabs = this.m_CitizenPrefabQuery.ToEntityListAsync((AllocatorManager.AllocatorHandle)this.World.UpdateAllocator.ToAllocator, out outJobHandle2),
                m_CitizenParametersData = this.m_CitizenParametersQuery.GetSingleton<CitizenParametersData>(),
                m_RandomSeed = RandomSeed.Next(),
                m_UpdateFrameIndex = updateFrame,
                m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_StatisticsEventQueue = this.m_CityStatisticsSystem.GetStatisticsEventQueue(out deps).AsParallelWriter()
            };
            this.Dependency = jobData1.ScheduleParallel<RealLifeBirthSystem.CheckBirthJob>(this.m_CitizenQuery, JobUtils.CombineDependencies(this.Dependency, deps, outJobHandle2, outJobHandle1));
            this.m_EndFrameBarrier.AddJobHandleForProducer(this.Dependency);
            this.m_TriggerSystem.AddActionBufferWriter(this.Dependency);
            this.m_CityStatisticsSystem.AddWriter(this.Dependency);

            RealLifeBirthSystem.SumBirthJob jobData2 = new RealLifeBirthSystem.SumBirthJob()
            {
                m_DebugBirth = this.m_DebugBirth,
                m_DebugBirthCount = this.m_DebugBirthCounter
            };
            this.Dependency = jobData2.Schedule<RealLifeBirthSystem.SumBirthJob>(this.Dependency);
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
        public RealLifeBirthSystem()
        {
        }

        [BurstCompile]
        private struct CheckBirthJob : IJobChunk
        {
            public NativeCounter.Concurrent m_DebugBirthCounter;
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public ComponentTypeHandle<Citizen> m_CitizenType;
            [ReadOnly]
            public ComponentTypeHandle<HouseholdMember> m_MemberType;
            [ReadOnly]
            public SharedComponentTypeHandle<UpdateFrame> m_UpdateFrameType;
            [ReadOnly]
            public BufferLookup<HouseholdCitizen> m_HouseholdCitizens;
            [ReadOnly]
            public ComponentLookup<Citizen> m_Citizens;
            [ReadOnly]
            public ComponentLookup<Game.Citizens.Student> m_Students;
            [ReadOnly]
            public ComponentLookup<PropertyRenter> m_PropertyRenters;
            public uint m_UpdateFrameIndex;
            public RandomSeed m_RandomSeed;
            [ReadOnly]
            public CitizenParametersData m_CitizenParametersData;
            [ReadOnly]
            public NativeList<Entity> m_CitizenPrefabs;
            [ReadOnly]
            public NativeList<Game.Prefabs.ArchetypeData> m_CitizenPrefabArchetypes;
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            public NativeQueue<StatisticsEvent>.ParallelWriter m_StatisticsEventQueue;

            private Entity SpawnBaby(int index, Entity household, ref Random random, Entity building)
            {
                this.m_DebugBirthCounter.Increment();
                int index1 = random.NextInt(this.m_CitizenPrefabs.Length);
                Entity citizenPrefab = this.m_CitizenPrefabs[index1];
                Game.Prefabs.ArchetypeData citizenPrefabArchetype = this.m_CitizenPrefabArchetypes[index1];
                Entity entity = this.m_CommandBuffer.CreateEntity(index, citizenPrefabArchetype.m_Archetype);
                this.m_CommandBuffer.SetComponent<PrefabRef>(index, entity, new PrefabRef()
                {
                    m_Prefab = citizenPrefab
                });
                HouseholdMember component1 = new HouseholdMember()
                {
                    m_Household = household
                };
                this.m_CommandBuffer.AddComponent<HouseholdMember>(index, entity, component1);
                Citizen component2 = new Citizen()
                {
                    m_BirthDay = 0,
                    m_State = CitizenFlags.None
                };
                this.m_CommandBuffer.SetComponent<Citizen>(index, entity, component2);
                this.m_CommandBuffer.AddComponent<CurrentBuilding>(index, entity, new CurrentBuilding()
                {
                    m_CurrentBuilding = building
                });
                return entity;
            }

            public void Execute(
              in ArchetypeChunk chunk,
              int unfilteredChunkIndex,
              bool useEnabledMask,
              in v128 chunkEnabledMask)
            {
                if ((int)chunk.GetSharedComponent<UpdateFrame>(this.m_UpdateFrameType).m_Index != (int)this.m_UpdateFrameIndex)
                    return;
                NativeArray<Entity> nativeArray1 = chunk.GetNativeArray(this.m_EntityType);
                NativeArray<Citizen> nativeArray2 = chunk.GetNativeArray<Citizen>(ref this.m_CitizenType);
                NativeArray<HouseholdMember> nativeArray3 = chunk.GetNativeArray<HouseholdMember>(ref this.m_MemberType);
                Random random = this.m_RandomSeed.GetRandom(unfilteredChunkIndex);
                for (int index1 = 0; index1 < nativeArray1.Length; ++index1)
                {
                    Entity entity1 = nativeArray1[index1];
                    Citizen citizen1 = nativeArray2[index1];
                    if (citizen1.GetAge() == CitizenAge.Adult && (citizen1.m_State & (CitizenFlags.Male | CitizenFlags.Tourist | CitizenFlags.Commuter)) == CitizenFlags.None)
                    {
                        Entity household = nativeArray3[index1].m_Household;
                        Entity property = Entity.Null;
                        if (this.m_PropertyRenters.HasComponent(household))
                        {
                            property = this.m_PropertyRenters[household].m_Property;
                        }
                        if (!(property == Entity.Null))
                        {
                            DynamicBuffer<HouseholdCitizen> householdCitizen = this.m_HouseholdCitizens[household];
                            Entity entity2 = Entity.Null;
                            float baseBirthRate = this.m_CitizenParametersData.m_BaseBirthRate;
                            for (int index2 = 0; index2 < householdCitizen.Length; ++index2)
                            {
                                Entity citizen2 = householdCitizen[index2].m_Citizen;
                                if (this.m_Citizens.HasComponent(citizen2))
                                {
                                    Citizen citizen3 = this.m_Citizens[citizen2];
                                    if ((citizen3.m_State & CitizenFlags.Male) != CitizenFlags.None && citizen3.GetAge() == CitizenAge.Adult)
                                    {
                                        baseBirthRate += this.m_CitizenParametersData.m_AdultFemaleBirthRateBonus;
                                        break;
                                    }
                                }
                            }
                            if (this.m_Students.HasComponent(entity1))
                            {
                                baseBirthRate *= this.m_CitizenParametersData.m_StudentBirthRateAdjust;
                            }
                            if ((double)random.NextFloat(1f) < (double)baseBirthRate / (double)RealLifeBirthSystem.kUpdatesPerDay)
                            {
                                this.SpawnBaby(unfilteredChunkIndex, household, ref random, property);
                                this.m_StatisticsEventQueue.Enqueue(new StatisticsEvent()
                                {
                                    m_Statistic = StatisticType.BirthRate,
                                    m_Change = 1f
                                });
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

        [BurstCompile]
        private struct SumBirthJob : IJob
        {
            public NativeCounter m_DebugBirthCount;
            public NativeValue<int> m_DebugBirth;

            public void Execute() => this.m_DebugBirth.value = this.m_DebugBirthCount.Count;
        }

        private struct TypeHandle
        {
            [ReadOnly]
            public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<Citizen> __Game_Citizens_Citizen_RO_ComponentTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<HouseholdMember> __Game_Citizens_HouseholdMember_RO_ComponentTypeHandle;
            public SharedComponentTypeHandle<UpdateFrame> __Game_Simulation_UpdateFrame_SharedComponentTypeHandle;
            [ReadOnly]
            public ComponentLookup<Citizen> __Game_Citizens_Citizen_RO_ComponentLookup;
            [ReadOnly]
            public BufferLookup<HouseholdCitizen> __Game_Citizens_HouseholdCitizen_RO_BufferLookup;
            [ReadOnly]
            public ComponentLookup<Game.Citizens.Student> __Game_Citizens_Student_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentLookup;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                this.__Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                this.__Game_Citizens_Citizen_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Citizen>(true);
                this.__Game_Citizens_HouseholdMember_RO_ComponentTypeHandle = state.GetComponentTypeHandle<HouseholdMember>(true);
                this.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle = state.GetSharedComponentTypeHandle<UpdateFrame>();
                this.__Game_Citizens_Citizen_RO_ComponentLookup = state.GetComponentLookup<Citizen>(true);
                this.__Game_Citizens_HouseholdCitizen_RO_BufferLookup = state.GetBufferLookup<HouseholdCitizen>(true);
                this.__Game_Citizens_Student_RO_ComponentLookup = state.GetComponentLookup<Game.Citizens.Student>(true);
                this.__Game_Buildings_PropertyRenter_RO_ComponentLookup = state.GetComponentLookup<PropertyRenter>(true);
            }
        }
    }
}
