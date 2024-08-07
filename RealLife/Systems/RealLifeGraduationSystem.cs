
using Colossal.Collections;
using Game.Buildings;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Prefabs;
using Game.Triggers;
using Game.Simulation;
using Game;
using RealLife;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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
        private EntityQuery __query_1855827631_0;
        private EntityQuery __query_1855827631_1;

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
          int adult_age_limit)
        {
            float ageInDays = citizen.GetAgeInDays(simulationFrame, timeData);
            float studyWillingness = citizen.GetPseudoRandom(CitizenPseudoRandom.StudyWillingness).NextFloat();
            int failedEducationCount = citizen.GetFailedEducationCount();
            float graduationProbability = RealLifeGraduationSystem.GetGraduationProbability(level, (int)citizen.m_WellBeing, schoolData, modifiers, studyWillingness, efficiency);
            return RealLifeGraduationSystem.GetDropoutProbability(level, commute, fee, wealth, ageInDays, studyWillingness, failedEducationCount, graduationProbability, ref economyParameters, adult_age_limit);
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
          int adult_age_limit)
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
          float efficiency)
        {
            float2 modifier1 = CityUtils.GetModifier(modifiers, CityModifierType.CollegeGraduation);
            float2 modifier2 = CityUtils.GetModifier(modifiers, CityModifierType.UniversityGraduation);
            return RealLifeGraduationSystem.GetGraduationProbability(level, wellbeing, schoolData.m_GraduationModifier, modifier1, modifier2, studyWillingness, efficiency);
        }

        public static float GetGraduationProbability(
          int level,
          int wellbeing,
          float graduationModifier,
          float2 collegeModifier,
          float2 uniModifier,
          float studyWillingness,
          float efficiency)
        {
            if ((double)efficiency <= 1.0 / 1000.0)
                return 0.0f;
            float num1 = math.saturate((float)((0.5 + (double)studyWillingness) * (double)wellbeing / 75.0));
            float num2;
            switch (level)
            {
                case 1:
                    num2 = math.smoothstep(0.0f, 1f, (float)(0.60000002384185791 * (double)num1 + 0.40999999642372131));
                    break;
                case 2:
                    num2 = 0.6f * math.log((float)(2.5999999046325684 * (double)num1 + 1.1000000238418579));
                    break;
                case 3:
                    float num3 = 90f * math.log((float)(1.6000000238418579 * (double)num1 + 1.0)) + collegeModifier.x;
                    num2 = (num3 + num3 * collegeModifier.y) / 100f;
                    break;
                case 4:
                    float num4 = 70f * num1 + uniModifier.x;
                    num2 = (num4 + num4 * uniModifier.y) / 100f;
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
            this.__TypeHandle.__Game_City_ServiceFee_RO_BufferLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_Efficiency_RO_BufferLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_TravelPurpose_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_City_CityModifier_RO_BufferLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_InstalledUpgrade_RO_BufferLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_SchoolData_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Citizen_RW_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref this.CheckedStateRef);

            RealLifeGraduationSystem.GraduationJob jobData = new RealLifeGraduationSystem.GraduationJob()
            {
                m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle,
                m_UpdateFrameType = this.__TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle,
                m_CitizenType = this.__TypeHandle.__Game_Citizens_Citizen_RW_ComponentTypeHandle,
                m_StudentType = this.__TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle,
                m_Prefabs = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup,
                m_SchoolDatas = this.__TypeHandle.__Game_Prefabs_SchoolData_RO_ComponentLookup,
                m_InstalledUpgrades = this.__TypeHandle.__Game_Buildings_InstalledUpgrade_RO_BufferLookup,
                m_CityModifiers = this.__TypeHandle.__Game_City_CityModifier_RO_BufferLookup,
                m_Purposes = this.__TypeHandle.__Game_Citizens_TravelPurpose_RO_ComponentLookup,
                m_BuildingEfficiencies = this.__TypeHandle.__Game_Buildings_Efficiency_RO_BufferLookup,
                m_Fees = this.__TypeHandle.__Game_City_ServiceFee_RO_BufferLookup,
                m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_TriggerBuffer = this.m_TriggerSystem.CreateActionBuffer().AsParallelWriter(),
                m_EconomyParameters = this.__query_1855827631_0.GetSingleton<EconomyParameterData>(),
                m_TimeData = this.__query_1855827631_1.GetSingleton<TimeData>(),
                m_RandomSeed = RandomSeed.Next(),
                m_City = this.m_CitySystem.City,
                m_SimulationFrame = this.m_SimulationSystem.frameIndex,
                m_UpdateFrameIndex = updateFrame,
                m_DebugFastGraduationLevel = this.debugFastGraduationLevel,
                teen_age_limit = Mod.m_Setting.teen_age_limit,
                adult_age_limit = Mod.m_Setting.adult_age_limit,
                years_in_college = Mod.m_Setting.years_in_college,
                years_in_university = Mod.m_Setting.years_in_university
            };

            this.Dependency = jobData.ScheduleParallel<RealLifeGraduationSystem.GraduationJob>(this.m_StudentQuery, this.Dependency);
            this.m_EndFrameBarrier.AddJobHandleForProducer(this.Dependency);
            this.m_TriggerSystem.AddActionBufferWriter(this.Dependency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void __AssignQueries(ref SystemState state)
        {
            this.__query_1855827631_0 = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[1]
              {
          ComponentType.ReadOnly<EconomyParameterData>()
              },
                Any = new ComponentType[0],
                None = new ComponentType[0],
                Disabled = new ComponentType[0],
                Absent = new ComponentType[0],
                Options = EntityQueryOptions.IncludeSystems
            });

            this.__query_1855827631_1 = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[1]
              {
          ComponentType.ReadOnly<TimeData>()
              },
                Any = new ComponentType[0],
                None = new ComponentType[0],
                Disabled = new ComponentType[0],
                Absent = new ComponentType[0],
                Options = EntityQueryOptions.IncludeSystems
            });
        }

        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            this.__AssignQueries(ref this.CheckedStateRef);
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
            public int teen_age_limit;
            public int adult_age_limit;
            public int years_in_college;
            public int years_in_university;

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
                int day = TimeSystem.GetDay(this.m_SimulationFrame, this.m_TimeData);
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
                                num1 = (int)this.m_SchoolDatas[prefab].m_EducationLevel;
                            }
                            SchoolData schoolData = this.m_SchoolDatas[prefab];
                            if (this.m_InstalledUpgrades.HasBuffer(school))
                            {
                                UpgradeUtils.CombineStats<SchoolData>(ref schoolData, this.m_InstalledUpgrades[school], ref this.m_Prefabs, ref this.m_SchoolDatas);
                            }
                            int wellBeing = (int)local.m_WellBeing;
                            float studyWillingness = local.GetPseudoRandom(CitizenPseudoRandom.StudyWillingness).NextFloat();
                            float efficiency = BuildingUtils.GetEfficiency(school, ref this.m_BuildingEfficiencies);
                            float graduationProbability = RealLifeGraduationSystem.GetGraduationProbability(num1, wellBeing, schoolData, cityModifier, studyWillingness, efficiency);
                            if (this.m_DebugFastGraduationLevel == 0 || this.m_DebugFastGraduationLevel == num1)
                            {
                                if (this.m_DebugFastGraduationLevel == num1 || (double)random.NextFloat() < (double)graduationProbability)
                                {
                                    bool graduate = false;
                                    switch(num1)
                                    {
                                        case 1: //elementary school
                                            graduate = ((int)ageGroup > 1);
                                            break;
                                        case 2: //high school
                                            graduate = ((int)ageGroup > 2);
                                            break;
                                        case 3: //college
                                            graduate = (age - teen_age_limit) > years_in_college;
                                            break;
                                        case 4: //university
                                            graduate = (age - teen_age_limit) > (years_in_college + years_in_university);
                                            break;
                                        default:
                                            break;
                                    }
                                    if(graduate)
                                    {
                                        local.SetEducationLevel(Mathf.Max(local.GetEducationLevel(), num1));
                                        if (this.m_DebugFastGraduationLevel != 0 || num1 > 1)
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
                                        float num2 = 1f - math.pow(math.saturate(1f - RealLifeGraduationSystem.GetDropoutProbability(local, (int)student.m_Level, student.m_LastCommuteTime, fee, 0, this.m_SimulationFrame, ref this.m_EconomyParameters, schoolData, cityModifier, efficiency, this.m_TimeData, adult_age_limit)), 32f);
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
