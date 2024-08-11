using HarmonyLib;
using UKEnemyIdentifier.Components;
using UnityEngine;

namespace UKEnemyIdentifier.Patches
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

            EnemyIdentifierManager.Instance.RegisterFactor(currentProjectile.gameObject, ___eid);

            if (!___currentProjectile.TryGetComponent<ProjectileSpread>(out _))
                return;
            
            foreach (var projectile in ___currentProjectile.GetComponentsInChildren<Projectile>())
                EnemyIdentifierManager.Instance.RegisterFactor(projectile.gameObject, ___eid);
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZombieProjectiles), "ShootProjectile")]
        public static void ZombieProjectiles_ShootProjectile_Postfix(GameObject ___currentProjectile, EnemyIdentifier ___eid, ZombieProjectiles __instance)
        {
            if (___currentProjectile == default)
                return;
            
            if (___currentProjectile.TryGetComponent<Projectile>(out var projectileComponent))
                EnemyIdentifierManager.Instance.RegisterFactor(projectileComponent.gameObject, ___eid);

            if (___currentProjectile.TryGetComponent<ProjectileSpread>(out var projectileSpreadComponent))
            {
                var projectile = projectileSpreadComponent.GetComponentInChildren<ProjectileSpread>();
                EnemyIdentifierManager.Instance.RegisterFactor(projectile.gameObject, ___eid);
            }
        }
    }
}