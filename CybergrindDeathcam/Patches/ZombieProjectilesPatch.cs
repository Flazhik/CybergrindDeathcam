using CybergrindDeathcam.Components;
using HarmonyLib;
using UnityEngine;

namespace CybergrindDeathcam.Patches
{
    [HarmonyPatch(typeof(ZombieProjectiles))]
    public class ZombieProjectilesPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZombieProjectiles), "ThrowProjectile")]
        public static void ZombieProjectiles_ThrowProjectile_Postfix(GameObject ___currentProjectile, EnemyIdentifier ___eid, ZombieProjectiles __instance)
        {
            var currentProjectile = ___currentProjectile.gameObject.GetComponentInChildren<Projectile>();
            if (currentProjectile == default)
                return;

            KillingFactors.Instance.RegisterFactor(currentProjectile.gameObject, ___eid);

            if (!___currentProjectile.TryGetComponent<ProjectileSpread>(out _))
                return;
            
            foreach (var projectile in ___currentProjectile.GetComponentsInChildren<Projectile>())
                KillingFactors.Instance.RegisterFactor(projectile.gameObject, ___eid);
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZombieProjectiles), "ShootProjectile")]
        public static void ZombieProjectiles_ShootProjectile_Postfix(GameObject ___currentProjectile, EnemyIdentifier ___eid, ZombieProjectiles __instance)
        {
            if (___currentProjectile == default)
                return;
            
            if (___currentProjectile.TryGetComponent<Projectile>(out var projectileComponent))
                KillingFactors.Instance.RegisterFactor(projectileComponent.gameObject, ___eid);

            if (___currentProjectile.TryGetComponent<ProjectileSpread>(out var projectileSpreadComponent))
            {
                var projectile = projectileSpreadComponent.GetComponentInChildren<ProjectileSpread>();
                KillingFactors.Instance.RegisterFactor(projectile.gameObject, ___eid);
            }
        }
    }
}