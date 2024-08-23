
using Colossal.Collections;
using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game;
using Game.Simulation;
using Game.Agents;
using Game.Buildings;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Companies;
using Game.Debug;
using Game.Prefabs;
using Game.Reflection;
using Game.Tools;
using Game.Triggers;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

#nullable disable
namespace RealLife.Systems
{
    public partial class RealLifeResidentialDemandSystem : GameSystemBase, IDefaultSerializable, ISerializable
    {
        public static readonly int kMaxFactorEffect = 15;
        public static readonly int kInitialEmptyBuildingEffect = 50;
        private TaxSystem m_TaxSystem;
        private CountEmploymentSystem m_CountEmploymentSystem;
        private CountStudyPositionsSystem m_CountStudyPositionsSystem;
        private CountWorkplacesSystem m_CountWorkplacesSystem;
        private CountHouseholdDataSystem m_CountHouseholdDataSystem;
        private CitySystem m_CitySystem;
        private TriggerSystem m_TriggerSystem;
        private EntityQuery m_DemandParameterGroup;
        private EntityQuery m_AllHouseholdGroup;
        private EntityQuery m_AllResidentialGroup;
        private EntityQuery m_UnlockedZoneQuery;
        [DebugWatchValue(color = "#27ae60")]
        private NativeValue<int> m_HouseholdDemand;
        [DebugWatchValue(color = "#117a65")]
        private NativeValue<int3> m_BuildingDemand;
        [DebugWatchValue]
        private NativeValue<int3> m_FreeProperties;
        [EnumArray(typeof(DemandFactor))]
        [DebugWatchValue]
        private NativeArray<int> m_LowDemandFactors;
        [EnumArray(typeof(DemandFactor))]
        [DebugWatchValue]
        private NativeArray<int> m_MediumDemandFactors;
        [EnumArray(typeof(DemandFactor))]
        [DebugWatchValue]
        private NativeArray<int> m_HighDemandFactors;
        [DebugWatchDeps]
        private JobHandle m_WriteDependencies;
        private JobHandle m_ReadDependencies;
        private int m_LastHouseholdDemand;
        private int3 m_LastBuildingDemand;
        private RealLifeResidentialDemandSystem.TypeHandle __TypeHandle;

        public override int GetUpdateInterval(SystemUpdatePhase phase) => 16;

        public override int GetUpdateOffset(SystemUpdatePhase phase) => 10;

        public int householdDemand => this.m_LastHouseholdDemand;

        public int3 buildingDemand => this.m_LastBuildingDemand;

        public NativeArray<int> GetLowDensityDemandFactors(out JobHandle deps)
        {
            deps = this.m_WriteDependencies;
            return this.m_LowDemandFactors;
        }

        public NativeArray<int> GetMediumDensityDemandFactors(out JobHandle deps)
        {
            deps = this.m_WriteDependencies;
            return this.m_MediumDemandFactors;
        }

        public NativeArray<int> GetHighDensityDemandFactors(out JobHandle deps)
        {
            deps = this.m_WriteDependencies;
            return this.m_HighDemandFactors;
        }

        public void AddReader(JobHandle reader)
        {
            this.m_ReadDependencies = JobHandle.CombineDependencies(this.m_ReadDependencies, reader);
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            this.m_DemandParameterGroup = this.GetEntityQuery(ComponentType.ReadOnly<DemandParameterData>());
            this.m_AllHouseholdGroup = this.GetEntityQuery(ComponentType.ReadOnly<Household>(), ComponentType.Exclude<TouristHousehold>(), ComponentType.Exclude<CommuterHousehold>(), ComponentType.Exclude<MovingAway>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Temp>());
            this.m_AllResidentialGroup = this.GetEntityQuery(ComponentType.ReadOnly<ResidentialProperty>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Condemned>(), ComponentType.Exclude<Abandoned>(), ComponentType.Exclude<Destroyed>(), ComponentType.Exclude<Temp>());
            this.m_UnlockedZoneQuery = this.GetEntityQuery(ComponentType.ReadOnly<ZoneData>(), ComponentType.ReadOnly<ZonePropertiesData>(), ComponentType.Exclude<Locked>());
            this.m_CitySystem = this.World.GetOrCreateSystemManaged<CitySystem>();
            this.m_TaxSystem = this.World.GetOrCreateSystemManaged<TaxSystem>();
            this.m_CountEmploymentSystem = this.World.GetOrCreateSystemManaged<CountEmploymentSystem>();
            this.m_CountStudyPositionsSystem = this.World.GetOrCreateSystemManaged<CountStudyPositionsSystem>();
            this.m_CountWorkplacesSystem = this.World.GetOrCreateSystemManaged<CountWorkplacesSystem>();
            this.m_CountHouseholdDataSystem = this.World.GetOrCreateSystemManaged<CountHouseholdDataSystem>();
            this.m_TriggerSystem = this.World.GetOrCreateSystemManaged<TriggerSystem>();
            this.m_HouseholdDemand = new NativeValue<int>(Allocator.Persistent);
            this.m_BuildingDemand = new NativeValue<int3>(Allocator.Persistent);
            this.m_FreeProperties = new NativeValue<int3>(Allocator.Persistent);
            this.m_LowDemandFactors = new NativeArray<int>(18, Allocator.Persistent);
            this.m_MediumDemandFactors = new NativeArray<int>(18, Allocator.Persistent);
            this.m_HighDemandFactors = new NativeArray<int>(18, Allocator.Persistent);
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnDestroy()
        {
            this.m_HouseholdDemand.Dispose();
            this.m_BuildingDemand.Dispose();
            this.m_FreeProperties.Dispose();
            this.m_LowDemandFactors.Dispose();
            this.m_MediumDemandFactors.Dispose();
            this.m_HighDemandFactors.Dispose();
            base.OnDestroy();
        }

        public void SetDefaults(Colossal.Serialization.Entities.Context context)
        {
            this.m_HouseholdDemand.value = 0;
            this.m_BuildingDemand.value = new int3();
            this.m_LowDemandFactors.Fill<int>(0);
            this.m_MediumDemandFactors.Fill<int>(0);
            this.m_HighDemandFactors.Fill<int>(0);
            this.m_LastHouseholdDemand = 0;
            this.m_LastBuildingDemand = new int3();
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(this.m_HouseholdDemand.value);
            writer.Write(this.m_BuildingDemand.value);
            writer.Write(this.m_LowDemandFactors.Length);
            writer.Write(this.m_LowDemandFactors);
            writer.Write(this.m_MediumDemandFactors);
            writer.Write(this.m_HighDemandFactors);
            writer.Write(this.m_LastHouseholdDemand);
            writer.Write(this.m_LastBuildingDemand);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            int num1;
            reader.Read(out num1);
            this.m_HouseholdDemand.value = num1;
            if (reader.context.version < Version.residentialDemandSplit)
            {
                int num2;
                reader.Read(out num2);
                this.m_BuildingDemand.value = new int3(num2 / 3, num2 / 3, num2 / 3);
            }
            else
            {
                int3 int3;
                reader.Read(out int3);
                this.m_BuildingDemand.value = int3;
            }
            if (reader.context.version < Version.demandFactorCountSerialization)
            {
                NativeArray<int> src = new NativeArray<int>(13, Allocator.Temp);
                reader.Read(src);
                CollectionUtils.CopySafe<int>(src, this.m_LowDemandFactors);
                src.Dispose();
            }
            else
            {
                int length;
                reader.Read(out length);
                if (length == this.m_LowDemandFactors.Length)
                {
                    reader.Read(this.m_LowDemandFactors);
                    reader.Read(this.m_MediumDemandFactors);
                    reader.Read(this.m_HighDemandFactors);
                }
                else
                {
                    NativeArray<int> src = new NativeArray<int>(length, Allocator.Temp);
                    reader.Read(src);
                    CollectionUtils.CopySafe<int>(src, this.m_LowDemandFactors);
                    reader.Read(src);
                    CollectionUtils.CopySafe<int>(src, this.m_MediumDemandFactors);
                    reader.Read(src);
                    CollectionUtils.CopySafe<int>(src, this.m_HighDemandFactors);
                    src.Dispose();
                }
            }
            reader.Read(out this.m_LastHouseholdDemand);
            if (reader.context.version < Version.residentialDemandSplit)
            {
                int num3;
                reader.Read(out num3);
                this.m_LastBuildingDemand = new int3(num3 / 3, num3 / 3, num3 / 3);
            }
            else
            {
                reader.Read(out this.m_LastBuildingDemand);
            }
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnUpdate()
        {
            if (this.m_DemandParameterGroup.IsEmptyIgnoreFilter)
                return;
            this.m_LastHouseholdDemand = this.m_HouseholdDemand.value;
            this.m_LastBuildingDemand = this.m_BuildingDemand.value;
            this.__TypeHandle.__Game_Prefabs_ZonePropertiesData_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_City_Population_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_Household_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Citizens_HomelessHousehold_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_Renter_RO_BufferTypeHandle.Update(ref this.CheckedStateRef);
            JobHandle outJobHandle1;
            JobHandle outJobHandle2;
            JobHandle outJobHandle3;
            JobHandle deps1;
            JobHandle deps2;

            RealLifeResidentialDemandSystem.UpdateResidentialDemandJob jobData = new RealLifeResidentialDemandSystem.UpdateResidentialDemandJob()
            {
                m_ResidentialChunks = this.m_AllResidentialGroup.ToArchetypeChunkListAsync((AllocatorManager.AllocatorHandle)this.World.UpdateAllocator.ToAllocator, out outJobHandle1),
                m_HouseholdChunks = this.m_AllHouseholdGroup.ToArchetypeChunkListAsync((AllocatorManager.AllocatorHandle)this.World.UpdateAllocator.ToAllocator, out outJobHandle2),
                m_UnlockedZones = this.m_UnlockedZoneQuery.ToComponentDataArray<ZonePropertiesData>((AllocatorManager.AllocatorHandle)Allocator.TempJob),
                m_RenterType = this.__TypeHandle.__Game_Buildings_Renter_RO_BufferTypeHandle,
                m_PrefabType = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle,
                m_HomelessHouseholdType = this.__TypeHandle.__Game_Citizens_HomelessHousehold_RO_ComponentTypeHandle,
                m_BuildingPropertyDatas = this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup,
                m_Households = this.__TypeHandle.__Game_Citizens_Household_RO_ComponentLookup,
                m_Populations = this.__TypeHandle.__Game_City_Population_RO_ComponentLookup,
                m_SpawnableDatas = this.__TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup,
                m_ZonePropertyDatas = this.__TypeHandle.__Game_Prefabs_ZonePropertiesData_RO_ComponentLookup,
                m_DemandParameters = this.m_DemandParameterGroup.ToComponentDataListAsync<DemandParameterData>((AllocatorManager.AllocatorHandle)this.World.UpdateAllocator.ToAllocator, out outJobHandle3),
                m_StudyPositions = this.m_CountStudyPositionsSystem.GetStudyPositionsByEducation(out deps1),
                m_FreeWorkplaces = this.m_CountWorkplacesSystem.GetFreeWorkplaces(),
                m_TotalWorkplaces = this.m_CountWorkplacesSystem.GetTotalWorkplaces(),
                m_HouseholdCountData = this.m_CountHouseholdDataSystem.GetHouseholdCountData(),
                m_UnemploymentRate = this.m_CountEmploymentSystem.GetUnemployment(out deps2),
                m_TaxRates = this.m_TaxSystem.GetTaxRates(),
                m_City = this.m_CitySystem.City,
                m_HouseholdDemand = this.m_HouseholdDemand,
                m_BuildingDemand = this.m_BuildingDemand,
                m_FreeProperties = this.m_FreeProperties,
                m_LowDemandFactors = this.m_LowDemandFactors,
                m_MediumDemandFactors = this.m_MediumDemandFactors,
                m_HighDemandFactors = this.m_HighDemandFactors,
                m_TriggerQueue = this.m_TriggerSystem.CreateActionBuffer()
            };
            this.Dependency = jobData.Schedule<RealLifeResidentialDemandSystem.UpdateResidentialDemandJob>(JobUtils.CombineDependencies(this.Dependency, this.m_ReadDependencies, outJobHandle1, outJobHandle2, outJobHandle3, deps1, deps2));
            this.m_WriteDependencies = this.Dependency;
            this.m_CountEmploymentSystem.AddReader(this.Dependency);
            this.m_CountStudyPositionsSystem.AddReader(this.Dependency);
            this.m_TaxSystem.AddReader(this.Dependency);
            this.m_TriggerSystem.AddActionBufferWriter(this.Dependency);
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
        public RealLifeResidentialDemandSystem()
        {
        }

        //[BurstCompile]
        private struct UpdateResidentialDemandJob : IJob
        {
            [ReadOnly]
            public NativeList<ArchetypeChunk> m_ResidentialChunks;
            [ReadOnly]
            public NativeList<ArchetypeChunk> m_HouseholdChunks;
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<ZonePropertiesData> m_UnlockedZones;
            [ReadOnly]
            public BufferTypeHandle<Renter> m_RenterType;
            [ReadOnly]
            public ComponentTypeHandle<PrefabRef> m_PrefabType;
            [ReadOnly]
            public ComponentTypeHandle<HomelessHousehold> m_HomelessHouseholdType;
            [ReadOnly]
            public ComponentLookup<BuildingPropertyData> m_BuildingPropertyDatas;
            [ReadOnly]
            public ComponentLookup<Household> m_Households;
            [ReadOnly]
            public ComponentLookup<Population> m_Populations;
            [ReadOnly]
            public ComponentLookup<SpawnableBuildingData> m_SpawnableDatas;
            [ReadOnly]
            public ComponentLookup<ZonePropertiesData> m_ZonePropertyDatas;
            [ReadOnly]
            public NativeList<DemandParameterData> m_DemandParameters;
            [ReadOnly]
            public NativeArray<int> m_StudyPositions;
            [ReadOnly]
            public NativeArray<int> m_TaxRates;
            [ReadOnly]
            public NativeValue<int2> m_UnemploymentRate;
            public Entity m_City;
            public NativeValue<int> m_HouseholdDemand;
            public NativeValue<int3> m_BuildingDemand;
            public NativeValue<int3> m_FreeProperties;
            public NativeArray<int> m_LowDemandFactors;
            public NativeArray<int> m_MediumDemandFactors;
            public NativeArray<int> m_HighDemandFactors;
            public CountHouseholdDataSystem.HouseholdData m_HouseholdCountData;
            public Workplaces m_FreeWorkplaces;
            public Workplaces m_TotalWorkplaces;
            public NativeQueue<TriggerAction> m_TriggerQueue;

            public void Execute()
            {
                bool3 c = new bool3();
                for (int index = 0; index < this.m_UnlockedZones.Length; ++index)
                {
                    if ((double)this.m_UnlockedZones[index].m_ResidentialProperties > 0.0)
                    {
                        float num = this.m_UnlockedZones[index].m_ResidentialProperties / this.m_UnlockedZones[index].m_SpaceMultiplier;
                        if (!this.m_UnlockedZones[index].m_ScaleResidentials)
                            c.x = true;
                        else if ((double)num < 1.0)
                            c.y = true;
                        else
                            c.z = true;
                    }
                }
                int3 int3_1 = new int3();
                int3 int3_2 = new int3();

                DemandParameterData demandParameter = this.m_DemandParameters[0];
                int num1 = 0;
                for (int index = 1; index <= 4; ++index)
                {
                    num1 += this.m_StudyPositions[index];
                }
                Population population = this.m_Populations[this.m_City];
                float num2 = 20f - math.smoothstep(0.0f, 20f, (float)population.m_Population / 20000f);
                int num3 = math.max(demandParameter.m_MinimumHappiness, population.m_AverageHappiness);
                int num4 = 0;
                int num5 = 0;

                for (int index = 0; index < this.m_HouseholdChunks.Length; ++index)
                {
                    ArchetypeChunk householdChunk = this.m_HouseholdChunks[index];
                    if (householdChunk.Has<HomelessHousehold>(ref this.m_HomelessHouseholdType))
                        num4 += householdChunk.Count;
                    num5 += householdChunk.Count;
                }
                float num6 = 0.0f;
                for (int jobLevel = 0; jobLevel < 5; ++jobLevel)
                {
                    num6 += (float)-(TaxSystem.GetResidentialTaxRate(jobLevel, this.m_TaxRates) - 10);
                }
                float f1 = demandParameter.m_TaxEffect * (num6 / 5f);
                float f2 = demandParameter.m_HappinessEffect * (float)(num3 - demandParameter.m_NeutralHappiness);
                float f3 = math.clamp((float)(-(double)demandParameter.m_HomelessEffect * (100.0 * (double)num4 / (1.0 + (double)num5) - (double)demandParameter.m_NeutralHomelessness)), (float)-RealLifeResidentialDemandSystem.kMaxFactorEffect, (float)RealLifeResidentialDemandSystem.kMaxFactorEffect);
                float num7 = math.clamp(demandParameter.m_AvailableWorkplaceEffect * ((float)this.m_FreeWorkplaces.SimpleWorkplacesCount - (float)((double)this.m_TotalWorkplaces.SimpleWorkplacesCount * (double)demandParameter.m_NeutralAvailableWorkplacePercentage / 100.0)), 0.0f, 40f);
                float y = math.clamp(demandParameter.m_AvailableWorkplaceEffect * ((float)this.m_FreeWorkplaces.ComplexWorkplacesCount - (float)((double)this.m_TotalWorkplaces.ComplexWorkplacesCount * (double)demandParameter.m_NeutralAvailableWorkplacePercentage / 100.0)), 0.0f, 20f);
                float f4 = demandParameter.m_StudentEffect * math.clamp((float)num1 / 200f, 0.0f, 20f);
                float f5 = demandParameter.m_NeutralUnemployment - (float)this.m_UnemploymentRate.value.x;
                Mod.log.Info($"f1: {f1}");
                Mod.log.Info($"f2: {f2}");
                Mod.log.Info($"f3: {f3}");
                Mod.log.Info($"y: {y}");
                Mod.log.Info($"num7: {num7}");
                Mod.log.Info($"f4: {f4}");
                Mod.log.Info($"f5: {f5}");
                this.m_HouseholdDemand.value = math.min(200, math.max(0, (int)((double)num2 + (double)f2 + (double)f3 + (double)f1 + (double)f5 + (double)f4 + (double)math.max(num7, y))));
                Mod.log.Info($"this.m_HouseholdDemand.value: {this.m_HouseholdDemand.value}");
                for (int index1 = 0; index1 < this.m_ResidentialChunks.Length; ++index1)
                {
                    ArchetypeChunk residentialChunk = this.m_ResidentialChunks[index1];
                    NativeArray<PrefabRef> nativeArray = residentialChunk.GetNativeArray<PrefabRef>(ref this.m_PrefabType);
                    BufferAccessor<Renter> bufferAccessor = residentialChunk.GetBufferAccessor<Renter>(ref this.m_RenterType);
                    for (int index2 = 0; index2 < nativeArray.Length; ++index2)
                    {
                        Entity prefab = nativeArray[index2].m_Prefab;
                        SpawnableBuildingData spawnableData = this.m_SpawnableDatas[prefab];
                        ZonePropertiesData componentData;
                        if (this.m_BuildingPropertyDatas.HasComponent(prefab) && this.m_ZonePropertyDatas.TryGetComponent(spawnableData.m_ZonePrefab, out componentData))
                        {
                            float num8 = componentData.m_ResidentialProperties / componentData.m_SpaceMultiplier;
                            BuildingPropertyData buildingPropertyData = this.m_BuildingPropertyDatas[prefab];
                            DynamicBuffer<Renter> dynamicBuffer = bufferAccessor[index2];
                            int num9 = 0;
                            for (int index3 = 0; index3 < dynamicBuffer.Length; ++index3)
                            {
                                if (this.m_Households.HasComponent(dynamicBuffer[index3].m_Renter))
                                    ++num9;
                            }
                            if (!componentData.m_ScaleResidentials)
                            {
                                ++int3_2.x;
                                if (residentialChunk.Has<PropertyOnMarket>() || residentialChunk.Has<PropertyToBeOnMarket>())
                                    int3_1.x += 1 - num9;
                            }
                            else if ((double)num8 < 1.0)
                            {
                                int3_2.y += buildingPropertyData.m_ResidentialProperties;
                                if (residentialChunk.Has<PropertyOnMarket>() || residentialChunk.Has<PropertyToBeOnMarket>())
                                    int3_1.y += buildingPropertyData.m_ResidentialProperties - num9;
                            }
                            else
                            {
                                int3_2.z += buildingPropertyData.m_ResidentialProperties;
                                if (residentialChunk.Has<PropertyOnMarket>() || residentialChunk.Has<PropertyToBeOnMarket>())
                                    int3_1.z += buildingPropertyData.m_ResidentialProperties - num9;
                            }
                        }
                    }
                }

                this.m_FreeProperties.value = int3_1;
                int num10 = Mathf.RoundToInt(100f * (float)(demandParameter.m_FreeResidentialRequirement.x - int3_1.x) / (float)demandParameter.m_FreeResidentialRequirement.x);
                int num11 = Mathf.RoundToInt(100f * (float)(demandParameter.m_FreeResidentialRequirement.y - int3_1.y) / (float)demandParameter.m_FreeResidentialRequirement.y);
                int num12 = Mathf.RoundToInt(100f * (float)(demandParameter.m_FreeResidentialRequirement.z - int3_1.z) / (float)demandParameter.m_FreeResidentialRequirement.z);
                Mod.log.Info($"num10: {num10}");
                this.m_LowDemandFactors[7] = Mathf.RoundToInt(f2);
                this.m_LowDemandFactors[8] = Mathf.RoundToInt(f3);
                this.m_LowDemandFactors[6] = Mathf.RoundToInt(num7) / 2;
                this.m_LowDemandFactors[5] = Mathf.RoundToInt(f5);
                this.m_LowDemandFactors[11] = Mathf.RoundToInt(f1);
                this.m_LowDemandFactors[13] = num10;
                this.m_MediumDemandFactors[7] = Mathf.RoundToInt(f2);
                this.m_MediumDemandFactors[8] = Mathf.RoundToInt(f3);
                this.m_MediumDemandFactors[6] = Mathf.RoundToInt(num7);
                this.m_MediumDemandFactors[5] = Mathf.RoundToInt(f5);
                this.m_MediumDemandFactors[11] = Mathf.RoundToInt(f1);
                this.m_MediumDemandFactors[12] = Mathf.RoundToInt(f4);
                this.m_MediumDemandFactors[13] = num11;
                this.m_HighDemandFactors[7] = Mathf.RoundToInt(f2);
                this.m_HighDemandFactors[8] = Mathf.RoundToInt(f3);
                this.m_HighDemandFactors[6] = Mathf.RoundToInt(num7);
                this.m_HighDemandFactors[5] = Mathf.RoundToInt(f5);
                this.m_HighDemandFactors[11] = Mathf.RoundToInt(f1);
                this.m_HighDemandFactors[12] = Mathf.RoundToInt(f4);
                this.m_HighDemandFactors[13] = num12;
                int num13 = this.m_LowDemandFactors[13] >= 0 ? this.m_LowDemandFactors[7] + this.m_LowDemandFactors[8] + this.m_LowDemandFactors[11] + this.m_LowDemandFactors[6] + this.m_LowDemandFactors[5] + this.m_LowDemandFactors[13] : 0;
                int num14 = this.m_MediumDemandFactors[13] >= 0 ? this.m_MediumDemandFactors[7] + this.m_MediumDemandFactors[8] + this.m_MediumDemandFactors[11] + this.m_MediumDemandFactors[6] + this.m_MediumDemandFactors[12] + this.m_MediumDemandFactors[5] + this.m_MediumDemandFactors[13] : 0;
                int num15 = this.m_HighDemandFactors[13] >= 0 ? this.m_HighDemandFactors[7] + this.m_HighDemandFactors[8] + this.m_HighDemandFactors[11] + this.m_HighDemandFactors[6] + this.m_HighDemandFactors[12] + this.m_HighDemandFactors[5] + this.m_HighDemandFactors[13] : 0;
                this.m_BuildingDemand.value = new int3(math.clamp(this.m_HouseholdDemand.value / 2 + num10 + num13, 0, 100), math.clamp(this.m_HouseholdDemand.value / 2 + num11 + num14, 0, 100), math.clamp(this.m_HouseholdDemand.value / 2 + num12 + num15, 0, 100));
                this.m_BuildingDemand.value = math.select(new int3(), this.m_BuildingDemand.value, c);
                this.m_TriggerQueue.Enqueue(new TriggerAction(TriggerType.ResidentialDemand, Entity.Null, (float)(this.m_BuildingDemand.value.x + this.m_BuildingDemand.value.y + this.m_BuildingDemand.value.z) / 100f));
            }
        }

        private struct TypeHandle
        {
            [ReadOnly]
            public BufferTypeHandle<Renter> __Game_Buildings_Renter_RO_BufferTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<HomelessHousehold> __Game_Citizens_HomelessHousehold_RO_ComponentTypeHandle;
            [ReadOnly]
            public ComponentLookup<BuildingPropertyData> __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<Household> __Game_Citizens_Household_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<Population> __Game_City_Population_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<SpawnableBuildingData> __Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<ZonePropertiesData> __Game_Prefabs_ZonePropertiesData_RO_ComponentLookup;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                this.__Game_Buildings_Renter_RO_BufferTypeHandle = state.GetBufferTypeHandle<Renter>(true);
                this.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PrefabRef>(true);
                this.__Game_Citizens_HomelessHousehold_RO_ComponentTypeHandle = state.GetComponentTypeHandle<HomelessHousehold>(true);
                this.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup = state.GetComponentLookup<BuildingPropertyData>(true);
                this.__Game_Citizens_Household_RO_ComponentLookup = state.GetComponentLookup<Household>(true);
                this.__Game_City_Population_RO_ComponentLookup = state.GetComponentLookup<Population>(true);
                this.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup = state.GetComponentLookup<SpawnableBuildingData>(true);
                this.__Game_Prefabs_ZonePropertiesData_RO_ComponentLookup = state.GetComponentLookup<ZonePropertiesData>(true);
            }
        }
    }
}
