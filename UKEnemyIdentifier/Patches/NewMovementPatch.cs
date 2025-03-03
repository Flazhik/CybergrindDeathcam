using HarmonyLib;
using UKEnemyIdentifier.Components;
using UnityEngine;

namespace UKEnemyIdentifier.Patches
{
    [HarmonyPatch(typeof(NewMovement))]
    public class NewMovementPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NewMovement), "GetHurt")]
        public static void NewMovement_GetHurt_Prefix(NewMovement __instance, ref NewMovementPatchState __state)
        {
            __state = new NewMovementPatchState
            {
                Invincible = __instance.gameObject.layer == LayerMask.NameToLayer("Invincible")
            };
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NewMovement), "GetHurt")]
        public static void NewMovement_GetHurt_Postfix(NewMovement __instance, NewMovementPatchState __state, int damage)
        {
            var manager = EnemyIdentifierManager.Instance;
            if (manager.DeadAlready || damage == 0 || __state.Invincible)
                return;
            
            var killer = manager.IdentifyEnemy();
            var playerKilled = __instance.hp == 0;
            
            if (playerKilled)
                manager.DeadAlready = true;
            
            manager.PlayerHurt(killer, damage, playerKilled);
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NewMovement), "Respawn")]
        public static void NewMovement_Respawn_Prefix(NewMovement __instance)
        {
            EnemyIdentifierManager.Instance.DeadAlready = false;
        }

        public struct NewMovementPatchState
        {
            public bool Invincible;
        }
    }
}