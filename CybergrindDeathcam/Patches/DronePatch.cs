using CybergrindDeathcam.Components;
using HarmonyLib;

namespace CybergrindDeathcam.Patches
{
    [HarmonyPatch(typeof(Drone))]
    public class DronePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Drone), "SetProjectileSettings")]
        public static void Drone_SetProjectileSettings_Postfix(Projectile proj, EnemyIdentifier ___eid)
        {
            KillingFactors.Instance.RegisterFactor(proj.gameObject, ___eid);
        }
    }
}