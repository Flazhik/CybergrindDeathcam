using HarmonyLib;
using UKEnemyIdentifier.Components;

namespace UKEnemyIdentifier.Patches
{
    [HarmonyPatch(typeof(Drone))]
    public class DronePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Drone), "SetProjectileSettings")]
        public static void Drone_SetProjectileSettings_Postfix(Projectile proj, EnemyIdentifier ___eid)
        {
            EnemyIdentifierManager.Instance.RegisterFactor(proj.gameObject, ___eid);
        }
    }
}