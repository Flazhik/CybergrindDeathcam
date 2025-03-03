using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UKEnemyIdentifier.Components;
using UnityEngine;
using static HarmonyLib.AccessTools;
using static System.Reflection.Emit.OpCodes;
using static UKEnemyIdentifier.Utils.ReflectionUtils;

namespace UKEnemyIdentifier.Patches
{
    [HarmonyPatch(typeof(Coin))]
    public class CoinPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Coin), "ShootAtPlayer")]
        public static IEnumerable<CodeInstruction> Coin_ShootAtPlayer_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!HurtingFactorInjectionPoint(codeInstructions[i]))
                    continue;
                
                SetCoinHurtingFactor(codeInstructions, i - 12);
                break;
            }

            return codeInstructions;
        }
        
        private static bool HurtingFactorInjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Callvirt && instruction.OperandIs(Method(typeof(NewMovement), "GetHurt", new []
            {
                typeof(int), typeof(bool), typeof(float), typeof(bool), typeof(bool), typeof(float), typeof(bool)
            }));
        
        private static void SetCoinHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldarg_0, null),
                (Call, Method(typeof(Component), "get_gameObject")),
                (Stfld, Field(typeof(EnemyIdentifierManager), "LastHurtingFactor"))));
        }
    }
}