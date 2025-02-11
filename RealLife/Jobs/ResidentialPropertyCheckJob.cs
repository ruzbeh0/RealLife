
using Game.Buildings;
using Game.Common;
using Game.Companies;
using Game.Net;
using Game.Prefabs;
using Game.Simulation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using static Game.Rendering.Utilities.State;
using Game.Agents;
using Game.Rendering;
using Game.Citizens;
using RealLife.Components;

namespace RealLife.Jobs
{
    [BurstCompile]
    public partial struct ResidentialPropertyCheckJob : IJobChunk
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public EconomyParameterData economyParameterData;

        public EntityTypeHandle entityTypeHandle;        
        public BufferTypeHandle<Renter> renterTypeHandle;
        public ComponentTypeHandle<PrefabRef> prefabRefTypeHandle;
        public ComponentTypeHandle<Building> buildingTypeHandle;

        public ComponentLookup<BuildingData> buildingDataLookup;
        public ComponentLookup<BuildingPropertyData> propertyDataLookup;
        public ComponentLookup<CommercialProperty> commercialPropertyLookup;
        public ComponentLookup<PropertyToBeOnMarket> propertyToBeOnMarketLookup;
        public ComponentLookup<PropertyOnMarket> propertyOnMarketLookup;
        public ComponentLookup<ConsumptionData> consumptionDataLookup;
        public ComponentLookup<LandValue> landValueLookup;
        public ComponentLookup<SpawnableBuildingData> spawnableBuildingDataLookup;
        public ComponentLookup<ZoneData> zoneDataLookup;
        public ComponentLookup<WorkProvider> workProviderLookup;
        public EntityArchetype m_RentEventArchetype;
        public Unity.Mathematics.Random random;
        public BufferLookup<HouseholdCitizen> m_CitizenBufs;
        public ComponentLookup<HealthProblem> m_HealthProblems;

        public ResidentialPropertyCheckJob()
        {
        }

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var entities = chunk.GetNativeArray(entityTypeHandle);
            var renterAccessor = chunk.GetBufferAccessor(ref renterTypeHandle);
            var prefabRefs = chunk.GetNativeArray(ref prefabRefTypeHandle);
            var buildings = chunk.GetNativeArray(ref buildingTypeHandle);

            //Mod.log.Info($"Processing {entities.Length} buildings");
            bool processedOnce = false;
            //Remove dead citizens
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var prefabRef = prefabRefs[i];
                var renters = renterAccessor[i];
                var building = buildings[i];
                if (!propertyDataLookup.TryGetComponent(prefabRef.m_Prefab, out var propertyData))
                {
                    return;
                }
                for (int j = 0; j < renters.Length; j++)
                {
                    Entity renter = renters[j].m_Renter;
                    if (this.m_CitizenBufs.HasBuffer(renter))
                    {
                        DynamicBuffer<HouseholdCitizen> citizenBuf = m_CitizenBufs[renter];
                        bool flag = false;
                        for (int index2 = 0; index2 < citizenBuf.Length; ++index2)
                        {
                            if (CitizenUtils.IsDead(citizenBuf[index2].m_Citizen, ref this.m_HealthProblems) && citizenBuf.Length == 1)
                            {
                                flag = true;
                                break;
                            }
                        }
            
                        if (flag)
                        {
                            RemoveHousehold(entity, renters[j], ResetType.Delete, unfilteredChunkIndex);
                            processedOnce = true;
                        }
                    }  
                }
            }

            //Remove empty households
            if (!processedOnce)
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var prefabRef = prefabRefs[i];
                    var renters = renterAccessor[i];
                    var building = buildings[i];
                    if (!propertyDataLookup.TryGetComponent(prefabRef.m_Prefab, out var propertyData))
                    {
                        return;
                    }
                    for (int j = 0; j < renters.Length; j++)
                    {
                        Entity renter = renters[j].m_Renter;
                        if (this.m_CitizenBufs.HasBuffer(renter))
                        {
                            DynamicBuffer<HouseholdCitizen> citizenBuf = m_CitizenBufs[renter];
                            if (citizenBuf.Length == 0 || citizenBuf.Length > 5)
                            {
                                RemoveHousehold(entity, renters[j], ResetType.Delete, unfilteredChunkIndex);
                            }
                        }
                    }
                }

            }
        }

        private void RemoveHousehold(Entity property, Renter renter, ResetType reset, int unfilteredChunkIndex)
        {
            var entity = renter;
            switch (reset)
            {
                case ResetType.Delete:
                    ecb.AddComponent<Deleted>(unfilteredChunkIndex, entity);
                    break;
                default:
                    throw new System.Exception($"Invalid ResetType provided: \"{reset}\"!");
            }
        }
    }
}
