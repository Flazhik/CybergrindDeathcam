using HarmonyLib;
using UKEnemyIdentifier.Components;
using UnityEngine;

namespace UKEnemyIdentifier.Patches
{
    [HarmonyPatch(typeof(SpiderBody))]
    public class SpiderBodyPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpiderBody), "ShootProj")]
        public static void SpiderBody_ShootProj_Postfix(GameObject ___currentProj, EnemyIdentifier ___eid)
        {
            EnemyIdentifierManager.Instance.RegisterFactor(___currentProj.gameObject, ___eid);
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpiderBody), "BeamFire")]
        public static void SpiderBody_BeamFire_Postfix(GameObject ___currentBeam, EnemyIdentifier ___eid)
        {
            if (___currentBeam == null || !___currentBeam.TryGetComponent<RevolverBeam>(out var component))
                return;
            
            EnemyIdentifierManager.Instance.RegisterFactor(component.gameObject, ___eid);
        }
    }
}