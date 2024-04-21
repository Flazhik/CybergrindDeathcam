using CybergrindDeathcam.Components;
using HarmonyLib;

namespace CybergrindDeathcam.Patches
{
    [HarmonyPatch(typeof(MassSpear))]
    public class MassSpearPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MassSpear), "DelayedPlayerCheck")]
        private static bool Mass_ShootHoming_Prefix(bool ___deflected, MassSpear __instance)
        {
            if (___deflected)
                return false;
            
            KillingFactors.Instance.LastHurtingFactor = __instance.gameObject;
            return true;
        }
    }
}