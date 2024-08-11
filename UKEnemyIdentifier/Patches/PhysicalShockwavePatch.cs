using HarmonyLib;
using UKEnemyIdentifier.Components;
using UnityEngine;

namespace UKEnemyIdentifier.Patches
{
    [HarmonyPatch(typeof(Explosion))]
    public class PhysicalShockwavePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PhysicalShockwave), "CheckCollision")]
        public static bool PhysicalShockwave_CheckCollision_Prefix(Collider col, PhysicalShockwave __instance)
        {
            if (!__instance.hasHurtPlayer && col.gameObject.layer != 15 && col.gameObject.CompareTag("Player"))
                EnemyIdentifierManager.Instance.LastHurtingFactor = __instance.gameObject;

            return true;
        }
    }
}