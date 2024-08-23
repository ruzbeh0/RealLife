using Game;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace RealLife.Systems
{
    public partial class CitizenParametersUpdaterSystem : GameSystemBase
    {
        private Dictionary<Entity, CitizenParametersData> _citizenParametersData = new Dictionary<Entity, CitizenParametersData>();

        private EntityQuery _query;

        protected override void OnCreate()
        {
            base.OnCreate();

            _query = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[] {
                    ComponentType.ReadWrite<CitizenParametersData>()
                }
            });

            RequireForUpdate(_query);
        }

        protected override void OnUpdate()
        {
            var prefabs = _query.ToEntityArray(Allocator.Temp);

            foreach (var tsd in prefabs)
            {
                CitizenParametersData data;

                if (!_citizenParametersData.TryGetValue(tsd, out data))
                {
                    data = EntityManager.GetComponentData<CitizenParametersData>(tsd);
                    _citizenParametersData.Add(tsd, data);
                }

                data.m_StudentBirthRateAdjust *= (100 + Mod.m_Setting.student_birth_rate_adjuster) / 100f;
                data.m_AdultFemaleBirthRateBonus *= (100 + Mod.m_Setting.adult_female_birth_rate_bonus_adjuster) / 100f;
                data.m_BaseBirthRate *= (100 + Mod.m_Setting.base_birth_rate_adjuster) / 100f;
                data.m_DivorceRate *= (100 + Mod.m_Setting.divorce_rate_adjuster) / 100f;
                data.m_LookForPartnerRate *= (100 + Mod.m_Setting.look_for_partner_rate_adjuster) / 100f;

                EntityManager.SetComponentData<CitizenParametersData>(tsd, data);
            }
        }

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            // One day (or month) in-game is '262144' ticks
            return 262144 / 8;
        }
    }
}