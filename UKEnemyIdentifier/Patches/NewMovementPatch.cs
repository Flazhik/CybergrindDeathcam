using HarmonyLib;
using UKEnemyIdentifier.Components;

namespace UKEnemyIdentifier.Patches
{
    [HarmonyPatch(typeof(NewMovement))]
    public class NewMovementPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NewMovement), "GetHurt")]
        public static void NewMovement_GetHurt_Postfix(NewMovement __instance, int damage)
        {
            var manager = EnemyIdentifierManager.Instance;
            if (manager.DeadAlready || damage == 0)
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
    }
}