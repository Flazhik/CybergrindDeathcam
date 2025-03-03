using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UKEnemyIdentifier.Components;
using UnityEngine;
using static HarmonyLib.AccessTools;
using static System.Reflection.Emit.OpCodes;
using static UKEnemyIdentifier.Utils.ReflectionUtils;

namespace UKEnemyIdentifier.Patches
{
    [HarmonyPatch(typeof(FleshPrison))]
    public class FleshPrisonPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FleshPrison), "Update")]
        public static IEnumerable<CodeInstruction> FleshPrison_Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (codeInstructions[i].opcode != Stloc_S
                    || !(codeInstructions[i].operand is LocalBuilder)
                    || ((LocalBuilder)codeInstructions[i].operand).LocalIndex != 11)
                    continue;
                
                var projectileOperand = (LocalBuilder)codeInstructions[i].operand;

                SetProjectileHurtingFactor(codeInstructions, projectileOperand, i + 1);
                break;
            }

            return codeInstructions;
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FleshPrison), "SpawnBlackHole")]
        public static void FleshPrison_SpawnBlackHole_Postfix(
            FleshPrison __instance,
            BlackHoleProjectile ___currentBlackHole,
            EnemyIdentifier ___eid)
        {
            EnemyIdentifierManager.Instance.RegisterFactor(___currentBlackHole.gameObject, ___eid);
        }

        private static void SetProjectileHurtingFactor(List<CodeInstruction> instructions, LocalBuilder projectile, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_S, projectile),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(FleshPrison), "eid")),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) }))));
        }
    }
}