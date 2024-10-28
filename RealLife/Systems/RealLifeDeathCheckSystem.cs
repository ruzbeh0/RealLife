
using Colossal;
using Colossal.Entities;
using Colossal.Mathematics;
using Game.Achievements;
using Game.Buildings;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Companies;
using Game.Creatures;
using Game.Notifications;
using Game.Prefabs;
using Game.Tools;
using Game.Triggers;
using Game.Simulation;
using Game;
using RealLife;
using RealLife.Utils;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System;

#nullable disable
namespace RealLife.Systems
{
    public partial class RealLifeDeathCheckSystem : GameSystemBase
    {
        public static readonly int kUpdatesPerDay = 4;
        private SimulationSystem m_SimulationSystem;
        private IconCommandSystem m_IconCommandSystem;
        private CitySystem m_CitySystem;
        private CityStatisticsSystem m_CityStatisticsSystem;
        private EndFrameBarrier m_EndFrameBarrier;
        private EntityQuery m_DeathCheckQuery;
        private EntityQuery m_HealthcareSettingsQuery;
        private EntityQuery m_TimeSettingsQuery;
        private EntityQuery m_TimeDataQuery;
        private TriggerSystem m_TriggerSystem;
        private AchievementTriggerSystem m_AchievementTriggerSystem;
        private RealLifeDeathCheckSystem.TypeHandle __TypeHandle;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 262144 / (RealLifeDeathCheckSystem.kUpdatesPerDay * 16);
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            this.m_SimulationSystem = this.World.GetOrCreateSystemManaged<SimulationSystem>();
            this.m_IconCommandSystem = this.World.GetOrCreateSystemManaged<IconCommandSystem>();
            this.m_CitySystem = this.World.GetOrCreateSystemManaged<CitySystem>();
            this.m_CityStatisticsSystem = this.World.GetOrCreateSystemManaged<CityStatisticsSystem>();
            this.m_EndFrameBarrier = this.World.GetOrCreateSystemManaged<EndFrameBarrier>();
            this.m_TriggerSystem = this.World.GetOrCreateSystemManaged<TriggerSystem>();
            this.m_AchievementTriggerSystem = this.World.GetOrCreateSystemManaged<AchievementTriggerSystem>();
            this.m_DeathCheckQuery = this.GetEntityQuery(ComponentType.ReadOnly<Citizen>(), ComponentType.ReadOnly<UpdateFrame>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Temp>());
            this.m_HealthcareSettingsQuery = this.GetEntityQuery(ComponentType.ReadOnly<HealthcareParameterData>());
            this.m_TimeSettingsQuery = this.GetEntityQuery(ComponentType.ReadOnly<TimeSettingsData>());
            this.m_TimeDataQuery = this.GetEntityQuery(ComponentType.ReadOnly<TimeData>());
            this.RequireForUpdate(this.m_DeathCheckQuery);
            this.RequireForUpdate(this.m_HealthcareSettingsQuery);
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnUpdate()
        {
            if (this.EntityManager.HasEnabledComponent<Locked>(this.m_HealthcareSettingsQuery.GetSingleton<HealthcareParameterData>().m_HealthcareServicePrefab))
                return;
            uint updateFrame = SimulationUtils.GetUpdateFrame(this.m_SimulationSystem.frameIndex, RealLifeDeathCheckSystem.kUpdatesPerDay, 16);
            this.__TypeHandle.__Game_Citizens_HouseholdCitizen_RO_BufferLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_Student_RO_BufferLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_City_CityModifier_RO_BufferLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Creatures_Resident_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_CurrentTransport_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_Building_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_Hospital_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_CurrentBuilding_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Companies_ResourceBuyer_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Leisure_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_HealthProblem_RW_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Worker_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Citizen_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref this.CheckedStateRef);
            JobHandle deps;

            RealLifeDeathCheckSystem.DeathCheckJob jobData = new RealLifeDeathCheckSystem.DeathCheckJob()
            {
                m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle,
                m_CitizenType = this.__TypeHandle.__Game_Citizens_Citizen_RO_ComponentTypeHandle,
                m_WorkerType = this.__TypeHandle.__Game_Citizens_Worker_RO_ComponentTypeHandle,
                m_HouseholdMemberType = this.__TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentTypeHandle,
                m_HealthProblemType = this.__TypeHandle.__Game_Citizens_HealthProblem_RW_ComponentTypeHandle,
                m_UpdateFrameType = this.__TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle,
                m_LeisureType = this.__TypeHandle.__Game_Citizens_Leisure_RO_ComponentTypeHandle,
                m_ResourceBuyerType = this.__TypeHandle.__Game_Companies_ResourceBuyer_RO_ComponentTypeHandle,
                m_StudentType = this.__TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle,
                m_CurrentBuildings = this.__TypeHandle.__Game_Citizens_CurrentBuilding_RO_ComponentLookup,
                m_HospitalData = this.__TypeHandle.__Game_Buildings_Hospital_RO_ComponentLookup,
                m_BuildingData = this.__TypeHandle.__Game_Buildings_Building_RO_ComponentLookup,
                m_CurrentTransport = this.__TypeHandle.__Game_Citizens_CurrentTransport_RO_ComponentLookup,
                m_ResidentData = this.__TypeHandle.__Game_Creatures_Resident_RO_ComponentLookup,
                m_CityModifiers = this.__TypeHandle.__Game_City_CityModifier_RO_BufferLookup,
                m_Students = this.__TypeHandle.__Game_Buildings_Student_RO_BufferLookup,
                m_HouseholdCitizens = this.__TypeHandle.__Game_Citizens_HouseholdCitizen_RO_BufferLookup,
                m_UpdateFrameIndex = updateFrame,
                m_RandomSeed = RandomSeed.Next(),
                m_HealthcareParameterData = this.m_HealthcareSettingsQuery.GetSingleton<HealthcareParameterData>(),
                m_City = this.m_CitySystem.City,
                m_TriggerBuffer = this.m_TriggerSystem.CreateActionBuffer().AsParallelWriter(),
                m_StatisticsEventQueue = this.m_CityStatisticsSystem.GetStatisticsEventQueue(out deps).AsParallelWriter(),
                m_IconCommandBuffer = this.m_IconCommandSystem.CreateCommandBuffer(),
                m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_PatientsRecoveredCounter = this.m_AchievementTriggerSystem.m_PatientsTreatedCounter.ToConcurrent(),
                m_SimulationFrame = this.m_SimulationSystem.frameIndex,
                m_TimeSettings = this.m_TimeSettingsQuery.GetSingleton<TimeSettingsData>(),
                m_TimeData = this.m_TimeDataQuery.GetSingleton<TimeData>(),
                male_life_expectancy = Mod.m_Setting.male_life_expectancy,
                female_life_expectancy = Mod.m_Setting.female_life_expectancy,
                corpse_vanish = Mod.m_Setting.corpse_vanish
            };
            this.Dependency = jobData.ScheduleParallel<RealLifeDeathCheckSystem.DeathCheckJob>(this.m_DeathCheckQuery, JobHandle.CombineDependencies(this.Dependency, deps));
            this.m_IconCommandSystem.AddCommandBufferWriter(this.Dependency);
            this.m_EndFrameBarrier.AddJobHandleForProducer(this.Dependency);
            this.m_CityStatisticsSystem.AddWriter(this.Dependency);
            this.m_TriggerSystem.AddActionBufferWriter(this.Dependency);
        }

        public static void PerformAfterDeathActions(
          Entity citizen,
          Entity household,
          NativeQueue<TriggerAction>.ParallelWriter triggerBuffer,
          NativeQueue<StatisticsEvent>.ParallelWriter statisticsEventQueue,
          ref BufferLookup<HouseholdCitizen> householdCitizens)
        {
            triggerBuffer.Enqueue(new TriggerAction(TriggerType.CitizenDied, Entity.Null, citizen, Entity.Null));
            DynamicBuffer<HouseholdCitizen> bufferData;
            if (household != Entity.Null && householdCitizens.TryGetBuffer(household, out bufferData))
            {
                for (int index = 0; index < bufferData.Length; ++index)
                {
                    if (bufferData[index].m_Citizen != citizen)
                        triggerBuffer.Enqueue(new TriggerAction(TriggerType.CitizensFamilyMemberDied, Entity.Null, bufferData[index].m_Citizen, citizen));
                }
            }
            statisticsEventQueue.Enqueue(new StatisticsEvent()
            {
                m_Statistic = StatisticType.DeathRate,
                m_Change = 1f
            });
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
        public RealLifeDeathCheckSystem()
        {
        }

        //[BurstCompile]
        private struct DeathCheckJob : IJobChunk
        {
            [ReadOnly]
            public SharedComponentTypeHandle<UpdateFrame> m_UpdateFrameType;
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public ComponentTypeHandle<Citizen> m_CitizenType;
            [ReadOnly]
            public ComponentTypeHandle<Worker> m_WorkerType;
            [ReadOnly]
            public ComponentTypeHandle<HouseholdMember> m_HouseholdMemberType;
            [ReadOnly]
            public ComponentTypeHandle<Game.Citizens.Student> m_StudentType;
            [ReadOnly]
            public ComponentTypeHandle<ResourceBuyer> m_ResourceBuyerType;
            [ReadOnly]
            public ComponentTypeHandle<Leisure> m_LeisureType;
            public ComponentTypeHandle<HealthProblem> m_HealthProblemType;
            [ReadOnly]
            public ComponentLookup<CurrentBuilding> m_CurrentBuildings;
            [ReadOnly]
            public ComponentLookup<Game.Buildings.Hospital> m_HospitalData;
            [ReadOnly]
            public ComponentLookup<Building> m_BuildingData;
            [ReadOnly]
            public ComponentLookup<Game.Creatures.Resident> m_ResidentData;
            [ReadOnly]
            public ComponentLookup<CurrentTransport> m_CurrentTransport;
            [ReadOnly]
            public BufferLookup<CityModifier> m_CityModifiers;
            [ReadOnly]
            public BufferLookup<Game.Buildings.Student> m_Students;
            [ReadOnly]
            public BufferLookup<HouseholdCitizen> m_HouseholdCitizens;
            [ReadOnly]
            public uint m_UpdateFrameIndex;
            [ReadOnly]
            public RandomSeed m_RandomSeed;
            [ReadOnly]
            public HealthcareParameterData m_HealthcareParameterData;
            [ReadOnly]
            public Entity m_City;
            public NativeQueue<StatisticsEvent>.ParallelWriter m_StatisticsEventQueue;
            public NativeQueue<TriggerAction>.ParallelWriter m_TriggerBuffer;
            public IconCommandBuffer m_IconCommandBuffer;
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            public NativeCounter.Concurrent m_PatientsRecoveredCounter;
            public TimeSettingsData m_TimeSettings;
            public TimeData m_TimeData;
            public uint m_SimulationFrame;
            public int female_life_expectancy;
            public int male_life_expectancy;
            public int corpse_vanish;

            private void Die(
              ArchetypeChunk chunk,
              int chunkIndex,
              int i,
              Entity citizen,
              Entity household,
              NativeArray<Game.Citizens.Student> students,
              NativeArray<HealthProblem> healthProblems,
              bool isVanishCorpse)
            {
                //Vanishing corpse logic from Infixo's Pop Rebalance Mod
                if (!healthProblems.IsCreated)
                {
                    HealthProblem component = new HealthProblem()
                    {
                        m_Flags = HealthProblemFlags.Dead | HealthProblemFlags.RequireTransport
                    };
                    if (isVanishCorpse)
                        component.m_Flags &= ~HealthProblemFlags.RequireTransport;
                    this.m_CommandBuffer.AddComponent<HealthProblem>(chunkIndex, citizen, component);
                }
                else
                {
                    HealthProblem healthProblem = healthProblems[i];
                    if ((healthProblem.m_Flags & HealthProblemFlags.RequireTransport) != HealthProblemFlags.None)
                    {
                        this.m_IconCommandBuffer.Remove(citizen, this.m_HealthcareParameterData.m_AmbulanceNotificationPrefab);
                        healthProblem.m_Timer = (byte)0;
                    }
                    healthProblem.m_Flags &= ~(HealthProblemFlags.Sick | HealthProblemFlags.Injured);
                    healthProblem.m_Flags |= HealthProblemFlags.Dead | HealthProblemFlags.RequireTransport;
                    if (isVanishCorpse)
                        healthProblem.m_Flags &= ~HealthProblemFlags.RequireTransport;
                    healthProblems[i] = healthProblem;
                }

                RealLifeDeathCheckSystem.PerformAfterDeathActions(citizen, household, this.m_TriggerBuffer, this.m_StatisticsEventQueue, ref this.m_HouseholdCitizens);

                if (chunk.Has<Game.Citizens.Student>(ref this.m_StudentType))
                {
                    Entity school = students[i].m_School;
                    if (this.m_Students.HasBuffer(school))
                    {
                        this.m_CommandBuffer.AddComponent<StudentsRemoved>(chunkIndex, school);
                    }
                    this.m_CommandBuffer.RemoveComponent<Game.Citizens.Student>(chunkIndex, citizen);
                }
                if (chunk.Has<Worker>(ref this.m_WorkerType))
                {
                    this.m_CommandBuffer.RemoveComponent<Worker>(chunkIndex, citizen);
                }
                if (chunk.Has<ResourceBuyer>(ref this.m_ResourceBuyerType))
                {
                    this.m_CommandBuffer.RemoveComponent<ResourceBuyer>(chunkIndex, citizen);
                }
                if (isVanishCorpse)
                {
                    m_CommandBuffer.AddComponent(chunkIndex, citizen, default(Deleted));
                }
                if (!chunk.Has<Leisure>(ref this.m_LeisureType))
                    return;
                this.m_CommandBuffer.RemoveComponent<Leisure>(chunkIndex, citizen);
            }

            public void Execute(
              in ArchetypeChunk chunk,
              int unfilteredChunkIndex,
              bool useEnabledMask,
              in v128 chunkEnabledMask)
            {
                if ((int)chunk.GetSharedComponent<UpdateFrame>(this.m_UpdateFrameType).m_Index != (int)this.m_UpdateFrameIndex)
                    return;
                Unity.Mathematics.Random random = this.m_RandomSeed.GetRandom(unfilteredChunkIndex);
                DynamicBuffer<CityModifier> cityModifier = this.m_CityModifiers[this.m_City];
                NativeArray<Entity> nativeArray1 = chunk.GetNativeArray(this.m_EntityType);
                NativeArray<HealthProblem> nativeArray2 = chunk.GetNativeArray<HealthProblem>(ref this.m_HealthProblemType);
                NativeArray<Citizen> nativeArray3 = chunk.GetNativeArray<Citizen>(ref this.m_CitizenType);
                NativeArray<Game.Citizens.Student> nativeArray4 = chunk.GetNativeArray<Game.Citizens.Student>(ref this.m_StudentType);
                NativeArray<HouseholdMember> nativeArray5 = chunk.GetNativeArray<HouseholdMember>(ref this.m_HouseholdMemberType);
                for (int index = 0; index < chunk.Count; ++index)
                {
                    Entity entity = nativeArray1[index];
                    Entity household = nativeArray5.Length != 0 ? nativeArray5[index].m_Household : Entity.Null;
                    Citizen citizen = nativeArray3[index];

                    bool isVanishCorpse = random.NextInt(100) < corpse_vanish;

                    if (this.m_CurrentTransport.HasComponent(entity))
                    {
                        CurrentTransport currentTransport = this.m_CurrentTransport[entity];
                        if (this.m_ResidentData.HasComponent(currentTransport.m_CurrentTransport) && (this.m_ResidentData[currentTransport.m_CurrentTransport].m_Flags & ResidentFlags.InVehicle) != ResidentFlags.None)
                            continue;
                    }
                    if (!nativeArray2.IsCreated || (nativeArray2[index].m_Flags & HealthProblemFlags.Dead) == HealthProblemFlags.None)
                    {
                        //Citizen can die based on average life expectancy
                        int age = TimeSystem.GetDay(m_SimulationFrame, m_TimeData) - citizen.m_BirthDay;
                        int lifeExpectancy = female_life_expectancy;
                        if((citizen.m_State & CitizenFlags.Male) != CitizenFlags.None)
                        {
                            lifeExpectancy = male_life_expectancy;
                        }
                        double extraYears = GaussianRandom.NextGaussianDouble(random);
                        if(extraYears < 0)
                        {
                            extraYears *= 5;
                        } else
                        {
                            extraYears *= 15;
                        }
                        lifeExpectancy +=  (int)extraYears;

                        //Checking death with a probability of 1/kUpdatesPerDay*2
                        int check = random.NextInt(kUpdatesPerDay*2);
                        if(check < 1)
                        {
                            if (age > lifeExpectancy)
                            {
                                this.Die(chunk, unfilteredChunkIndex, index, entity, household, nativeArray4, nativeArray2, isVanishCorpse);
                            }
                        }
                        
                        else if (nativeArray2.IsCreated && (nativeArray2[index].m_Flags & (HealthProblemFlags.Sick | HealthProblemFlags.Injured)) != HealthProblemFlags.None)
                        {
                            HealthProblem healthProblem = nativeArray2[index];
                            int health = (int)citizen.m_Health;
                            int num1 = 10 - health / 10;
                            int num2 = num1 * num1 + 8;
                            if (random.NextInt(RealLifeDeathCheckSystem.kUpdatesPerDay * 1000) <= num2)
                            {
                                this.Die(chunk, unfilteredChunkIndex, index, entity, household, nativeArray4, nativeArray2, isVanishCorpse);
                            }
                            else
                            {
                                float num3 = MathUtils.Logistic(3f, 1000f, 6f, (float)((double)num1 / 10.0 - 0.34999999403953552));
                                int num4 = 0;
                                if (this.m_CurrentBuildings.HasComponent(entity))
                                {
                                    Entity currentBuilding = this.m_CurrentBuildings[entity].m_CurrentBuilding;
                                    if (this.m_BuildingData.HasComponent(currentBuilding) && !BuildingUtils.CheckOption(this.m_BuildingData[currentBuilding], BuildingOption.Inactive) && this.m_HospitalData.HasComponent(currentBuilding))
                                    {
                                        num4 = (int)this.m_HospitalData[currentBuilding].m_TreatmentBonus;
                                    }
                                }
                                num3 -= (float)(10 * num4);
                                CityUtils.ApplyModifier(ref num3, cityModifier, CityModifierType.RecoveryFailChange);
                                if ((double)random.NextFloat(1000f) >= (double)num3)
                                {
                                    if ((healthProblem.m_Flags & HealthProblemFlags.RequireTransport) != HealthProblemFlags.None)
                                    {
                                        this.m_IconCommandBuffer.Remove(entity, this.m_HealthcareParameterData.m_AmbulanceNotificationPrefab);
                                        healthProblem.m_Timer = (byte)0;
                                    }
                                    healthProblem.m_Flags &= ~(HealthProblemFlags.Sick | HealthProblemFlags.Injured | HealthProblemFlags.RequireTransport);
                                    nativeArray2[index] = healthProblem;
                                    this.m_PatientsRecoveredCounter.Increment();
                                }
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
            [ReadOnly]
            public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<Citizen> __Game_Citizens_Citizen_RO_ComponentTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<Worker> __Game_Citizens_Worker_RO_ComponentTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<HouseholdMember> __Game_Citizens_HouseholdMember_RO_ComponentTypeHandle;
            public ComponentTypeHandle<HealthProblem> __Game_Citizens_HealthProblem_RW_ComponentTypeHandle;
            public SharedComponentTypeHandle<UpdateFrame> __Game_Simulation_UpdateFrame_SharedComponentTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<Leisure> __Game_Citizens_Leisure_RO_ComponentTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<ResourceBuyer> __Game_Companies_ResourceBuyer_RO_ComponentTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<Game.Citizens.Student> __Game_Citizens_Student_RO_ComponentTypeHandle;
            [ReadOnly]
            public ComponentLookup<CurrentBuilding> __Game_Citizens_CurrentBuilding_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<Game.Buildings.Hospital> __Game_Buildings_Hospital_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<Building> __Game_Buildings_Building_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<CurrentTransport> __Game_Citizens_CurrentTransport_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<Game.Creatures.Resident> __Game_Creatures_Resident_RO_ComponentLookup;
            [ReadOnly]
            public BufferLookup<CityModifier> __Game_City_CityModifier_RO_BufferLookup;
            [ReadOnly]
            public BufferLookup<Game.Buildings.Student> __Game_Buildings_Student_RO_BufferLookup;
            [ReadOnly]
            public BufferLookup<HouseholdCitizen> __Game_Citizens_HouseholdCitizen_RO_BufferLookup;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                this.__Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                this.__Game_Citizens_Citizen_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Citizen>(true);
                this.__Game_Citizens_Worker_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Worker>(true);
                this.__Game_Citizens_HouseholdMember_RO_ComponentTypeHandle = state.GetComponentTypeHandle<HouseholdMember>(true);
                this.__Game_Citizens_HealthProblem_RW_ComponentTypeHandle = state.GetComponentTypeHandle<HealthProblem>();
                this.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle = state.GetSharedComponentTypeHandle<UpdateFrame>();
                this.__Game_Citizens_Leisure_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Leisure>(true);
                this.__Game_Companies_ResourceBuyer_RO_ComponentTypeHandle = state.GetComponentTypeHandle<ResourceBuyer>(true);
                this.__Game_Citizens_Student_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Citizens.Student>(true);
                this.__Game_Citizens_CurrentBuilding_RO_ComponentLookup = state.GetComponentLookup<CurrentBuilding>(true);
                this.__Game_Buildings_Hospital_RO_ComponentLookup = state.GetComponentLookup<Game.Buildings.Hospital>(true);
                this.__Game_Buildings_Building_RO_ComponentLookup = state.GetComponentLookup<Building>(true);
                this.__Game_Citizens_CurrentTransport_RO_ComponentLookup = state.GetComponentLookup<CurrentTransport>(true);
                this.__Game_Creatures_Resident_RO_ComponentLookup = state.GetComponentLookup<Game.Creatures.Resident>(true);
                this.__Game_City_CityModifier_RO_BufferLookup = state.GetBufferLookup<CityModifier>(true);
                this.__Game_Buildings_Student_RO_BufferLookup = state.GetBufferLookup<Game.Buildings.Student>(true);
                this.__Game_Citizens_HouseholdCitizen_RO_BufferLookup = state.GetBufferLookup<HouseholdCitizen>(true);
            }
        }
    }
}
