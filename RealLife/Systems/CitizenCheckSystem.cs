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
using Game.Tools;
using Game.Buildings;

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
            _query = GetEntityQuery(ComponentType.ReadWrite<Citizen>(), ComponentType.Exclude<Temp>(), ComponentType.Exclude<Deleted>());

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
            Mod.log.Info($"Processing {cits.Length} citizens on day:{day}");
            int children = 0;
            int children_school_age = 0;
            int teenagers = 0;
            int adults = 0;
            int elders = 0;
            foreach (var ci in cits)
            {
                Citizen data;

                if (EntityManager.TryGetComponent<Citizen>(ci, out data))
                {
                    int age = day - data.m_BirthDay;
                    Mod.log.Info($"age:{age}, agegroup:{data.GetAge()}, edu:{data.GetEducationLevel()}");
                    
                    //Temporary solution to negative ages. Assigning new ages based on age group
                    //if (age < 0)
                    //{
                    //    
                    //    CitizenAge ageGroup = data.GetAge();
                    //    switch ((int)ageGroup)
                    //    {
                    //        case 0: //child
                    //            age = random.NextInt(0, Mod.m_Setting.child_age_limit);
                    //            break;
                    //        case 1: //teen
                    //            age = random.NextInt(Mod.m_Setting.child_age_limit + 1, Mod.m_Setting.teen_age_limit);
                    //            break;
                    //        case 2: //adult
                    //            age = random.NextInt(Mod.m_Setting.teen_age_limit + 1, Mod.m_Setting.adult_age_limit);
                    //            break;
                    //        case 3: //elder
                    //            age = random.NextInt(Mod.m_Setting.adult_age_limit + 1, Mod.m_Setting.male_life_expectancy);
                    //            break;
                    //        default:
                    //            age = Mod.m_Setting.male_life_expectancy;
                    //            break;
                    //    }
                    //    //Mod.log.Info($"age:{age}, day:{day}, bd:{(int)data.m_BirthDay}, newbd:{(short)(day - age)}, ageGroup:{ageGroup}");
                    //
                    //    data.m_BirthDay = (short)(day - age);
                    //
                    //    counter++;
                    //}
                    //else
                    //{
                    //    if (age <= Mod.m_Setting.child_age_limit)
                    //    {   
                    //        //if (age >=  Mod.m_Setting.child_school_start_age)
                    //        //{
                    //        //    children_school_age++;
                    //        //}
                    //        if (!data.GetAge().Equals(CitizenAge.Child) && !data.GetAge().Equals(CitizenAge.Teen))
                    //        {
                    //            //Mod.log.Info($"age:{age}, day:{day}, bd:{(int)data.m_BirthDay}, newbd:{(short)(day - age)}, agegroup:{data.GetAge()}");
                    //            //Mod.log.Info($"age:{age}, AGE:{data.GetAge()}");
                    //            data.SetAge(CitizenAge.Child);
                    //            children++;
                    //        }
                    //    
                    //    } else if (age <= Mod.m_Setting.teen_age_limit)
                    //    { 
                    //        if (!data.GetAge().Equals(CitizenAge.Teen) && !data.GetAge().Equals(CitizenAge.Adult))
                    //        {
                    //            data.SetAge(CitizenAge.Teen);
                    //            teenagers++;
                    //        }
                    //    } else if (age <= Mod.m_Setting.adult_age_limit)
                    //    {
                    //        if (!data.GetAge().Equals(CitizenAge.Adult) && !data.GetAge().Equals(CitizenAge.Elderly))
                    //        {
                    //            data.SetAge(CitizenAge.Adult);
                    //            adults++;
                    //        }
                    //    } else
                    //    {
                    //        if (!data.GetAge().Equals(CitizenAge.Elderly))
                    //        {
                    //            data.SetAge(CitizenAge.Elderly);
                    //            elders++;
                    //        }
                    //    }
                    //        
                    //}
                    //
                    //EntityManager.SetComponentData<Citizen>(ci, data);

                }
            }
            //Mod.log.Info($"Fixed {counter} citizen ages");
            //Mod.log.Info($"Fixed Age Groups: Children: {children}, Teenagers: {teenagers}, Adults: {adults}, Elders: {elders}");
            //Enabled = false;
        }

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            // One day (or month) in-game is '262144' ticks
            return 262144 / 64;
        }
    }
}