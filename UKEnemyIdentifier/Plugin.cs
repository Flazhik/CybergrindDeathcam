using BepInEx;
using HarmonyLib;

namespace UKEnemyIdentifier

{
    [BepInProcess("ULTRAKILL.exe")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony _harmony;
        private void Awake()
        {
            _harmony = new Harmony(PluginInfo.GUID);
            _harmony.PatchAll();
        }
    }
}