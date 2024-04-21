using CybergrindDeathcam.Components;
using HarmonyLib;
using UnityEngine;

namespace CybergrindDeathcam.Patches
{
    [HarmonyPatch(typeof(SpiderBody))]
    public class SpiderBodyPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpiderBody), "ShootProj")]
        public static void SpiderBody_ShootProj_Postfix(GameObject ___currentProj, EnemyIdentifier ___eid)
        {
            KillingFactors.Instance.RegisterFactor(___currentProj.gameObject, ___eid);
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpiderBody), "BeamFire")]
        public static void SpiderBody_BeamFire_Postfix(GameObject ___currentBeam, EnemyIdentifier ___eid)
        {
            if (___currentBeam == null || !___currentBeam.TryGetComponent<RevolverBeam>(out var component))
                return;
            
            KillingFactors.Instance.RegisterFactor(component.gameObject, ___eid);
        }
    }
}