using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using static HarmonyLib.AccessTools;
using static System.Reflection.Emit.OpCodes;
using static UKEnemyIdentifier.Utils.ReflectionUtils;

namespace UKEnemyIdentifier.Patches
{
    [HarmonyPatch(typeof(V2))]
    public class V2Patch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(V2), "ThrowCoins")]
        public static IEnumerable<CodeInstruction> V2_ThrowCoins_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;

                SetCoinEnemyId(codeInstructions, i + 9);
                break;
            }

            return codeInstructions;
        }
        
        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Ldfld && instruction.OperandIs(Field(typeof(Coin), "flash"));
        
        private static void SetCoinEnemyId(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Ldloc_2, null),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(V2), "eid")),
                (Stfld, Field(typeof(Coin), "eid"))));
        }
    }
}