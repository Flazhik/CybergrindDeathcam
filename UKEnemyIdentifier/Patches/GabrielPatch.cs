using HarmonyLib;
using UKEnemyIdentifier.Components;
using UnityEngine;

namespace UKEnemyIdentifier.Patches
{
    [HarmonyPatch(typeof(Gabriel))]
    public class GabrielPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Gabriel), "ThrowWeapon")]
        public static void Gabriel_ThrowWeapon_Postfix(GameObject ___thrownObject, EnemyIdentifier ___eid)
        {
            foreach (var projectile in ___thrownObject.GetComponentsInChildren<Projectile>())
                EnemyIdentifierManager.Instance.RegisterFactor(projectile.gameObject, ___eid);
        }
    }
}