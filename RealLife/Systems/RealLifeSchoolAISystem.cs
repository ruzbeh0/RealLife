
using Colossal.Collections;
using Game;
using Game.Simulation;
using Game.Buildings;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Prefabs;
using Game.Tools;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

#nullable disable
namespace RealLife.Systems
{
    public partial class RealLifeSchoolAISystem : GameSystemBase
    {
        private SimulationSystem m_SimulationSystem;
        private CitySystem m_CitySystem;
        private EndFrameBarrier m_EndFrameBarrier;
        private EntityQuery m_SchoolQuery;
        private RealLifeSchoolAISystem.TypeHandle __TypeHandle;
        private EntityQuery __query_1235104412_0;
        private EntityQuery __query_1235104412_1;
        private EntityQuery __query_1235104412_2;

        public override int GetUpdateInterval(SystemUpdatePhase phase) => 256;

        public override int GetUpdateOffset(SystemUpdatePhase phase) => 96;

        [UnityEngine.Scripting.Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            this.m_SimulationSystem = this.World.GetOrCreateSystemManaged<SimulationSystem>();
            this.m_CitySystem = this.World.GetOrCreateSystemManaged<CitySystem>();
            this.m_EndFrameBarrier = this.World.GetOrCreateSystemManaged<EndFrameBarrier>();
            this.m_SchoolQuery = this.GetEntityQuery(ComponentType.ReadWrite<Game.Buildings.School>(), ComponentType.ReadWrite<Game.Buildings.Student>(), ComponentType.ReadOnly<PrefabRef>(), ComponentType.Exclude<Temp>(), ComponentType.Exclude<Deleted>());
            this.RequireForUpdate(this.m_SchoolQuery);
            this.RequireForUpdate<EconomyParameterData>();
            this.RequireForUpdate<EducationParameterData>();
            this.RequireForUpdate<TimeData>();
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnUpdate()
        {
            this.__TypeHandle.__Game_City_ServiceFee_RO_BufferLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_City_CityModifier_RO_BufferLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_TravelPurpose_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Citizen_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Student_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_SchoolData_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_Student_RW_BufferTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_ServiceUsage_RW_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_School_RW_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_Efficiency_RO_BufferTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_InstalledUpgrade_RO_BufferTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);

            RealLifeSchoolAISystem.SchoolTickJob jobData = new RealLifeSchoolAISystem.SchoolTickJob()
            {
                m_PrefabRefType = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle,
                m_InstalledUpgradeType = this.__TypeHandle.__Game_Buildings_InstalledUpgrade_RO_BufferTypeHandle,
                m_EfficiencyType = this.__TypeHandle.__Game_Buildings_Efficiency_RO_BufferTypeHandle,
                m_SchoolType = this.__TypeHandle.__Game_Buildings_School_RW_ComponentTypeHandle,
                m_ServiceUsageType = this.__TypeHandle.__Game_Buildings_ServiceUsage_RW_ComponentTypeHandle,
                m_StudentType = this.__TypeHandle.__Game_Buildings_Student_RW_BufferTypeHandle,
                m_Prefabs = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup,
                m_SchoolDatas = this.__TypeHandle.__Game_Prefabs_SchoolData_RO_ComponentLookup,
                m_Students = this.__TypeHandle.__Game_Citizens_Student_RO_ComponentLookup,
                m_Citizens = this.__TypeHandle.__Game_Citizens_Citizen_RO_ComponentLookup,
                m_TravelPurposes = this.__TypeHandle.__Game_Citizens_TravelPurpose_RO_ComponentLookup,
                m_CityModifiers = this.__TypeHandle.__Game_City_CityModifier_RO_BufferLookup,
                m_Fees = this.__TypeHandle.__Game_City_ServiceFee_RO_BufferLookup,
                m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_EconomyParameters = this.__query_1235104412_0.GetSingleton<EconomyParameterData>(),
                m_EducationParameters = this.__query_1235104412_1.GetSingleton<EducationParameterData>(),
                m_TimeData = this.__query_1235104412_2.GetSingleton<TimeData>(),
                m_RandomSeed = RandomSeed.Next(),
                m_City = this.m_CitySystem.City,
                m_SimulationFrame = this.m_SimulationSystem.frameIndex,
                elementary_grad_probability = Mod.m_Setting.elementary_grad_prob,
                high_grad_probability = Mod.m_Setting.high_grad_prob,
                college_grad_probability = Mod.m_Setting.college_grad_prob,
                university_grad_probability = Mod.m_Setting.university_grad_prob,
                adult_age_limit = Mod.m_Setting.adult_age_limit
            };
            this.Dependency = jobData.ScheduleParallel<RealLifeSchoolAISystem.SchoolTickJob>(this.m_SchoolQuery, this.Dependency);
            this.m_EndFrameBarrier.AddJobHandleForProducer(this.Dependency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void __AssignQueries(ref SystemState state)
        {
            this.__query_1235104412_0 = state.GetEntityQuery(new EntityQueryDesc()
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
            this.__query_1235104412_1 = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[1]
              {
          ComponentType.ReadOnly<EducationParameterData>()
              },
                Any = new ComponentType[0],
                None = new ComponentType[0],
                Disabled = new ComponentType[0],
                Absent = new ComponentType[0],
                Options = EntityQueryOptions.IncludeSystems
            });
            this.__query_1235104412_2 = state.GetEntityQuery(new EntityQueryDesc()
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
        public RealLifeSchoolAISystem()
        {
        }

        [BurstCompile]
        private struct SchoolTickJob : IJobChunk
        {
            [ReadOnly]
            public ComponentTypeHandle<PrefabRef> m_PrefabRefType;
            [ReadOnly]
            public BufferTypeHandle<InstalledUpgrade> m_InstalledUpgradeType;
            [ReadOnly]
            public BufferTypeHandle<Efficiency> m_EfficiencyType;
            public ComponentTypeHandle<Game.Buildings.School> m_SchoolType;
            public ComponentTypeHandle<ServiceUsage> m_ServiceUsageType;
            public BufferTypeHandle<Game.Buildings.Student> m_StudentType;
            [ReadOnly]
            public ComponentLookup<PrefabRef> m_Prefabs;
            [ReadOnly]
            public ComponentLookup<SchoolData> m_SchoolDatas;
            [ReadOnly]
            public ComponentLookup<Game.Citizens.Student> m_Students;
            [ReadOnly]
            public ComponentLookup<Citizen> m_Citizens;
            [ReadOnly]
            public ComponentLookup<TravelPurpose> m_TravelPurposes;
            [ReadOnly]
            public BufferLookup<CityModifier> m_CityModifiers;
            [ReadOnly]
            public BufferLookup<ServiceFee> m_Fees;
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            public EconomyParameterData m_EconomyParameters;
            public EducationParameterData m_EducationParameters;
            public TimeData m_TimeData;
            public RandomSeed m_RandomSeed;
            public Entity m_City;
            public uint m_SimulationFrame;
            public float elementary_grad_probability;
            public float high_grad_probability;
            public float college_grad_probability;
            public float university_grad_probability;
            public int adult_age_limit;

            public void Execute(
              in ArchetypeChunk chunk,
              int unfilteredChunkIndex,
              bool useEnabledMask,
              in v128 chunkEnabledMask)
            {
                DynamicBuffer<CityModifier> cityModifier = this.m_CityModifiers[this.m_City];
                NativeArray<PrefabRef> nativeArray1 = chunk.GetNativeArray<PrefabRef>(ref this.m_PrefabRefType);
                BufferAccessor<InstalledUpgrade> bufferAccessor1 = chunk.GetBufferAccessor<InstalledUpgrade>(ref this.m_InstalledUpgradeType);
                BufferAccessor<Efficiency> bufferAccessor2 = chunk.GetBufferAccessor<Efficiency>(ref this.m_EfficiencyType);
                NativeArray<Game.Buildings.School> nativeArray2 = chunk.GetNativeArray<Game.Buildings.School>(ref this.m_SchoolType);
                BufferAccessor<Game.Buildings.Student> bufferAccessor3 = chunk.GetBufferAccessor<Game.Buildings.Student>(ref this.m_StudentType);
                NativeArray<ServiceUsage> nativeArray3 = chunk.GetNativeArray<ServiceUsage>(ref this.m_ServiceUsageType);
                Random random = this.m_RandomSeed.GetRandom(unfilteredChunkIndex);
                for (int index1 = 0; index1 < chunk.Count; ++index1)
                {
                    Entity prefab = nativeArray1[index1].m_Prefab;
                    ref Game.Buildings.School local = ref nativeArray2.ElementAt<Game.Buildings.School>(index1);
                    float efficiency = BuildingUtils.GetEfficiency(bufferAccessor2, index1);
                    SchoolData componentData1;

                    this.m_SchoolDatas.TryGetComponent(prefab, out componentData1);
                    if (bufferAccessor1.Length != 0)
                    {
                        UpgradeUtils.CombineStats<SchoolData>(ref componentData1, bufferAccessor1[index1], ref this.m_Prefabs, ref this.m_SchoolDatas);
                    }

                    float fee = ServiceFeeSystem.GetFee(ServiceFeeSystem.GetEducationResource((int)componentData1.m_EducationLevel), this.m_Fees[this.m_City]);
                    DynamicBuffer<Game.Buildings.Student> dynamicBuffer = bufferAccessor3[index1];
                    float num1 = 0.0f;
                    float num2 = 0.0f;
                    int num3 = 0;
                    for (int index2 = dynamicBuffer.Length - 1; index2 >= 0; --index2)
                    {
                        Game.Citizens.Student componentData2;
                        Citizen componentData3;

                        if (this.m_Students.TryGetComponent((Entity)dynamicBuffer[index2], out componentData2) && this.m_Citizens.TryGetComponent((Entity)dynamicBuffer[index2], out componentData3))
                        {
                            if ((double)efficiency <= 1.0 / 1000.0 && (double)random.NextFloat() < (double)this.m_EducationParameters.m_InoperableSchoolLeaveProbability)
                            {
                                this.LeaveSchool(unfilteredChunkIndex, (Entity)dynamicBuffer[index2]);
                                dynamicBuffer.RemoveAt(index2);
                            }
                            else
                            {
                                int failedEducationCount = componentData3.GetFailedEducationCount();
                                float ageInDays = componentData3.GetAgeInDays(this.m_SimulationFrame, this.m_TimeData);
                                float graduationProbability = RealLifeGraduationSystem.GetGraduationProbability((int)componentData2.m_Level, (int)componentData3.m_WellBeing, componentData1, cityModifier, 0.5f, efficiency, elementary_grad_probability, high_grad_probability, college_grad_probability, university_grad_probability);
                                if ((double)graduationProbability > 1.0 / 1000.0)
                                {
                                    num1 += math.min(4f, (float)((double)failedEducationCount + 0.5 - 1.0 / (double)math.log2(1f - math.saturate(graduationProbability)))) / 1f;
                                    float num4 = math.pow(1f - math.saturate(graduationProbability), 4f);
                                    float num5 = math.saturate(RealLifeGraduationSystem.GetDropoutProbability((int)componentData2.m_Level, componentData2.m_LastCommuteTime, fee, 0, ageInDays, 0.5f, failedEducationCount, graduationProbability, ref this.m_EconomyParameters, adult_age_limit, elementary_grad_probability, high_grad_probability, college_grad_probability, university_grad_probability));
                                    num2 += math.saturate(num4 + num5);
                                }
                                else
                                {
                                    num1 += 4f;
                                    ++num2;
                                }
                                ++num3;
                            }
                        }
                        else
                            dynamicBuffer.RemoveAt(index2);
                    }
                    if (num3 > 0)
                    {
                        local.m_AverageGraduationTime = num1 / (float)num3;
                        local.m_AverageFailProbability = num2 / (float)num3;
                    }
                    else
                    {
                        float graduationProbability = RealLifeGraduationSystem.GetGraduationProbability((int)componentData1.m_EducationLevel, 50, componentData1, cityModifier, 0.5f, efficiency, elementary_grad_probability, high_grad_probability, college_grad_probability, university_grad_probability);
                        local.m_AverageGraduationTime = (float)(0.5 - 1.0 / (double)math.log2(1f - math.saturate(graduationProbability)));
                        local.m_AverageFailProbability = 0.0f;
                    }

                    local.m_StudentWellbeing = (sbyte)math.clamp((int)math.round(efficiency * (float)componentData1.m_StudentWellbeing), -100, 100);
                    local.m_StudentHealth = (sbyte)math.clamp((int)math.round(efficiency * (float)componentData1.m_StudentHealth), -100, 100);
                    if (nativeArray3.Length != 0)
                        nativeArray3[index1] = new ServiceUsage()
                        {
                            m_Usage = 1f * (float)num3 / (float)componentData1.m_StudentCapacity
                        };
                }
            }

            private void LeaveSchool(int chunkIndex, Entity entity)
            {
                this.m_CommandBuffer.RemoveComponent<Game.Citizens.Student>(chunkIndex, entity);
                TravelPurpose componentData;
                if (!this.m_TravelPurposes.TryGetComponent(entity, out componentData))
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
            public ComponentTypeHandle<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
            [ReadOnly]
            public BufferTypeHandle<InstalledUpgrade> __Game_Buildings_InstalledUpgrade_RO_BufferTypeHandle;
            [ReadOnly]
            public BufferTypeHandle<Efficiency> __Game_Buildings_Efficiency_RO_BufferTypeHandle;
            public ComponentTypeHandle<Game.Buildings.School> __Game_Buildings_School_RW_ComponentTypeHandle;
            public ComponentTypeHandle<ServiceUsage> __Game_Buildings_ServiceUsage_RW_ComponentTypeHandle;
            public BufferTypeHandle<Game.Buildings.Student> __Game_Buildings_Student_RW_BufferTypeHandle;
            [ReadOnly]
            public ComponentLookup<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<SchoolData> __Game_Prefabs_SchoolData_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<Game.Citizens.Student> __Game_Citizens_Student_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<Citizen> __Game_Citizens_Citizen_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<TravelPurpose> __Game_Citizens_TravelPurpose_RO_ComponentLookup;
            [ReadOnly]
            public BufferLookup<CityModifier> __Game_City_CityModifier_RO_BufferLookup;
            [ReadOnly]
            public BufferLookup<ServiceFee> __Game_City_ServiceFee_RO_BufferLookup;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                this.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PrefabRef>(true);
                this.__Game_Buildings_InstalledUpgrade_RO_BufferTypeHandle = state.GetBufferTypeHandle<InstalledUpgrade>(true);
                this.__Game_Buildings_Efficiency_RO_BufferTypeHandle = state.GetBufferTypeHandle<Efficiency>(true);
                this.__Game_Buildings_School_RW_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Buildings.School>();
                this.__Game_Buildings_ServiceUsage_RW_ComponentTypeHandle = state.GetComponentTypeHandle<ServiceUsage>();
                this.__Game_Buildings_Student_RW_BufferTypeHandle = state.GetBufferTypeHandle<Game.Buildings.Student>();
                this.__Game_Prefabs_PrefabRef_RO_ComponentLookup = state.GetComponentLookup<PrefabRef>(true);
                this.__Game_Prefabs_SchoolData_RO_ComponentLookup = state.GetComponentLookup<SchoolData>(true);
                this.__Game_Citizens_Student_RO_ComponentLookup = state.GetComponentLookup<Game.Citizens.Student>(true);
                this.__Game_Citizens_Citizen_RO_ComponentLookup = state.GetComponentLookup<Citizen>(true);
                this.__Game_Citizens_TravelPurpose_RO_ComponentLookup = state.GetComponentLookup<TravelPurpose>(true);
                this.__Game_City_CityModifier_RO_BufferLookup = state.GetBufferLookup<CityModifier>(true);
                this.__Game_City_ServiceFee_RO_BufferLookup = state.GetBufferLookup<ServiceFee>(true);
            }
        }
    }
}
