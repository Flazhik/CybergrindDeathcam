using System.IO;
using System.Reflection;
using BepInEx;
using CybergrindDeathcam.Components;
using HarmonyLib;
using PluginConfig.API;
using PluginConfig.API.Fields;
using UKEnemyIdentifier.Components;
using UnityEngine.SceneManagement;

namespace CybergrindDeathcam

{
    [BepInProcess("ULTRAKILL.exe")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class CybergrindDeathcam : BaseUnityPlugin
    {
        public static BoolField UseCampaignDeathSequence;
        public static BoolField SkipLeaderboards;
        public static IntField LeaderboardsSkipThreshold;
        public static IntField AlwaysShowLeaderboardsStartingFrom;
        
        private static PluginConfigurator _config;
        private static Harmony _harmony;
        
        private static void SetupConfig()
        {
            _config = PluginConfigurator.Create(PluginInfo.NAME, PluginInfo.GUID);
            /*FreezeRate = new FloatField(_config.rootPanel, "Freeze rate",
                "deathcam.tmp.freezerate", 0.4f); 
            ZoomInRate = new FloatField(_config.rootPanel, "Zoom in rate",
                "deathcam.tmp.zomminrate", 0.4f);   */
            UseCampaignDeathSequence = new BoolField(_config.rootPanel, "Enable campaign death screen",
                "deathcam.campaign-death-sequence", false);   
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
            
            _harmony = new Harmony(PluginInfo.GUID);
            SetupConfig();
            _harmony.PatchAll();
            SceneManager.sceneLoaded += (_, mode) =>
            {
                MonoSingleton<EnemyIdentifierManager>.Instance.OnPlayerHurt -= OnHit;
                if (SceneHelper.CurrentScene == "Endless")
                    MonoSingleton<EnemyIdentifierManager>.Instance.OnPlayerHurt += OnHit;
            };
        }

        private static void OnHit(EnemyIdentifierManager.PlayerHurtEvent e)
        {
            if (!e.PlayerIsKilled)
                return;

            var deathCam = NewMovement.Instance.gameObject.AddComponent<DeathCam>();
            deathCam.killer = e.EnemyId;
            deathCam.enabled = true;
        }
    }
}