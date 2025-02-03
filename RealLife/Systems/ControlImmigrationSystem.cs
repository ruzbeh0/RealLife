// Decompiled with JetBrains decompiler
// Type: Reserve_housing_for_local_population.ReserveHousingforlocalPop
// Assembly: Reserve housing for local population, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A69A704A-2E28-4F8F-A524-B8CB14D95EEA
// Assembly location: C:\Users\12249\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods\Reserve\Reserve housing for local population.dll

using Game;
using Game.Agents;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Game.Zones;
using Unity.Entities;
using UnityEngine.Scripting;

#nullable disable
namespace RealLife.Systems
{
    public partial class ControlImmigrationSystem : GameSystemBase
    {
        public EntityQuery HousehodlsQuery;
        public EntityQuery HomesOnMarketQuery;
        private int HouseholdsCount;
        private int HomesOnMarket;
        private int TotalHomes;
        public CountHouseholdDataSystem.HouseholdData m_HouseholdCountData;
        public CountResidentialPropertySystem.ResidentialPropertyData m_ResidentialPropertyData;

        protected override void OnCreate()
        {
            HousehodlsQuery = GetEntityQuery(
                ComponentType.ReadOnly<Household>(),
                ComponentType.Exclude<TouristHousehold>(),
                ComponentType.Exclude<CommuterHousehold>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<Temp>()
            );

            HomesOnMarketQuery = GetEntityQuery(
                ComponentType.ReadOnly<PropertyOnMarket>(),
                ComponentType.ReadOnly<ResidentialProperty>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<Temp>()
            );
        }

        protected override void OnUpdate()
        {
            this.HouseholdsCount = this.HousehodlsQuery.CalculateEntityCount();
            this.HomesOnMarket = this.HomesOnMarketQuery.CalculateEntityCount();
            this.TotalHomes = this.HouseholdsCount + this.HomesOnMarket;

            bool enable = true;

            if (this.TotalHomes == 0 || this.HouseholdsCount <= 10)
                enable = true;
            else if (this.HomesOnMarket * 1000 / this.TotalHomes <= 5)
                enable = false;

            Mod.log.Info($"HouseholdSpawnSystem Enabled: {enable}, Homes On Market: {HomesOnMarket}, Total Homes: {TotalHomes}");
            this.World.GetOrCreateSystemManaged<HouseholdSpawnSystem>().Enabled = enable;
        }

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            // One day (or month) in-game is '262144' ticks
            return 262144 / 32;
        }

        [Preserve]
        public ControlImmigrationSystem()
        {
        }
    }
}
