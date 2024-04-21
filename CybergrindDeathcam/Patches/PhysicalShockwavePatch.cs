using CybergrindDeathcam.Components;
using HarmonyLib;
using UnityEngine;

namespace CybergrindDeathcam.Patches
{
    [HarmonyPatch(typeof(Explosion))]
    public class PhysicalShockwavePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PhysicalShockwave), "CheckCollision")]
        public static bool PhysicalShockwave_CheckCollision_Prefix(Collider col, PhysicalShockwave __instance)
        {
            if (!__instance.hasHurtPlayer && col.gameObject.layer != 15 && col.gameObject.CompareTag("Player"))
                KillingFactors.Instance.LastHurtingFactor = __instance.gameObject;

            return true;
        }
    }
}