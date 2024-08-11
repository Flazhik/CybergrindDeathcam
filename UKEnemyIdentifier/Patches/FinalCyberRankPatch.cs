using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using static HarmonyLib.AccessTools;
using static System.Reflection.Emit.OpCodes;

namespace UKEnemyIdentifier.Patches
{
    [HarmonyPatch(typeof(FinalCyberRank))]
    public class FinalCyberRankPatch
    {
       
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FinalCyberRank), "Update")]
        public static IEnumerable<CodeInstruction> FinalCyberRank_Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;

                DisableCybergrindTimeFreeze(codeInstructions, i);
                break;
            }

            return codeInstructions;
        }

        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Call && instruction.OperandIs(Method(typeof(Time), "set_timeScale", new [] { typeof(float) }));
        
        private static void DisableCybergrindTimeFreeze(List<CodeInstruction> instructions, int index)
        {
            instructions.RemoveRange(index - 7, 8);
        }
    }
}