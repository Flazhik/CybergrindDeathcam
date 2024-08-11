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
    [HarmonyPatch(typeof(FireZone))]
    public class FireZonePatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FireZone), "OnTriggerStay")]
        public static IEnumerable<CodeInstruction> FireZone_OnTriggerStay_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;

                SetFireHurtingFactor(codeInstructions, i - 1);
                break;
            }

            return codeInstructions;
        }

        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Call && instruction.OperandIs(Method(typeof(PlayerTracker), "get_Instance"));
        
        private static void SetFireHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldarg_0, null),
                (Call, Method(typeof(Component), "get_gameObject")),
                (Stfld, Field(typeof(EnemyIdentifierManager), "LastHurtingFactor"))));
        }
    }
}