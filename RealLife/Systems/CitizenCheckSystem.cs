using Colossal.Entities;
using Game;
using Game.Common;
using Game.Citizens;
using Game.Prefabs;
using Game.Simulation;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Core;

namespace RealLife.Systems
{
    public partial class CitizenCheckSystem : GameSystemBase
    {
        private EntityQuery _query;
        private SimulationSystem m_SimulationSystem;
        private uint m_SimulationFrame;
        private EntityQuery m_TimeDataQuery;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_SimulationSystem = this.World.GetOrCreateSystemManaged<SimulationSystem>();
            m_TimeDataQuery = this.GetEntityQuery(ComponentType.ReadOnly<Game.Common.TimeData>());
            _query = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[] {
                    ComponentType.ReadWrite<Citizen>()
                }
            });

            RequireForUpdate(_query);
            RequireForUpdate(m_TimeDataQuery);
        }

        protected override void OnUpdate()
        {
            var cits = _query.ToEntityArray(Allocator.Temp);
            Game.Common.TimeData m_TimeData = this.m_TimeDataQuery.GetSingleton<Game.Common.TimeData>();
            m_SimulationFrame = this.m_SimulationSystem.frameIndex;
            int day = TimeSystem.GetDay(this.m_SimulationFrame, m_TimeData);
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(this.m_SimulationFrame);
            int counter = 0;
            Mod.log.Info("start");
            foreach (var ci in cits)
            {
                Citizen data;

                if (EntityManager.TryGetComponent<Citizen>(ci, out data))
                {
                    int age = day - (int)data.m_BirthDay;
                    //Mod.log.Info($"age:{age}, agegroup:{data.GetAge()}, edu:{data.GetEducationLevel()}");
                    //Mod.log.Info($"age:{age}, FF:{this.m_SimulationFrame}, bd:{(int)data.m_BirthDay}");
                    //Temporary solution to negative ages. Assigning new ages based on age group
                    if (age < 0)
                    {
                        CitizenAge ageGroup = data.GetAge();
                        switch ((int)ageGroup)
                        {
                            case 0: //child
                                age = random.NextInt(0, Mod.m_Setting.child_age_limit);
                                break;
                            case 1: //teen
                                age = random.NextInt(Mod.m_Setting.child_age_limit + 1, Mod.m_Setting.teen_age_limit);
                                break;
                            case 2: //adult
                                age = random.NextInt(Mod.m_Setting.teen_age_limit + 1, Mod.m_Setting.adult_age_limit);
                                break;
                            case 3: //elder
                                age = random.NextInt(Mod.m_Setting.adult_age_limit + 1, Mod.m_Setting.male_life_expectancy);
                                break;
                            default:
                                age = Mod.m_Setting.male_life_expectancy;
                                break;
                        }

                        data.m_BirthDay = (short)(day - age);

                        EntityManager.SetComponentData<Citizen>(ci, data);
                        counter++;
                    }

                }
            }
            Mod.log.Info($"Fixed {counter} citizen ages");
            Enabled = false;
        }

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            // One day (or month) in-game is '262144' ticks
            return 262144 / 8;
        }
    }
}