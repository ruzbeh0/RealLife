using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.PSI.Environment;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.Simulation;
using System.IO;
using Unity.Entities;
using RealLife.Systems;
using Game.Serialization;
using Game.UI.InGame;
using Game.UI;
using HarmonyLib;
using System.Linq;

namespace RealLife
{
    public class Mod : IMod
    {
        public static readonly string harmonyID = "RealLife";
        public static ILog log = LogManager.GetLogger($"{nameof(RealLife)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        public static Setting m_Setting;

        // Mods Settings Folder
        public static string SettingsFolder = Path.Combine(EnvPath.kUserDataPath, "ModsSettings", nameof(RealLife));

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            if (!Directory.Exists(SettingsFolder))
            {
                Directory.CreateDirectory(SettingsFolder);
            }

            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));


            AssetDatabase.global.LoadSettings(nameof(RealLife), m_Setting, new Setting(this));

            // Disable original systems
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Simulation.AgingSystem>().Enabled = false;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Simulation.ApplyToSchoolSystem>().Enabled = false;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Simulation.GraduationSystem>().Enabled = false;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Simulation.FindSchoolSystem>().Enabled = false;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Simulation.DeathCheckSystem>().Enabled = false;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Simulation.SchoolAISystem>().Enabled = false;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Simulation.BirthSystem>().Enabled = false;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Citizens.CitizenInitializeSystem>().Enabled = false;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.UI.InGame.EducationInfoviewUISystem>().Enabled = false;

            //updateSystem.UpdateAfter<CitizenCheckSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<RealLifeAgingSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<RealLifeApplyToSchoolSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<RealLifeGraduationSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<RealLifeFindSchoolSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<RealLifeDeathCheckSystem>(SystemUpdatePhase.GameSimulation); 
            updateSystem.UpdateAt<RealLifeSchoolAISystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<RealLife.Systems.RealLifeBirthSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAfter<CheckBuildingsSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<RealLifeCitizenInitializeSystem>(SystemUpdatePhase.Modification5);
            updateSystem.UpdateAfter<EducationParameterUpdaterSystem>(SystemUpdatePhase.PrefabUpdate);
            updateSystem.UpdateBefore<EducationParameterUpdaterSystem>(SystemUpdatePhase.PrefabReferences);
            updateSystem.UpdateAfter<CitizenParametersUpdaterSystem>(SystemUpdatePhase.PrefabUpdate);
            updateSystem.UpdateBefore<CitizenParametersUpdaterSystem>(SystemUpdatePhase.PrefabReferences);
            updateSystem.UpdateBefore<PreDeserialize<RealLifeEducationInfoviewUISystem>>(SystemUpdatePhase.Deserialize);
            updateSystem.UpdateAt<RealLifeEducationInfoviewUISystem>(SystemUpdatePhase.UIUpdate);

            //Harmony
            var harmony = new Harmony(harmonyID);
            //Harmony.DEBUG = true;
            harmony.PatchAll(typeof(Mod).Assembly);
            var patchedMethods = harmony.GetPatchedMethods().ToArray();
            log.Info($"Plugin {harmonyID} made patches! Patched methods: " + patchedMethods);
            foreach (var patchedMethod in patchedMethods)
            {
                log.Info($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
            }
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            if (m_Setting != null)
            {
                m_Setting.UnregisterInOptionsUI();
                m_Setting = null;
            }
        }
    }
}
