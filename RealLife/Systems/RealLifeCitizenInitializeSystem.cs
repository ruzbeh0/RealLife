
using Game;
using Game.Common;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Game.Triggers;
using Game.Citizens;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Game.Companies;
using RealLife.Utils;

#nullable disable
namespace RealLife.Systems
{
    public partial class RealLifeCitizenInitializeSystem : GameSystemBase
    {
        private EntityQuery m_Additions;
        private EntityQuery m_TimeSettingGroup;
        private EntityQuery m_CitizenPrefabs;
        private EntityQuery m_TimeDataQuery;
        private EntityQuery m_DemandParameterQuery;
        private SimulationSystem m_SimulationSystem;
        private TriggerSystem m_TriggerSystem;
        private ModificationBarrier5 m_EndFrameBarrier;
        private RealLifeCitizenInitializeSystem.TypeHandle __TypeHandle;
        private CountWorkplacesSystem m_CountWorkplacesSystem;

        [UnityEngine.Scripting.Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            this.m_SimulationSystem = this.World.GetOrCreateSystemManaged<SimulationSystem>();
            this.m_TriggerSystem = this.World.GetOrCreateSystemManaged<TriggerSystem>();
            this.m_EndFrameBarrier = this.World.GetOrCreateSystemManaged<ModificationBarrier5>();
            this.m_Additions = this.GetEntityQuery(ComponentType.ReadWrite<Citizen>(), ComponentType.ReadWrite<HouseholdMember>(), ComponentType.ReadOnly<Created>(), ComponentType.Exclude<Temp>());
            this.m_CitizenPrefabs = this.GetEntityQuery(ComponentType.ReadOnly<CitizenData>());
            this.m_TimeSettingGroup = this.GetEntityQuery(ComponentType.ReadOnly<TimeSettingsData>());
            this.m_TimeDataQuery = this.GetEntityQuery(ComponentType.ReadOnly<TimeData>());
            this.m_DemandParameterQuery = this.GetEntityQuery(ComponentType.ReadOnly<DemandParameterData>());
            this.RequireForUpdate(this.m_Additions);
            this.RequireForUpdate(this.m_TimeDataQuery);
            this.RequireForUpdate(this.m_TimeSettingGroup);
            this.RequireForUpdate(this.m_DemandParameterQuery);
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnUpdate()
        {
            this.__TypeHandle.__Game_Prefabs_CitizenData_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_CrimeVictim_RW_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_MailSender_RW_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_CarKeeper_RW_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Arrived_RW_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_HouseholdCitizen_RW_BufferLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Citizen_RW_ComponentLookup.Update(ref this.CheckedStateRef);
            JobHandle outJobHandle;

            this.m_CountWorkplacesSystem = this.World.GetOrCreateSystemManaged<CountWorkplacesSystem>();

            RealLifeCitizenInitializeSystem.InitializeCitizenJob jobData = new RealLifeCitizenInitializeSystem.InitializeCitizenJob()
            {
                m_Entities = this.m_Additions.ToEntityArray((AllocatorManager.AllocatorHandle)Allocator.TempJob),
                m_HouseholdMembers = this.m_Additions.ToComponentDataListAsync<HouseholdMember>((AllocatorManager.AllocatorHandle)this.World.UpdateAllocator.ToAllocator, out outJobHandle),
                m_CitizenPrefabs = this.m_CitizenPrefabs.ToEntityArray((AllocatorManager.AllocatorHandle)Allocator.TempJob),
                m_Citizens = this.__TypeHandle.__Game_Citizens_Citizen_RW_ComponentLookup,
                m_HouseholdCitizens = this.__TypeHandle.__Game_Citizens_HouseholdCitizen_RW_BufferLookup,
                m_Arriveds = this.__TypeHandle.__Game_Citizens_Arrived_RW_ComponentLookup,
                m_CarKeepers = this.__TypeHandle.__Game_Citizens_CarKeeper_RW_ComponentLookup,
                m_MailSenders = this.__TypeHandle.__Game_Citizens_MailSender_RW_ComponentLookup,
                m_CrimeVictims = this.__TypeHandle.__Game_Citizens_CrimeVictim_RW_ComponentLookup,
                m_CitizenDatas = this.__TypeHandle.__Game_Prefabs_CitizenData_RO_ComponentLookup,
                m_DemandParameters = this.m_DemandParameterQuery.GetSingleton<DemandParameterData>(),
                m_TimeSettings = this.m_TimeSettingGroup.GetSingleton<TimeSettingsData>(),
                m_TimeData = this.m_TimeDataQuery.GetSingleton<TimeData>(),
                m_SimulationFrame = this.m_SimulationSystem.frameIndex,
                m_RandomSeed = RandomSeed.Next(),
                m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer(),
                m_TriggerBuffer = this.m_TriggerSystem.CreateActionBuffer(),
                child_age_limit = Mod.m_Setting.child_age_limit,
                teen_age_limit = Mod.m_Setting.teen_age_limit,
                adult_age_limit = Mod.m_Setting.adult_age_limit,
                female_life_expectancy = Mod.m_Setting.female_life_expectancy,
                male_life_expectancy = Mod.m_Setting.male_life_expectancy,
                elementary_grad_prob = Mod.m_Setting.elementary_grad_prob,
                m_FreeWorkplaces = this.m_CountWorkplacesSystem.GetFreeWorkplaces()
            };
            this.Dependency = jobData.Schedule<RealLifeCitizenInitializeSystem.InitializeCitizenJob>(JobHandle.CombineDependencies(this.Dependency, outJobHandle));
            this.m_EndFrameBarrier.AddJobHandleForProducer(this.Dependency);
        }

        public static Entity GetPrefab(
          NativeArray<Entity> citizenPrefabs,
          Citizen citizen,
          ComponentLookup<CitizenData> citizenDatas,
          Random rnd)
        {
            int max = 0;
            for (int index = 0; index < citizenPrefabs.Length; ++index)
            {
                CitizenData citizenData = citizenDatas[citizenPrefabs[index]];
                if ((citizen.m_State & CitizenFlags.Male) == CitizenFlags.None ^ citizenData.m_Male)
                    ++max;
            }
            if (max > 0)
            {
                int num = rnd.NextInt(max);
                for (int index = 0; index < citizenPrefabs.Length; ++index)
                {
                    CitizenData citizenData = citizenDatas[citizenPrefabs[index]];
                    if ((citizen.m_State & CitizenFlags.Male) == CitizenFlags.None ^ citizenData.m_Male)
                    {
                        --num;
                        if (num < 0)
                            return new PrefabRef()
                            {
                                m_Prefab = citizenPrefabs[index]
                            }.m_Prefab;
                    }
                }
            }
            return Entity.Null;
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
        public RealLifeCitizenInitializeSystem()
        {
        }

        [BurstCompile]
        private struct InitializeCitizenJob : IJob
        {
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<Entity> m_Entities;
            [ReadOnly]
            public NativeList<HouseholdMember> m_HouseholdMembers;
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<Entity> m_CitizenPrefabs;
            public BufferLookup<HouseholdCitizen> m_HouseholdCitizens;
            public ComponentLookup<Citizen> m_Citizens;
            public ComponentLookup<Arrived> m_Arriveds;
            public ComponentLookup<CrimeVictim> m_CrimeVictims;
            public ComponentLookup<CarKeeper> m_CarKeepers;
            public ComponentLookup<MailSender> m_MailSenders;
            [ReadOnly]
            public ComponentLookup<CitizenData> m_CitizenDatas;
            public NativeQueue<TriggerAction> m_TriggerBuffer;
            [ReadOnly]
            public RandomSeed m_RandomSeed;
            public uint m_SimulationFrame;
            [DeallocateOnJobCompletion]
            public TimeData m_TimeData;
            public DemandParameterData m_DemandParameters;
            public TimeSettingsData m_TimeSettings;
            public EntityCommandBuffer m_CommandBuffer;
            public int child_age_limit;
            public int teen_age_limit;
            public int adult_age_limit;
            public int female_life_expectancy;
            public int male_life_expectancy;
            public int elementary_grad_prob;
            [ReadOnly]
            public Workplaces m_FreeWorkplaces;

            public void Execute()
            {
                int daysPerYear = this.m_TimeSettings.m_DaysPerYear;
                Random random = this.m_RandomSeed.GetRandom(0);

                for (int index1 = 0; index1 < this.m_Entities.Length; ++index1)
                {
                    Entity entity1 = this.m_Entities[index1];
                    this.m_Arriveds.SetComponentEnabled(entity1, false);
                    this.m_MailSenders.SetComponentEnabled(entity1, false);
                    this.m_CrimeVictims.SetComponentEnabled(entity1, false);
                    this.m_CarKeepers.SetComponentEnabled(entity1, false);
                    Citizen citizen1 = this.m_Citizens[entity1];
                    Entity household = this.m_HouseholdMembers[index1].m_Household;
                    bool flag = (citizen1.m_State & CitizenFlags.Commuter) != 0;
                    int num1 = (citizen1.m_State & CitizenFlags.Tourist) != 0 ? 1 : 0;
                    citizen1.m_PseudoRandom = (ushort)(random.NextUInt() % 65536U);
                    citizen1.m_Health = (byte)(40 + random.NextInt(20));
                    citizen1.m_WellBeing = (byte)(40 + random.NextInt(20));
                    citizen1.m_LeisureCounter = num1 == 0 ? (byte)(random.NextInt(92) + 128) : (byte)random.NextInt(128);
                    if (random.NextBool())
                        citizen1.m_State |= CitizenFlags.Male;
                    Entity prefab = RealLifeCitizenInitializeSystem.GetPrefab(this.m_CitizenPrefabs, citizen1, this.m_CitizenDatas, random);
                    this.m_CommandBuffer.AddComponent<PrefabRef>(entity1, new PrefabRef()
                    {
                        m_Prefab = prefab
                    });

                    DynamicBuffer<HouseholdCitizen> householdCitizen = this.m_HouseholdCitizens[household];
                    householdCitizen.Add(new HouseholdCitizen()
                    {
                        m_Citizen = entity1
                    });
                    int num2 = 0;
                    int2 int2 = int2.zero;
                    if (citizen1.m_BirthDay == (short)0)
                    {
                        citizen1.SetAge(CitizenAge.Child);
                        Entity primaryTarget = Entity.Null;
                        Entity entity2 = Entity.Null;
                        for (int index2 = 0; index2 < householdCitizen.Length; ++index2)
                        {
                            Entity citizen2 = householdCitizen[index2].m_Citizen;

                            if (this.m_Citizens.HasComponent(citizen2) && this.m_Citizens[citizen2].GetAge() == CitizenAge.Adult)
                            {
                                if (primaryTarget == Entity.Null)
                                    primaryTarget = citizen2;
                                else
                                    entity2 = citizen2;
                            }
                        }
                        if (primaryTarget != Entity.Null)
                        {
                            if (entity2 != Entity.Null)
                            {
                                this.m_TriggerBuffer.Enqueue(new TriggerAction(TriggerType.CitizenCoupleMadeBaby, Entity.Null, primaryTarget, entity1));
                            }
                            else
                            {
                                this.m_TriggerBuffer.Enqueue(new TriggerAction(TriggerType.CitizenSingleMadeBaby, Entity.Null, primaryTarget, entity1));
                            }
                        }
                    }
                    else if (citizen1.m_BirthDay == (short)1)
                    {
                        // Adult age must be between teen age limit and adult retirement age
                        int adultAgeLimitInDays = adult_age_limit;
                        int teentAgeLimitInDays = teen_age_limit;

                        // Adult age will have a higher probability for being younger
                        double ageVariation = GaussianRandom.NextGaussianDouble(random)/2.5f;

                        int median_point = (adultAgeLimitInDays - teentAgeLimitInDays) / 4;

                        int age_offset;

                        if(ageVariation > 0)
                        {
                            age_offset = (int)((adultAgeLimitInDays - median_point - teentAgeLimitInDays) * ageVariation);
                        } else
                        {
                            age_offset = (int)((median_point) * ageVariation);
                        }

                        num2 = teentAgeLimitInDays + median_point + age_offset;
                        //Mod.log.Info($"Adult Immigrant age: {num2}, ageVar:{ageVariation}, ageOff:{age_offset}, medpoint:{median_point}");
                        citizen1.SetAge(CitizenAge.Adult);
                        
                        // Education will be based on what is required on free workplaces
                        int totalWorkPlaces = m_FreeWorkplaces[0] + m_FreeWorkplaces[1] + m_FreeWorkplaces[2] + m_FreeWorkplaces[3] + m_FreeWorkplaces[4];

                        int eduLevel = 0;
                        if (totalWorkPlaces == 0) 
                        {
                            eduLevel = (random.NextInt(5) + 1) / 2;
                        }
                        else 
                        {
                        
                            //Same logic as pop rebalance mod by Infixo
                            int roll = random.NextInt(totalWorkPlaces);
                            for (int c = 4; c >= 0; c--)
                            {
                                totalWorkPlaces -= m_FreeWorkplaces[c];
                                if (roll >= totalWorkPlaces)
                                {
                                    eduLevel = c;
                                    break;
                                }
                            }
                        }
                        int2.x = eduLevel;
                        int2.y = eduLevel;
                    }
                    else if (citizen1.m_BirthDay == (short)2)
                    {
                        int childAgeLimit = child_age_limit;
                        int teenAgeLimit = teen_age_limit;
                        
                        double num3 = (double)citizen1.GetPseudoRandom(CitizenPseudoRandom.StudyWillingness).NextFloat();
                        if ((double)random.NextFloat(1f) > (double)this.m_DemandParameters.m_TeenSpawnPercentage)
                        {
                            citizen1.SetAge(CitizenAge.Child);
                            citizen1.SetEducationLevel(0);
                        
                            // Child age must be between zero and child age limit
                            num2 = random.NextInt(childAgeLimit);
                        }
                        else
                        {
                            citizen1.SetAge(CitizenAge.Teen);
                            int eduLevel = 0;
                            if(random.NextInt(100) < elementary_grad_prob)
                            {
                                eduLevel = 1;
                            }
                            int2 = new int2(eduLevel, eduLevel);
                        
                            // Teen age must be between child age limit and teen age limit
                            num2 = childAgeLimit + random.NextInt(teenAgeLimit - childAgeLimit);
                        }
                    }
                    else if (citizen1.m_BirthDay == (short)3)
                    {
                        //Elder age must be between adult retirement and life expectancy
                        int adultAgeLimitInDays = adult_age_limit;
                        int life_expectancy = female_life_expectancy;
                        if ((citizen1.m_State & CitizenFlags.Male) != CitizenFlags.None)
                        {
                            life_expectancy = male_life_expectancy;
                        }
                        num2 = adultAgeLimitInDays + random.NextInt(life_expectancy - adultAgeLimitInDays);
                        citizen1.SetAge(CitizenAge.Elderly);
                        int2 = new int2(0, 4);
                    }
                    else
                    {
                        // Adult age must be between teen age limit and adult retirement age
                        int adultAgeLimitInDays = adult_age_limit;
                        int teentAgeLimitInDays = teen_age_limit;
                        
                        num2 = teentAgeLimitInDays + random.NextInt(adultAgeLimitInDays - teentAgeLimitInDays);
                        citizen1.SetAge(CitizenAge.Adult);
                        int2 = new int2(2, 3);
                    }
                    
                    float max = 0.0f;
                    float num4 = 1f;
                    for (int index3 = 0; index3 <= 3; ++index3)
                    {
                        if (index3 >= int2.x && index3 <= int2.y)
                        {
                            max += this.m_DemandParameters.m_NewCitizenEducationParameters[index3];
                        }
                        num4 -= this.m_DemandParameters.m_NewCitizenEducationParameters[index3];
                    }
                    if (int2.y == 4)
                        max += num4;
                    float num5 = random.NextFloat(max);
                    for (int x = int2.x; x <= int2.y; ++x)
                    {
                        if (x == 4 || (double)num5 < (double)this.m_DemandParameters.m_NewCitizenEducationParameters[x])
                        {
                            citizen1.SetEducationLevel(x);
                            break;
                        }
                        num5 -= this.m_DemandParameters.m_NewCitizenEducationParameters[x];
                    }
                    
                    citizen1.m_BirthDay = (short)(TimeSystem.GetDay(this.m_SimulationFrame, this.m_TimeData) - num2);
                    
                    this.m_Citizens[entity1] = citizen1;
                }
            }
        }

        private struct TypeHandle
        {
            public ComponentLookup<Citizen> __Game_Citizens_Citizen_RW_ComponentLookup;
            public BufferLookup<HouseholdCitizen> __Game_Citizens_HouseholdCitizen_RW_BufferLookup;
            public ComponentLookup<Arrived> __Game_Citizens_Arrived_RW_ComponentLookup;
            public ComponentLookup<CarKeeper> __Game_Citizens_CarKeeper_RW_ComponentLookup;
            public ComponentLookup<MailSender> __Game_Citizens_MailSender_RW_ComponentLookup;
            public ComponentLookup<CrimeVictim> __Game_Citizens_CrimeVictim_RW_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<CitizenData> __Game_Prefabs_CitizenData_RO_ComponentLookup;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                this.__Game_Citizens_Citizen_RW_ComponentLookup = state.GetComponentLookup<Citizen>();
                this.__Game_Citizens_HouseholdCitizen_RW_BufferLookup = state.GetBufferLookup<HouseholdCitizen>();
                this.__Game_Citizens_Arrived_RW_ComponentLookup = state.GetComponentLookup<Arrived>();
                this.__Game_Citizens_CarKeeper_RW_ComponentLookup = state.GetComponentLookup<CarKeeper>();
                this.__Game_Citizens_MailSender_RW_ComponentLookup = state.GetComponentLookup<MailSender>();
                this.__Game_Citizens_CrimeVictim_RW_ComponentLookup = state.GetComponentLookup<CrimeVictim>();
                this.__Game_Prefabs_CitizenData_RO_ComponentLookup = state.GetComponentLookup<CitizenData>(true);
            }
        }
    }
}
