
using Colossal.Collections;
using Game.Buildings;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Prefabs;
using Game.Triggers;
using Game.Simulation;
using Game;
using Unity.Entities.Internal;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static Game.Prefabs.ElectricityConnection;

#nullable disable
namespace RealLife.Systems
{
    public partial class RealLifeGraduationSystem : GameSystemBase
    {
        public int debugFastGraduationLevel;
        public const int kUpdatesPerDay = 1;
        public const int kCheckSlowdown = 2;
        private SimulationSystem m_SimulationSystem;
        private EndFrameBarrier m_EndFrameBarrier;
        private CitySystem m_CitySystem;
        private TriggerSystem m_TriggerSystem;
        private EntityQuery m_StudentQuery;
        private RealLifeGraduationSystem.TypeHandle __TypeHandle;
        private EntityQuery m_TimeDataQuery;
        private EntityQuery m_EconomyParameterQuery;


        public override int GetUpdateInterval(SystemUpdatePhase phase) => 16384;

        public static float GetDropoutProbability(
          Citizen citizen,
          int level,
          float commute,
          float fee,
          int wealth,
          uint simulationFrame,
          ref EconomyParameterData economyParameters,
          SchoolData schoolData,
          DynamicBuffer<CityModifier> modifiers,
          float efficiency,
          TimeData timeData,
          int adult_age_limit,
          float elementary_grad_prob,
          float high_grad_prob,
          float college_grad_prob,
          float university_grad_prob
          )
        {
            float ageInDays = citizen.GetAgeInDays(simulationFrame, timeData);
            float studyWillingness = citizen.GetPseudoRandom(CitizenPseudoRandom.StudyWillingness).NextFloat();
            int failedEducationCount = citizen.GetFailedEducationCount();
            float graduationProbability = RealLifeGraduationSystem.GetGraduationProbability(level, (int)citizen.m_WellBeing, schoolData, modifiers, studyWillingness, efficiency, elementary_grad_prob, high_grad_prob, college_grad_prob, university_grad_prob);
            return RealLifeGraduationSystem.GetDropoutProbability(level, commute, fee, wealth, ageInDays, studyWillingness, failedEducationCount, graduationProbability, ref economyParameters, adult_age_limit, elementary_grad_prob, high_grad_prob, college_grad_prob, university_grad_prob);
        }

        public static float GetDropoutProbability(
          int level,
          float commute,
          float fee,
          int wealth,
          float age,
          float studyWillingness,
          int failedEducationCount,
          float graduationProbability,
          ref EconomyParameterData economyParameters,
          int adult_age_limit,
          float elementary_grad_prob,
          float high_grad_prob,
          float college_grad_prob,
          float university_grad_prob)
        {
            int y = 4 - failedEducationCount;
            float s = math.pow(1f - graduationProbability, (float)y);
            float num1 = (float)(1.0 / ((double)graduationProbability * 2.0 * 1.0));
            float num2 = num1 * fee;
            if (level > 2)
                num2 -= num1 * (float)economyParameters.m_UnemploymentBenefit;
            float num3 = math.max(0.0f, (float)adult_age_limit - age);
            float num4 = (float)economyParameters.GetWage(math.min(2, level - 1)) * num3;
            float num5 = (float)((double)math.lerp((float)economyParameters.GetWage(level), (float)economyParameters.GetWage(level - 1), s) * ((double)num3 - (double)num1) - (double)num2 + (0.5 + (double)studyWillingness) * (double)economyParameters.m_UnemploymentBenefit * (double)num1);
            if ((double)num4 >= (double)num5)
                return 1f;
            float num6 = (num5 - num4) / num4;
            return math.saturate((float)((double)level / 4.0 - 0.10000000149011612 - 10.0 * (double)num6 - (double)wealth / ((double)num5 - (double)num4) + (double)commute / 5000.0));
        }

        public static float GetGraduationProbability(
          int level,
          int wellbeing,
          SchoolData schoolData,
          DynamicBuffer<CityModifier> modifiers,
          float studyWillingness,
          float efficiency,
          float elementary_grad_prob,
          float high_grad_prob,
          float college_grad_prob,
          float university_grad_prob)
        {
            float2 modifier1 = CityUtils.GetModifier(modifiers, CityModifierType.CollegeGraduation);
            float2 modifier2 = CityUtils.GetModifier(modifiers, CityModifierType.UniversityGraduation);
            return RealLifeGraduationSystem.GetGraduationProbability(level, wellbeing, schoolData.m_GraduationModifier, modifier1, modifier2, studyWillingness, efficiency, elementary_grad_prob, high_grad_prob, college_grad_prob, university_grad_prob);
        }

        public static float GetGraduationProbability(
          int level,
          int wellbeing,
          float graduationModifier,
          float2 collegeModifier,
          float2 uniModifier,
          float studyWillingness,
          float efficiency,
          float elementary_grad_prob,
          float high_grad_prob,
          float college_grad_prob,
          float university_grad_prob)
        {
            if ((double)efficiency <= 1.0 / 1000.0)
                return 0.0f;
            float num1 = math.saturate((float)((0.5 + (double)studyWillingness) * (double)wellbeing / 75.0));
            float num2;
            switch (level)
            {
                case 1:
                    num2 = (elementary_grad_prob/100f);
                    break;
                case 2:
                    num2 = (high_grad_prob / 100f);
                    break;
                case 3:
                    float num3 = (college_grad_prob / 100f) * math.log((float)(1.6000000238418579 * (double)num1 + 1.0)) + collegeModifier.x;
                    num2 = (num3 + college_grad_prob) / 100f;
                    break;
                case 4:
                    float num4 = (university_grad_prob / 100f) * num1 + uniModifier.x;
                    num2 = (num4 + university_grad_prob) / 100f;
                    break;
                default:
                    num2 = 0.0f;
                    break;
            }
            
            return (float)(1.0 - (1.0 - (double)num2) / (double)efficiency) + graduationModifier;
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            this.m_SimulationSystem = this.World.GetOrCreateSystemManaged<SimulationSystem>();
            this.m_EndFrameBarrier = this.World.GetOrCreateSystemManaged<EndFrameBarrier>();
            this.m_TriggerSystem = this.World.GetOrCreateSystemManaged<TriggerSystem>();
            this.m_TimeDataQuery = this.GetEntityQuery(ComponentType.ReadOnly<TimeData>());
            this.m_EconomyParameterQuery = this.GetEntityQuery(ComponentType.ReadOnly<EconomyParameterData>());
            this.m_CitySystem = this.World.GetOrCreateSystemManaged<CitySystem>();
            this.m_StudentQuery = this.GetEntityQuery(ComponentType.ReadOnly<Game.Citizens.Student>(), ComponentType.ReadWrite<Citizen>(), ComponentType.ReadOnly<UpdateFrame>());
            this.RequireForUpdate(this.m_StudentQuery);
            this.RequireForUpdate<EconomyParameterData>();
            this.RequireForUpdate<TimeData>();
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnUpdate()
        {
            uint updateFrame = SimulationUtils.GetUpdateFrame(this.m_SimulationSystem.frameIndex, 1, 16);

            RealLifeGraduationSystem.GraduationJob jobData = new RealLifeGraduationSystem.GraduationJob()
            {
                m_EntityType = InternalCompilerInterface.GetEntityTypeHandle(ref this.__TypeHandle.__Unity_Entities_Entity_TypeHandle, ref this.CheckedStateRef),
                m_UpdateFrameType = InternalCompilerInterface.GetSharedComponentTypeHandle<UpdateFrame>(ref this.__TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle, ref this.CheckedStateRef),
                m_CitizenType = InternalCompilerInterface.GetComponentTypeHandle<Citizen>(ref this.__TypeHandle.__Game_Citizens_Citizen_RW_ComponentTypeHandle, ref this.CheckedStateRef),
                m_StudentType = InternalCompilerInterface.GetComponentTypeHandle<Game.Citizens.Student>(ref this.__TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle, ref this.CheckedStateRef),
                m_Prefabs = InternalCompilerInterface.GetComponentLookup<PrefabRef>(ref this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup, ref this.CheckedStateRef),
                m_SchoolDatas = InternalCompilerInterface.GetComponentLookup<SchoolData>(ref this.__TypeHandle.__Game_Prefabs_SchoolData_RO_ComponentLookup, ref this.CheckedStateRef),
                m_InstalledUpgrades = InternalCompilerInterface.GetBufferLookup<InstalledUpgrade>(ref this.__TypeHandle.__Game_Buildings_InstalledUpgrade_RO_BufferLookup, ref this.CheckedStateRef),
                m_CityModifiers = InternalCompilerInterface.GetBufferLookup<CityModifier>(ref this.__TypeHandle.__Game_City_CityModifier_RO_BufferLookup, ref this.CheckedStateRef),
                m_Purposes = InternalCompilerInterface.GetComponentLookup<TravelPurpose>(ref this.__TypeHandle.__Game_Citizens_TravelPurpose_RO_ComponentLookup, ref this.CheckedStateRef),
                m_BuildingEfficiencies = InternalCompilerInterface.GetBufferLookup<Efficiency>(ref this.__TypeHandle.__Game_Buildings_Efficiency_RO_BufferLookup, ref this.CheckedStateRef),
                m_Fees = InternalCompilerInterface.GetBufferLookup<ServiceFee>(ref this.__TypeHandle.__Game_City_ServiceFee_RO_BufferLookup, ref this.CheckedStateRef),
                m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_TriggerBuffer = this.m_TriggerSystem.CreateActionBuffer().AsParallelWriter(),
                m_EconomyParameters = this.m_EconomyParameterQuery.GetSingleton<EconomyParameterData>(),
                m_TimeData = this.m_TimeDataQuery.GetSingleton<TimeData>(),
                m_RandomSeed = RandomSeed.Next(),
                m_City = this.m_CitySystem.City,
                m_SimulationFrame = this.m_SimulationSystem.frameIndex,
                m_UpdateFrameIndex = updateFrame,
                m_DebugFastGraduationLevel = this.debugFastGraduationLevel,
                child_age_limit = Mod.m_Setting.child_age_limit,
                teen_age_limit = Mod.m_Setting.teen_age_limit,
                men_age_limit = Mod.m_Setting.male_adult_age_limit,
                women_age_limit = Mod.m_Setting.female_adult_age_limit,
                years_in_college = Mod.m_Setting.years_in_college,
                years_in_university = Mod.m_Setting.years_in_university,
                elementary_grad_probability = Mod.m_Setting.elementary_grad_prob,
                high_grad_probability = Mod.m_Setting.high_grad_prob,
                college_grad_probability = Mod.m_Setting.college_grad_prob,
                university_grad_probability = Mod.m_Setting.university_grad_prob,
                male_life_expectancy = Mod.m_Setting.male_life_expectancy,
                female_life_expectancy = Mod.m_Setting.female_life_expectancy,
                college_in_univ_prob = Mod.m_Setting.college_edu_in_univ,
                day = TimeSystem.GetDay(this.m_SimulationSystem.frameIndex, this.m_TimeDataQuery.GetSingleton<TimeData>())
            };

            this.Dependency = jobData.ScheduleParallel<RealLifeGraduationSystem.GraduationJob>(this.m_StudentQuery, this.Dependency);
            this.m_EndFrameBarrier.AddJobHandleForProducer(this.Dependency);
            this.m_TriggerSystem.AddActionBufferWriter(this.Dependency);
        }

        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            this.__TypeHandle.__AssignHandles(ref this.CheckedStateRef);
        }

        [UnityEngine.Scripting.Preserve]
        public RealLifeGraduationSystem()
        {
        }

        [BurstCompile]
        public struct GraduationJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public SharedComponentTypeHandle<UpdateFrame> m_UpdateFrameType;
            [ReadOnly]
            public ComponentTypeHandle<Game.Citizens.Student> m_StudentType;
            public ComponentTypeHandle<Citizen> m_CitizenType;
            [ReadOnly]
            public ComponentLookup<PrefabRef> m_Prefabs;
            [ReadOnly]
            public ComponentLookup<SchoolData> m_SchoolDatas;
            [ReadOnly]
            public ComponentLookup<TravelPurpose> m_Purposes;
            [ReadOnly]
            public BufferLookup<InstalledUpgrade> m_InstalledUpgrades;
            [ReadOnly]
            public BufferLookup<CityModifier> m_CityModifiers;
            [ReadOnly]
            public BufferLookup<Efficiency> m_BuildingEfficiencies;
            [ReadOnly]
            public BufferLookup<ServiceFee> m_Fees;
            public NativeQueue<TriggerAction>.ParallelWriter m_TriggerBuffer;
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            public EconomyParameterData m_EconomyParameters;
            public TimeData m_TimeData;
            public RandomSeed m_RandomSeed;
            public uint m_SimulationFrame;
            public Entity m_City;
            public uint m_UpdateFrameIndex;
            public int m_DebugFastGraduationLevel;
            public int child_age_limit;
            public int teen_age_limit;
            public int men_age_limit;
            public int women_age_limit;
            public int male_life_expectancy;
            public int female_life_expectancy;
            public int years_in_college;
            public int years_in_university;
            public float elementary_grad_probability;
            public float high_grad_probability;
            public float college_grad_probability;
            public float university_grad_probability;
            public int college_in_univ_prob;
            public int day;

            public void Execute(
              in ArchetypeChunk chunk,
              int unfilteredChunkIndex,
              bool useEnabledMask,
              in v128 chunkEnabledMask)
            {

                if (this.m_DebugFastGraduationLevel == 0 && (int)chunk.GetSharedComponent<UpdateFrame>(this.m_UpdateFrameType).m_Index != (int)this.m_UpdateFrameIndex)
                    return;

                NativeArray<Game.Citizens.Student> nativeArray1 = chunk.GetNativeArray<Game.Citizens.Student>(ref this.m_StudentType);
                NativeArray<Citizen> nativeArray2 = chunk.GetNativeArray<Citizen>(ref this.m_CitizenType);
                NativeArray<Entity> nativeArray3 = chunk.GetNativeArray(this.m_EntityType);
                DynamicBuffer<CityModifier> cityModifier = this.m_CityModifiers[this.m_City];
                Unity.Mathematics.Random random = this.m_RandomSeed.GetRandom(unfilteredChunkIndex);

                for (int index = 0; index < chunk.Count; ++index)
                {
                    Entity entity = nativeArray3[index];
                    Game.Citizens.Student student = nativeArray1[index];
                    ref Citizen local = ref nativeArray2.ElementAt<Citizen>(index);
                    Entity school = student.m_School;
                    int age = day - (int)local.m_BirthDay;
                    CitizenAge ageGroup = local.GetAge();

                    if (this.m_Prefabs.HasComponent(school))
                    {
                        Entity prefab = this.m_Prefabs[school].m_Prefab;
                        if (this.m_SchoolDatas.HasComponent(prefab))
                        {
                            int num1 = (int)student.m_Level;
                            if (num1 == (int)byte.MaxValue)
                            {
                                if(college_in_univ_prob > 0)
                                {
                                    if(num1 < (int)this.m_SchoolDatas[prefab].m_EducationLevel)
                                    {
                                        num1 = (int)this.m_SchoolDatas[prefab].m_EducationLevel - 1;
                                        //Mod.log.Info($"num1:{num1}, univ:{(int)this.m_SchoolDatas[prefab].m_EducationLevel}");
                                    } else
                                    {
                                        num1 = (int)this.m_SchoolDatas[prefab].m_EducationLevel;
                                    }
                                } else
                                {
                                    num1 = (int)this.m_SchoolDatas[prefab].m_EducationLevel;
                                }
                                
                            }
                            SchoolData schoolData = this.m_SchoolDatas[prefab];
                            if (this.m_InstalledUpgrades.HasBuffer(school))
                            {
                                UpgradeUtils.CombineStats<SchoolData>(ref schoolData, this.m_InstalledUpgrades[school], ref this.m_Prefabs, ref this.m_SchoolDatas);
                            }
                            int wellBeing = (int)local.m_WellBeing;
                            float studyWillingness = local.GetPseudoRandom(CitizenPseudoRandom.StudyWillingness).NextFloat();
                            float efficiency = BuildingUtils.GetEfficiency(school, ref this.m_BuildingEfficiencies);
                            float graduationProbability = RealLifeGraduationSystem.GetGraduationProbability(num1, wellBeing, schoolData, cityModifier, studyWillingness, efficiency, elementary_grad_probability, high_grad_probability, college_grad_probability, university_grad_probability);
                            //Mod.log.Info($"age:{age},graduationProbability:{graduationProbability},num1:{num1}");
                            if (this.m_DebugFastGraduationLevel == 0 || this.m_DebugFastGraduationLevel == num1)
                            {
                                if (this.m_DebugFastGraduationLevel == num1 || (double)random.NextFloat() < (double)graduationProbability)
                                {
                                    bool graduate = false;

                                    switch (num1)
                                    {
                                        case 1: //elementary school
                                            graduate = age >= child_age_limit;
                                            break;
                                        case 2: //high school
                                            graduate = age >= teen_age_limit;
                                            break;
                                        case 3: //college
                                            graduate = (age - teen_age_limit) >= years_in_college;
                                            break;
                                        case 4: //university
                                            graduate = (age - teen_age_limit) >= (years_in_college + years_in_university);
                                            break;
                                        default:
                                            break;
                                    }

                                    if (graduate)
                                    {
                                        ref Citizen local2 = ref nativeArray2.ElementAt<Citizen>(index);
                                        int age2 = day - (int)local.m_BirthDay;                                       
                                        
                                        local.SetEducationLevel(Mathf.Max(local.GetEducationLevel(), num1));
                                        if (this.m_DebugFastGraduationLevel != 0 || num1 >= 1)
                                        {
                                            this.LeaveSchool(unfilteredChunkIndex, entity, school);
                                            this.m_TriggerBuffer.Enqueue(new TriggerAction(TriggerType.CitizenGraduated, Entity.Null, entity, school));
                                        }
                                    }
                                }
                                else if (num1 > 2)
                                {
                                    int failedEducationCount = local.GetFailedEducationCount();
                                    if (failedEducationCount < 3)
                                    {
                                        local.SetFailedEducationCount(failedEducationCount + 1);
                                        float fee = ServiceFeeSystem.GetFee(ServiceFeeSystem.GetEducationResource((int)student.m_Level), this.m_Fees[this.m_City]);
                                        float num2 = 1f - math.pow(math.saturate(1f - RealLifeGraduationSystem.GetDropoutProbability(local, (int)student.m_Level, student.m_LastCommuteTime, fee, 0, this.m_SimulationFrame, ref this.m_EconomyParameters, schoolData, cityModifier, efficiency, this.m_TimeData, men_age_limit, elementary_grad_probability, high_grad_probability, college_grad_probability, university_grad_probability)), 32f);
                                        if ((double)random.NextFloat() < (double)num2)
                                        {
                                            this.LeaveSchool(unfilteredChunkIndex, entity, school);
                                            this.m_TriggerBuffer.Enqueue(new TriggerAction(TriggerType.CitizenDroppedOutSchool, Entity.Null, entity, school));
                                        }
                                    }
                                    else
                                    {
                                        this.LeaveSchool(unfilteredChunkIndex, entity, school);
                                        this.m_TriggerBuffer.Enqueue(new TriggerAction(TriggerType.CitizenFailedSchool, Entity.Null, entity, school));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private void LeaveSchool(int chunkIndex, Entity entity, Entity school)
            {
                this.m_CommandBuffer.AddComponent<StudentsRemoved>(chunkIndex, school);
                this.m_CommandBuffer.RemoveComponent<Game.Citizens.Student>(chunkIndex, entity);
                TravelPurpose componentData;
                if (!this.m_Purposes.TryGetComponent(entity, out componentData))
                    return;
                switch (componentData.m_Purpose)
                {
                    case Game.Citizens.Purpose.Studying:
                    case Game.Citizens.Purpose.GoingToSchool:
                        this.m_CommandBuffer.RemoveComponent<TravelPurpose>(chunkIndex, entity);
                        break;
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
            public SharedComponentTypeHandle<UpdateFrame> __Game_Simulation_UpdateFrame_SharedComponentTypeHandle;
            public ComponentTypeHandle<Citizen> __Game_Citizens_Citizen_RW_ComponentTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<Game.Citizens.Student> __Game_Citizens_Student_RO_ComponentTypeHandle;
            [ReadOnly]
            public ComponentLookup<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<SchoolData> __Game_Prefabs_SchoolData_RO_ComponentLookup;
            [ReadOnly]
            public BufferLookup<InstalledUpgrade> __Game_Buildings_InstalledUpgrade_RO_BufferLookup;
            [ReadOnly]
            public BufferLookup<CityModifier> __Game_City_CityModifier_RO_BufferLookup;
            [ReadOnly]
            public ComponentLookup<TravelPurpose> __Game_Citizens_TravelPurpose_RO_ComponentLookup;
            [ReadOnly]
            public BufferLookup<Efficiency> __Game_Buildings_Efficiency_RO_BufferLookup;
            [ReadOnly]
            public BufferLookup<ServiceFee> __Game_City_ServiceFee_RO_BufferLookup;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                this.__Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                this.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle = state.GetSharedComponentTypeHandle<UpdateFrame>();
                this.__Game_Citizens_Citizen_RW_ComponentTypeHandle = state.GetComponentTypeHandle<Citizen>();
                this.__Game_Citizens_Student_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Citizens.Student>(true);
                this.__Game_Prefabs_PrefabRef_RO_ComponentLookup = state.GetComponentLookup<PrefabRef>(true);
                this.__Game_Prefabs_SchoolData_RO_ComponentLookup = state.GetComponentLookup<SchoolData>(true);
                this.__Game_Buildings_InstalledUpgrade_RO_BufferLookup = state.GetBufferLookup<InstalledUpgrade>(true);
                this.__Game_City_CityModifier_RO_BufferLookup = state.GetBufferLookup<CityModifier>(true);
                this.__Game_Citizens_TravelPurpose_RO_ComponentLookup = state.GetComponentLookup<TravelPurpose>(true);
                this.__Game_Buildings_Efficiency_RO_BufferLookup = state.GetBufferLookup<Efficiency>(true);
                this.__Game_City_ServiceFee_RO_BufferLookup = state.GetBufferLookup<ServiceFee>(true);
            }
        }
    }
}
