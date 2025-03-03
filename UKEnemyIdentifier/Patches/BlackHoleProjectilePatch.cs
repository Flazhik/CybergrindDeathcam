using HarmonyLib;
using UKEnemyIdentifier.Components;
using UnityEngine;

namespace UKEnemyIdentifier.Patches
{
    [HarmonyPatch(typeof(BlackHoleProjectile))]
    public class BlackHoleProjectilePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlackHoleProjectile), "OnTriggerEnter")]
        public static bool BlackHoleProjectile_OnTriggerEnter_Prefix(BlackHoleProjectile __instance, Collider other)
        {
            if (__instance.enemy && __instance.target != null && __instance.target.IsTargetTransform(other.gameObject.transform))
                EnemyIdentifierManager.Instance.LastHurtingFactor = __instance.gameObject;

            return true;
        }
    }
}