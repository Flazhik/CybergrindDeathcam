using CybergrindDeathcam.Components;
using HarmonyLib;

namespace CybergrindDeathcam.Patches
{
    [HarmonyPatch(typeof(NewMovement))]
    public class NewMovementPatch
    {
        public static bool DeadAlready;
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NewMovement), "GetHurt")]
        public static void NewMovement_GetHurt_Postfix(NewMovement __instance)
        {
            if (__instance.hp != 0 || DeadAlready)
                return;

            DeadAlready = true;
            var killer = KillingFactors.Instance.IdentifyKiller();
            var deathCam = __instance.gameObject.AddComponent<DeathCam>();
            deathCam.killer = killer;
            deathCam.enabled = true;
        }
    }
}