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
    [HarmonyPatch(typeof(Sisyphus))]
    public class SisyphusPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Sisyphus), "FixedUpdate")]
        public static IEnumerable<CodeInstruction> StatueBoss_FixedUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!LandingWaveInjectionPoint(codeInstructions[i]))
                    continue;

                SetWaveHurtingFactor(codeInstructions, i + 1);
                break;
            }

            return codeInstructions;
        }        

        private static bool LandingWaveInjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stfld && instruction.OperandIs(Field(typeof(PhysicalShockwave), "speed"));
        
        private static void SetWaveHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_2, null),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(Sisyphus), "eid")),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Nop, null)));
        }
    }
}