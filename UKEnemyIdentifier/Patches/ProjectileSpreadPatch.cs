using HarmonyLib;
using UKEnemyIdentifier.Components;

namespace UKEnemyIdentifier.Patches
{
    [HarmonyPatch(typeof(ProjectileSpread))]
    public class ProjectileSpreadPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProjectileSpread), "Start")]
        private static bool ProjectileSpread_Start_Prefix(ProjectileSpread __instance, ref EnemyIdentifier __state)
        {
            var eid = EnemyIdentifierManager.Instance.GetIdentifierFor(__instance.GetComponentInChildren<Projectile>());
            if (eid != default)
                __state = eid;
            
            return true;
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProjectileSpread), "Start")]
        private static void ProjectileSpread_Start_Postfix(ProjectileSpread __instance, EnemyIdentifier __state)
        {
            if (__state == null)
                return;
            foreach (var projectile in __instance.GetComponentsInChildren<Projectile>())
                EnemyIdentifierManager.Instance.RegisterFactor(projectile.gameObject, __state);
        }
    }
}