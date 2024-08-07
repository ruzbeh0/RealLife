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

namespace RealLife
{
    public class Mod : IMod
    {
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
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<RealLifeEducationInfoviewUISystem>().Enabled = false;

            updateSystem.UpdateAt<RealLifeAgingSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<RealLifeApplyToSchoolSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<RealLifeGraduationSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<RealLifeFindSchoolSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<RealLifeDeathCheckSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<RealLifeDeathCheckSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateBefore<PreDeserialize<RealLifeEducationInfoviewUISystem>>(SystemUpdatePhase.Deserialize);
            updateSystem.UpdateAt<RealLifeEducationInfoviewUISystem>(SystemUpdatePhase.UIUpdate);
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
