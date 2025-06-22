
using Game.Areas;
using Game.Buildings;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Companies;
using Game.Economy;
using Game.Pathfind;
using Game.Prefabs;
using Game.Triggers;
using Game.Vehicles;
using Game.Simulation;
using Game;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Internal;
using Unity.Jobs;
using Unity.Mathematics;

#nullable disable
namespace RealLife.Systems
{
    public partial class RealLifeFindSchoolSystem : GameSystemBase
    {
        public bool debugFastFindSchool;
        private PathfindSetupSystem m_PathfindSetupSystem;
        private SimulationSystem m_SimulationSystem;
        private CitySystem m_CitySystem;
        private EndFrameBarrier m_EndFrameBarrier;
        private EntityQuery m_SchoolSeekerQuery;
        private EntityQuery m_ResultsQuery;
        private TriggerSystem m_TriggerSystem;
        private RealLifeFindSchoolSystem.TypeHandle __TypeHandle;
        private EntityQuery __query_17488131_0;
        private EntityQuery __query_17488131_1;

        public override int GetUpdateInterval(SystemUpdatePhase phase) => 16;

        [UnityEngine.Scripting.Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            this.m_EndFrameBarrier = this.World.GetOrCreateSystemManaged<EndFrameBarrier>();
            this.m_PathfindSetupSystem = this.World.GetOrCreateSystemManaged<PathfindSetupSystem>();
            this.m_TriggerSystem = this.World.GetOrCreateSystemManaged<TriggerSystem>();
            this.m_CitySystem = this.World.GetOrCreateSystemManaged<CitySystem>();
            this.m_SimulationSystem = this.World.GetOrCreateSystemManaged<SimulationSystem>();
            this.m_SchoolSeekerQuery = this.GetEntityQuery(ComponentType.ReadWrite<SchoolSeeker>(), ComponentType.ReadOnly<Owner>(), ComponentType.Exclude<PathInformation>(), ComponentType.Exclude<Deleted>());
            this.m_ResultsQuery = this.GetEntityQuery(ComponentType.ReadWrite<SchoolSeeker>(), ComponentType.ReadOnly<Owner>(), ComponentType.ReadOnly<PathInformation>(), ComponentType.Exclude<Deleted>());
            this.RequireAnyForUpdate(this.m_SchoolSeekerQuery, this.m_ResultsQuery);
            this.RequireForUpdate<EconomyParameterData>();
            this.RequireForUpdate<TimeData>();
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnUpdate()
        {
            if (!this.m_SchoolSeekerQuery.IsEmptyIgnoreFilter)
            {
                RealLifeFindSchoolSystem.FindSchoolJob jobData = new RealLifeFindSchoolSystem.FindSchoolJob()
                {
                    m_EntityType = InternalCompilerInterface.GetEntityTypeHandle(ref this.__TypeHandle.__Unity_Entities_Entity_TypeHandle, ref this.CheckedStateRef),
                    m_SchoolSeekerType = InternalCompilerInterface.GetComponentTypeHandle<SchoolSeeker>(ref this.__TypeHandle.__Game_Citizens_SchoolSeeker_RO_ComponentTypeHandle, ref this.CheckedStateRef),
                    m_OwnerType = InternalCompilerInterface.GetComponentTypeHandle<Owner>(ref this.__TypeHandle.__Game_Common_Owner_RO_ComponentTypeHandle, ref this.CheckedStateRef),
                    m_HouseholdMembers = InternalCompilerInterface.GetComponentLookup<HouseholdMember>(ref this.__TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentLookup, ref this.CheckedStateRef),
                    m_PropertyRenters = InternalCompilerInterface.GetComponentLookup<PropertyRenter>(ref this.__TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup, ref this.CheckedStateRef),
                    m_CurrentDistrictData = InternalCompilerInterface.GetComponentLookup<CurrentDistrict>(ref this.__TypeHandle.__Game_Areas_CurrentDistrict_RO_ComponentLookup, ref this.CheckedStateRef),
                    m_Citizens = InternalCompilerInterface.GetComponentLookup<Citizen>(ref this.__TypeHandle.__Game_Citizens_Citizen_RO_ComponentLookup, ref this.CheckedStateRef),
                    m_Households = InternalCompilerInterface.GetComponentLookup<Household>(ref this.__TypeHandle.__Game_Citizens_Household_RO_ComponentLookup, ref this.CheckedStateRef),
                    m_HouseholdCitizens = InternalCompilerInterface.GetBufferLookup<HouseholdCitizen>(ref this.__TypeHandle.__Game_Citizens_HouseholdCitizen_RO_BufferLookup, ref this.CheckedStateRef),
                    m_OwnedVehicles = InternalCompilerInterface.GetBufferLookup<OwnedVehicle>(ref this.__TypeHandle.__Game_Vehicles_OwnedVehicle_RO_BufferLookup, ref this.CheckedStateRef),
                    m_PathfindQueue = this.m_PathfindSetupSystem.GetQueue((object)this, 64).AsParallelWriter(),
                    m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                    college_in_univ_prob = Mod.m_Setting.college_edu_in_univ
                };
                this.Dependency = jobData.ScheduleParallel<RealLifeFindSchoolSystem.FindSchoolJob>(this.m_SchoolSeekerQuery, this.Dependency);
                this.m_PathfindSetupSystem.AddQueueWriter(this.Dependency);
                this.m_EndFrameBarrier.AddJobHandleForProducer(this.Dependency);
            }
            if (this.m_ResultsQuery.IsEmptyIgnoreFilter)
                return;

            JobHandle outJobHandle;

            RealLifeFindSchoolSystem.StartStudyingJob jobData1 = new RealLifeFindSchoolSystem.StartStudyingJob()
            {
                m_Chunks = this.m_ResultsQuery.ToArchetypeChunkListAsync((AllocatorManager.AllocatorHandle)this.World.UpdateAllocator.ToAllocator, out outJobHandle),
                m_EntityType = InternalCompilerInterface.GetEntityTypeHandle(ref this.__TypeHandle.__Unity_Entities_Entity_TypeHandle, ref this.CheckedStateRef),
                m_SchoolSeekerType = InternalCompilerInterface.GetComponentTypeHandle<SchoolSeeker>(ref this.__TypeHandle.__Game_Citizens_SchoolSeeker_RO_ComponentTypeHandle, ref this.CheckedStateRef),
                m_OwnerType = InternalCompilerInterface.GetComponentTypeHandle<Owner>(ref this.__TypeHandle.__Game_Common_Owner_RO_ComponentTypeHandle, ref this.CheckedStateRef),
                m_PathInfoType = InternalCompilerInterface.GetComponentTypeHandle<PathInformation>(ref this.__TypeHandle.__Game_Pathfind_PathInformation_RO_ComponentTypeHandle, ref this.CheckedStateRef),
                m_Citizens = InternalCompilerInterface.GetComponentLookup<Citizen>(ref this.__TypeHandle.__Game_Citizens_Citizen_RW_ComponentLookup, ref this.CheckedStateRef),
                m_Prefabs = InternalCompilerInterface.GetComponentLookup<PrefabRef>(ref this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup, ref this.CheckedStateRef),
                m_SchoolData = InternalCompilerInterface.GetComponentLookup<SchoolData>(ref this.__TypeHandle.__Game_Prefabs_SchoolData_RO_ComponentLookup, ref this.CheckedStateRef),
                m_StudentBuffers = InternalCompilerInterface.GetBufferLookup<Game.Buildings.Student>(ref this.__TypeHandle.__Game_Buildings_Student_RW_BufferLookup, ref this.CheckedStateRef),
                m_InstalledUpgrades = InternalCompilerInterface.GetBufferLookup<InstalledUpgrade>(ref this.__TypeHandle.__Game_Buildings_InstalledUpgrade_RO_BufferLookup, ref this.CheckedStateRef),
                m_Fees = InternalCompilerInterface.GetBufferLookup<ServiceFee>(ref this.__TypeHandle.__Game_City_ServiceFee_RO_BufferLookup, ref this.CheckedStateRef),
                m_CityModifiers = InternalCompilerInterface.GetBufferLookup<CityModifier>(ref this.__TypeHandle.__Game_City_CityModifier_RO_BufferLookup, ref this.CheckedStateRef),
                m_HouseholdDatas = InternalCompilerInterface.GetComponentLookup<Household>(ref this.__TypeHandle.__Game_Citizens_Household_RO_ComponentLookup, ref this.CheckedStateRef),
                m_Efficiencies = InternalCompilerInterface.GetBufferLookup<Efficiency>(ref this.__TypeHandle.__Game_Buildings_Efficiency_RO_BufferLookup, ref this.CheckedStateRef),
                m_Resources = InternalCompilerInterface.GetBufferLookup<Resources>(ref this.__TypeHandle.__Game_Economy_Resources_RO_BufferLookup, ref this.CheckedStateRef),
                m_HouseholdMembers = InternalCompilerInterface.GetComponentLookup<HouseholdMember>(ref this.__TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentLookup, ref this.CheckedStateRef),
                m_Deleteds = InternalCompilerInterface.GetComponentLookup<Deleted>(ref this.__TypeHandle.__Game_Common_Deleted_RO_ComponentLookup, ref this.CheckedStateRef),
                m_Workers = InternalCompilerInterface.GetComponentLookup<Worker>(ref this.__TypeHandle.__Game_Citizens_Worker_RO_ComponentLookup, ref this.CheckedStateRef),
                m_Employees = InternalCompilerInterface.GetBufferLookup<Employee>(ref this.__TypeHandle.__Game_Companies_Employee_RW_BufferLookup, ref this.CheckedStateRef),
                m_City = this.m_CitySystem.City,
                m_EconomyParameters = this.__query_17488131_0.GetSingleton<EconomyParameterData>(),
                m_RandomSeed = RandomSeed.Next(),
                m_SimulationFrame = this.m_SimulationSystem.frameIndex,
                m_DebugFastFindSchool = this.debugFastFindSchool,
                m_TriggerBuffer = this.m_TriggerSystem.CreateActionBuffer(),
                m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer()
            };
            this.Dependency = jobData1.Schedule<RealLifeFindSchoolSystem.StartStudyingJob>(JobHandle.CombineDependencies(outJobHandle, this.Dependency));
            this.m_TriggerSystem.AddActionBufferWriter(this.Dependency);
            this.m_EndFrameBarrier.AddJobHandleForProducer(this.Dependency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void __AssignQueries(ref SystemState state)
        {
            EntityQueryBuilder entityQueryBuilder1 = new EntityQueryBuilder((AllocatorManager.AllocatorHandle)Allocator.Temp);
            EntityQueryBuilder entityQueryBuilder2 = entityQueryBuilder1.WithAll<EconomyParameterData>();
            entityQueryBuilder2 = entityQueryBuilder2.WithOptions(EntityQueryOptions.IncludeSystems);
            // ISSUE: reference to a compiler-generated field
            this.__query_17488131_0 = entityQueryBuilder2.Build(ref state);
            entityQueryBuilder1.Reset();
            EntityQueryBuilder entityQueryBuilder3 = entityQueryBuilder1.WithAll<TimeData>();
            entityQueryBuilder3 = entityQueryBuilder3.WithOptions(EntityQueryOptions.IncludeSystems);
            // ISSUE: reference to a compiler-generated field
            this.__query_17488131_1 = entityQueryBuilder3.Build(ref state);
            entityQueryBuilder1.Reset();
            entityQueryBuilder1.Dispose();
        }

        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            this.__AssignQueries(ref this.CheckedStateRef);
            this.__TypeHandle.__AssignHandles(ref this.CheckedStateRef);
        }

        [UnityEngine.Scripting.Preserve]
        public RealLifeFindSchoolSystem()
        {
        }

        [BurstCompile]
        private struct FindSchoolJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public ComponentTypeHandle<Owner> m_OwnerType;
            [ReadOnly]
            public ComponentTypeHandle<SchoolSeeker> m_SchoolSeekerType;
            [ReadOnly]
            public ComponentLookup<HouseholdMember> m_HouseholdMembers;
            [ReadOnly]
            public ComponentLookup<PropertyRenter> m_PropertyRenters;
            [ReadOnly]
            public ComponentLookup<CurrentDistrict> m_CurrentDistrictData;
            [ReadOnly]
            public ComponentLookup<Citizen> m_Citizens;
            [ReadOnly]
            public ComponentLookup<Household> m_Households;
            [ReadOnly]
            public BufferLookup<HouseholdCitizen> m_HouseholdCitizens;
            [ReadOnly]
            public BufferLookup<OwnedVehicle> m_OwnedVehicles;
            public NativeQueue<SetupQueueItem>.ParallelWriter m_PathfindQueue;
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            public RandomSeed m_RandomSeed;
            public int college_in_univ_prob;

            public void Execute(
              in ArchetypeChunk chunk,
              int unfilteredChunkIndex,
              bool useEnabledMask,
              in v128 chunkEnabledMask)
            {
                NativeArray<Entity> nativeArray1 = chunk.GetNativeArray(this.m_EntityType);
                NativeArray<Owner> nativeArray2 = chunk.GetNativeArray<Owner>(ref this.m_OwnerType);
                NativeArray<SchoolSeeker> nativeArray3 = chunk.GetNativeArray<SchoolSeeker>(ref this.m_SchoolSeekerType);
                Unity.Mathematics.Random random = Unity.Mathematics.Random.CreateFromIndex((uint)unfilteredChunkIndex);
                for (int index = 0; index < nativeArray1.Length; ++index)
                {
                    Entity owner = nativeArray2[index].m_Owner;
                    if (!this.m_Citizens.HasComponent(owner))
                    {
                        this.m_CommandBuffer.AddComponent<Deleted>(unfilteredChunkIndex, nativeArray1[index], new Deleted());
                    }
                    else
                    {
                        Citizen citizen = this.m_Citizens[owner];
                        Entity household1 = this.m_HouseholdMembers[owner].m_Household;
                        if (this.m_PropertyRenters.HasComponent(household1))
                        {
                            Entity entity = nativeArray1[index];
                            Entity property = this.m_PropertyRenters[household1].m_Property;
                            int level = nativeArray3[index].m_Level;
                            Entity district = Entity.Null;
                            if (this.m_CurrentDistrictData.HasComponent(property))
                            {
                                district = this.m_CurrentDistrictData[property].m_District;
                            }
                            this.m_CommandBuffer.AddComponent<PathInformation>(unfilteredChunkIndex, entity, new PathInformation()
                            {
                                m_State = PathFlags.Pending
                            });
                            Household household2 = this.m_Households[household1];
                            DynamicBuffer<HouseholdCitizen> householdCitizen = this.m_HouseholdCitizens[household1];
                            PathfindParameters parameters = new PathfindParameters()
                            {
                                m_MaxSpeed = (float2)111.111115f,
                                m_WalkSpeed = (float2)1.66666675f,
                                m_Weights = CitizenUtils.GetPathfindWeights(citizen, household2, householdCitizen.Length),
                                m_Methods = PathMethod.Pedestrian | PathMethod.PublicTransportDay,
                                m_MaxCost = CitizenBehaviorSystem.kMaxPathfindCost,
                                m_PathfindFlags = PathfindFlags.Simplified | PathfindFlags.IgnorePath
                            };
                            SetupQueueTarget setupQueueTarget = new SetupQueueTarget();
                            setupQueueTarget.m_Type = SetupTargetType.CurrentLocation;
                            setupQueueTarget.m_Methods = PathMethod.Pedestrian;
                            SetupQueueTarget origin = setupQueueTarget;
                            setupQueueTarget = new SetupQueueTarget();
                            setupQueueTarget.m_Type = SetupTargetType.SchoolSeekerTo;
                            setupQueueTarget.m_Methods = PathMethod.Pedestrian;

                            //If allow college in university
                            //Mod.log.Info($"College in Univ:{college_in_univ},level:{level}");
                            if (college_in_univ_prob > 0 && level == 3)
                            {
                                int prob = random.NextInt(100);
                                if (prob < college_in_univ_prob)
                                {
                                    level = 4;
                                }
                                //Mod.log.Info($"Prob:{prob},level:{level}");
                            }
                            setupQueueTarget.m_Value = level;
                            setupQueueTarget.m_Entity = district;
                            SetupQueueTarget destination = setupQueueTarget;
                            if (citizen.GetAge() != CitizenAge.Child)
                            {
                                PathUtils.UpdateOwnedVehicleMethods(household1, ref this.m_OwnedVehicles, ref parameters, ref origin, ref destination);
                            }
                            this.m_PathfindQueue.Enqueue(new SetupQueueItem(entity, parameters, origin, destination));
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
        private struct StartStudyingJob : IJob
        {
            [ReadOnly]
            public NativeList<ArchetypeChunk> m_Chunks;
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public ComponentTypeHandle<Owner> m_OwnerType;
            [ReadOnly]
            public ComponentTypeHandle<SchoolSeeker> m_SchoolSeekerType;
            [ReadOnly]
            public ComponentTypeHandle<PathInformation> m_PathInfoType;
            [NativeDisableParallelForRestriction]
            public ComponentLookup<Citizen> m_Citizens;
            public BufferLookup<Game.Buildings.Student> m_StudentBuffers;
            [ReadOnly]
            public ComponentLookup<Deleted> m_Deleteds;
            [ReadOnly]
            public ComponentLookup<PrefabRef> m_Prefabs;
            [ReadOnly]
            public ComponentLookup<SchoolData> m_SchoolData;
            [ReadOnly]
            public ComponentLookup<HouseholdMember> m_HouseholdMembers;
            [ReadOnly]
            public ComponentLookup<Household> m_HouseholdDatas;
            [ReadOnly]
            public BufferLookup<Efficiency> m_Efficiencies;
            [ReadOnly]
            public ComponentLookup<Worker> m_Workers;
            [ReadOnly]
            public BufferLookup<ServiceFee> m_Fees;
            [ReadOnly]
            public BufferLookup<CityModifier> m_CityModifiers;
            [ReadOnly]
            public BufferLookup<Resources> m_Resources;
            [ReadOnly]
            public BufferLookup<InstalledUpgrade> m_InstalledUpgrades;
            public BufferLookup<Employee> m_Employees;
            public NativeQueue<TriggerAction> m_TriggerBuffer;
            public uint m_SimulationFrame;
            public Entity m_City;
            public EconomyParameterData m_EconomyParameters;
            public EntityCommandBuffer m_CommandBuffer;
            public RandomSeed m_RandomSeed;
            public bool m_DebugFastFindSchool;

            public void Execute()
            {
                this.m_RandomSeed.GetRandom((int)this.m_SimulationFrame);
                for (int index1 = 0; index1 < this.m_Chunks.Length; ++index1)
                {
                    ArchetypeChunk chunk = this.m_Chunks[index1];
                    NativeArray<Owner> nativeArray1 = chunk.GetNativeArray<Owner>(ref this.m_OwnerType);
                    NativeArray<PathInformation> nativeArray2 = chunk.GetNativeArray<PathInformation>(ref this.m_PathInfoType);
                    NativeArray<Entity> nativeArray3 = chunk.GetNativeArray(this.m_EntityType);
                    NativeArray<SchoolSeeker> nativeArray4 = chunk.GetNativeArray<SchoolSeeker>(ref this.m_SchoolSeekerType);
                    DynamicBuffer<CityModifier> cityModifier = this.m_CityModifiers[this.m_City];
                    for (int index2 = 0; index2 < nativeArray3.Length; ++index2)
                    {
                        if ((nativeArray2[index2].m_State & PathFlags.Pending) == (PathFlags)0)
                        {
                            Entity e = nativeArray3[index2];
                            Entity owner = nativeArray1[index2].m_Owner;
                            bool flag = false;
                            if (this.m_Citizens.HasComponent(owner) && !this.m_Deleteds.HasComponent(owner))
                            {
                                Entity destination = nativeArray2[index2].m_Destination;
                                if (this.m_Prefabs.HasComponent(destination) && this.m_StudentBuffers.HasBuffer(destination))
                                {
                                    DynamicBuffer<Game.Buildings.Student> studentBuffer = this.m_StudentBuffers[destination];
                                    Entity prefab = this.m_Prefabs[destination].m_Prefab;
                                    if (this.m_SchoolData.HasComponent(prefab))
                                    {
                                        SchoolData result = this.m_SchoolData[prefab];
                                        //if(result.m_EducationLevel < 5 && result.m_EducationLevel != nativeArray4[index2].m_Level)
                                        //{
                                        //    Mod.log.Info($"School level :{result.m_EducationLevel}, Student level:{nativeArray4[index2].m_Level}");
                                        //}
                                        
                                        if (this.m_InstalledUpgrades.HasBuffer(destination))
                                        {
                                            UpgradeUtils.CombineStats<SchoolData>(ref result, this.m_InstalledUpgrades[destination], ref this.m_Prefabs, ref this.m_SchoolData);
                                        }
                                        int studentCapacity = result.m_StudentCapacity;
                                        if (studentBuffer.Length < studentCapacity)
                                        {
                                            studentBuffer.Add(new Game.Buildings.Student()
                                            {
                                                m_Student = owner
                                            });
                                            this.m_CommandBuffer.AddComponent<Game.Citizens.Student>(owner, new Game.Citizens.Student()
                                            {
                                                m_School = destination,
                                                m_LastCommuteTime = nativeArray2[index2].m_Duration,
                                                m_Level = (byte)nativeArray4[index2].m_Level
                                            });
                                            if (this.m_Workers.HasComponent(owner))
                                            {
                                                Entity workplace = this.m_Workers[owner].m_Workplace;
                                                if (this.m_Employees.HasBuffer(workplace))
                                                {
                                                    DynamicBuffer<Employee> employee = this.m_Employees[workplace];
                                                    for (int index3 = 0; index3 < employee.Length; ++index3)
                                                    {
                                                        if (employee[index3].m_Worker == owner)
                                                        {
                                                            employee.RemoveAtSwapBack(index3);
                                                            break;
                                                        }
                                                    }
                                                }
                                                this.m_CommandBuffer.RemoveComponent<Worker>(owner);
                                            }
                                            this.m_TriggerBuffer.Enqueue(new TriggerAction(TriggerType.CitizenStartedSchool, Entity.Null, owner, destination));
                                            Citizen citizen = this.m_Citizens[owner];
                                            citizen.SetFailedEducationCount(0);
                                            this.m_Citizens[owner] = citizen;
                                            flag = true;
                                            this.m_CommandBuffer.RemoveComponent<SchoolSeekerCooldown>(owner);
                                        }
                                    }
                                }
                                if (!flag)
                                {
                                    this.m_CommandBuffer.AddComponent<SchoolSeekerCooldown>(owner, new SchoolSeekerCooldown()
                                    {
                                        m_SimulationFrame = this.m_SimulationFrame
                                    });
                                }
                            }
                            this.m_CommandBuffer.RemoveComponent<HasSchoolSeeker>(owner);
                            this.m_CommandBuffer.AddComponent<Deleted>(e, new Deleted());
                        }
                    }
                }
            }
        }

        private struct TypeHandle
        {
            [ReadOnly]
            public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<SchoolSeeker> __Game_Citizens_SchoolSeeker_RO_ComponentTypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<Owner> __Game_Common_Owner_RO_ComponentTypeHandle;
            [ReadOnly]
            public ComponentLookup<HouseholdMember> __Game_Citizens_HouseholdMember_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<CurrentDistrict> __Game_Areas_CurrentDistrict_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<Citizen> __Game_Citizens_Citizen_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<Household> __Game_Citizens_Household_RO_ComponentLookup;
            [ReadOnly]
            public BufferLookup<HouseholdCitizen> __Game_Citizens_HouseholdCitizen_RO_BufferLookup;
            [ReadOnly]
            public BufferLookup<OwnedVehicle> __Game_Vehicles_OwnedVehicle_RO_BufferLookup;
            [ReadOnly]
            public ComponentTypeHandle<PathInformation> __Game_Pathfind_PathInformation_RO_ComponentTypeHandle;
            public ComponentLookup<Citizen> __Game_Citizens_Citizen_RW_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<SchoolData> __Game_Prefabs_SchoolData_RO_ComponentLookup;
            public BufferLookup<Game.Buildings.Student> __Game_Buildings_Student_RW_BufferLookup;
            [ReadOnly]
            public BufferLookup<InstalledUpgrade> __Game_Buildings_InstalledUpgrade_RO_BufferLookup;
            [ReadOnly]
            public BufferLookup<ServiceFee> __Game_City_ServiceFee_RO_BufferLookup;
            [ReadOnly]
            public BufferLookup<CityModifier> __Game_City_CityModifier_RO_BufferLookup;
            [ReadOnly]
            public BufferLookup<Efficiency> __Game_Buildings_Efficiency_RO_BufferLookup;
            [ReadOnly]
            public BufferLookup<Resources> __Game_Economy_Resources_RO_BufferLookup;
            [ReadOnly]
            public ComponentLookup<Deleted> __Game_Common_Deleted_RO_ComponentLookup;
            [ReadOnly]
            public ComponentLookup<Worker> __Game_Citizens_Worker_RO_ComponentLookup;
            public BufferLookup<Employee> __Game_Companies_Employee_RW_BufferLookup;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                this.__Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                this.__Game_Citizens_SchoolSeeker_RO_ComponentTypeHandle = state.GetComponentTypeHandle<SchoolSeeker>(true);
                this.__Game_Common_Owner_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Owner>(true);
                this.__Game_Citizens_HouseholdMember_RO_ComponentLookup = state.GetComponentLookup<HouseholdMember>(true);
                this.__Game_Buildings_PropertyRenter_RO_ComponentLookup = state.GetComponentLookup<PropertyRenter>(true);
                this.__Game_Areas_CurrentDistrict_RO_ComponentLookup = state.GetComponentLookup<CurrentDistrict>(true);
                this.__Game_Citizens_Citizen_RO_ComponentLookup = state.GetComponentLookup<Citizen>(true);
                this.__Game_Citizens_Household_RO_ComponentLookup = state.GetComponentLookup<Household>(true);
                this.__Game_Citizens_HouseholdCitizen_RO_BufferLookup = state.GetBufferLookup<HouseholdCitizen>(true);
                this.__Game_Vehicles_OwnedVehicle_RO_BufferLookup = state.GetBufferLookup<OwnedVehicle>(true);
                this.__Game_Pathfind_PathInformation_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PathInformation>(true);
                this.__Game_Citizens_Citizen_RW_ComponentLookup = state.GetComponentLookup<Citizen>();
                this.__Game_Prefabs_PrefabRef_RO_ComponentLookup = state.GetComponentLookup<PrefabRef>(true);
                this.__Game_Prefabs_SchoolData_RO_ComponentLookup = state.GetComponentLookup<SchoolData>(true);
                this.__Game_Buildings_Student_RW_BufferLookup = state.GetBufferLookup<Game.Buildings.Student>();
                this.__Game_Buildings_InstalledUpgrade_RO_BufferLookup = state.GetBufferLookup<InstalledUpgrade>(true);
                this.__Game_City_ServiceFee_RO_BufferLookup = state.GetBufferLookup<ServiceFee>(true);
                this.__Game_City_CityModifier_RO_BufferLookup = state.GetBufferLookup<CityModifier>(true);
                this.__Game_Buildings_Efficiency_RO_BufferLookup = state.GetBufferLookup<Efficiency>(true);
                this.__Game_Economy_Resources_RO_BufferLookup = state.GetBufferLookup<Resources>(true);
                this.__Game_Common_Deleted_RO_ComponentLookup = state.GetComponentLookup<Deleted>(true);
                this.__Game_Citizens_Worker_RO_ComponentLookup = state.GetComponentLookup<Worker>(true);
                this.__Game_Companies_Employee_RW_BufferLookup = state.GetBufferLookup<Employee>();
            }
        }
    }
}
