using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Companies;
using Game.Prefabs;
using Game.Prefabs.Climate;
using Game.Simulation;
using Game.UI.InGame;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static Game.Rendering.Debug.RenderPrefabRenderer;
using static Game.Simulation.ClimateSystem;

namespace RealLife.Patches
{

    [HarmonyPatch]
    public class RealLifePatches
    {
        [HarmonyPatch(typeof(EducationInfoviewUISystem), "PerformUpdate")]
        [HarmonyPrefix]
        static bool RealLifePatches_PerformUpdate(EducationInfoviewUISystem __instance)
        {
            return false;
        }

    }
}
