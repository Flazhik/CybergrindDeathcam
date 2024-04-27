using System.IO;
using System.Reflection;
using BepInEx;
using CybergrindDeathcam.Components;
using CybergrindDeathcam.Patches;
using HarmonyLib;
using PluginConfig.API;
using PluginConfig.API.Fields;
using UnityEngine.SceneManagement;

namespace CybergrindDeathcam

{
    [BepInProcess("ULTRAKILL.exe")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class CybergrindDeathcam : BaseUnityPlugin
    {
        public static BoolField SkipLeaderboards;
        public static IntField LeaderboardsSkipThreshold;
        public static IntField AlwaysShowLeaderboardsStartingFrom;
        
        private static PluginConfigurator _config;
        
        private Harmony _harmony;
        private KillingFactors _killingFactors;

        private static void SetupConfig()
        {
            _config = PluginConfigurator.Create(PluginInfo.NAME, PluginInfo.GUID);
            SkipLeaderboards = new BoolField(_config.rootPanel, "Skip Cyber Grind leaderboards",
                "deathcam.skip-leaderboards", false);            
            LeaderboardsSkipThreshold = new IntField(_config.rootPanel, "Don't skip if PB is <= N waves away",
                "deathcam.threshold", 5);
            AlwaysShowLeaderboardsStartingFrom = new IntField(_config.rootPanel, "Don't skip starting from wave",
                "deathcam.always-show-starting-from", 999);

            SkipLeaderboards.onValueChange += ManageSkipFields;
            SkipLeaderboards.TriggerValueChangeEvent();
            _config.SetIconWithURL($"file://{Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "icon.png")}");
        }

        private static void ManageSkipFields(BoolField.BoolValueChangeEvent e)
        {
            var value = e.value;
            LeaderboardsSkipThreshold.hidden = !value;
            AlwaysShowLeaderboardsStartingFrom.hidden = !value;
        }
        
        private void Awake()
        {
            AssetsManager.Instance.LoadAssets();
            AssetsManager.Instance.RegisterPrefabs();
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            _killingFactors = KillingFactors.Instance;
            _harmony = new Harmony(PluginInfo.GUID);
            SetupConfig();
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            NewMovementPatch.DeadAlready = false;
            if (scene != SceneManager.GetActiveScene())
                return;

            switch (SceneHelper.CurrentScene)
            {
                case "Endless":
                {
                    _killingFactors = KillingFactors.Instance;
                    _harmony.PatchAll();
                    break;
                }

                default:
                {
                    _harmony.UnpatchSelf();
                    break;
                }
            }
        }
    }
}