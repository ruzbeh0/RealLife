using Game;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace RealLife.Systems
{
    public partial class EducationParameterUpdaterSystem : GameSystemBase
    {
        private Dictionary<Entity, EducationParameterData> _demandParameterData = new Dictionary<Entity, EducationParameterData>();

        private EntityQuery _query;

        protected override void OnCreate()
        {
            base.OnCreate();

            _query = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[] {
                    ComponentType.ReadWrite<EducationParameterData>()
                }
            });

            RequireForUpdate(_query);
        }

        protected override void OnUpdate()
        {
            var prefabs = _query.ToEntityArray(Allocator.Temp);

            foreach (var tsd in prefabs)
            {
                EducationParameterData data;

                if (!_demandParameterData.TryGetValue(tsd, out data))
                {
                    data = EntityManager.GetComponentData<EducationParameterData>(tsd);
                    _demandParameterData.Add(tsd, data);
                }

                data.m_EnterHighSchoolProbability = Mod.m_Setting.enter_high_school_prob/100f;
                data.m_AdultEnterHighSchoolProbability = Mod.m_Setting.adult_enter_high_school_prob/100f;
                data.m_WorkerContinueEducationProbability = Mod.m_Setting.worker_continue_education/100f;

                EntityManager.SetComponentData<EducationParameterData>(tsd, data);
            }
        }

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            // One day (or month) in-game is '262144' ticks
            return 262144 / 8;
        }
    }
}