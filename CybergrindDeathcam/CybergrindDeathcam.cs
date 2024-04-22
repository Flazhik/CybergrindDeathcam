using System.IO;
using BepInEx;
using BepInEx.Configuration;
using CybergrindDeathcam.Components;
using CybergrindDeathcam.Patches;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace CybergrindDeathcam

{
    [BepInProcess("ULTRAKILL.exe")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class CybergrindDeathcam : BaseUnityPlugin
    {
        private new static readonly ConfigFile Config = new ConfigFile(Path.Combine(Paths.ConfigPath, "CybergrindDeathcam", "config.cfg"), true);
        
        public static readonly ConfigEntry<bool> SkipLeaderboards = Config.Bind("CybergrindDeathcam", 
            "SkipLeaderboards",
            false,
            "On death skips leaderboards sequence completely and restarts the Cyber Grind at once");
        
        public static readonly ConfigEntry<float> LeaderboardsSkipThreshold = Config.Bind("CybergrindDeathcam", 
            "LeaderboardsSkipThreshold",
            5f,
            "If SkipLeaderboards is enabled, defines the largest possible difference between your personal best and the current wave for the leaderboards to show up");
        
        public static readonly ConfigEntry<float> AlwaysShowLeaderboardsStartingFrom = Config.Bind("CybergrindDeathcam", 
            "AlwaysShowLeaderboardsStartingFrom",
            999f,
            "Starting from this wave, the leaderboards sequence will always be played no matter what");
        
        private Harmony _harmony;
        private KillingFactors _killingFactors;
        
        private void Awake()
        {
            AssetsManager.Instance.LoadAssets();
            AssetsManager.Instance.RegisterPrefabs();
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            _killingFactors = KillingFactors.Instance;
            _harmony = new Harmony(PluginInfo.GUID);
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