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
    [HarmonyPatch(typeof(Nail))]
    public class NailPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Nail), "OnCollisionEnter")]
        public static IEnumerable<CodeInstruction> EnemyShotgun_AltFire_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;

                SetNailHurtingFactor(codeInstructions, i - 8);
                break;
            }

            return codeInstructions;
        }
        
        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Callvirt && instruction.OperandIs(Method(typeof(NewMovement), "GetHurt", new []
            {
                typeof(int), typeof(bool), typeof(float), typeof(bool), typeof(bool), typeof(float), typeof(bool)
            }));
        
        private static void SetNailHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldarg_0, null),
                (Call, Method(typeof(Component), "get_gameObject")),
                (Stfld, Field(typeof(EnemyIdentifierManager), "LastHurtingFactor"))));
        }
    }
}