using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using CybergrindDeathcam.Components;
using HarmonyLib;
using UnityEngine;
using static HarmonyLib.AccessTools;
using static System.Reflection.Emit.OpCodes;
using static CybergrindDeathcam.Utils.ReflectionUtils;

namespace CybergrindDeathcam.Patches
{
    [HarmonyPatch(typeof(VirtueInsignia))]
    public class VirtueInsigniaPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(VirtueInsignia), "OnTriggerEnter")]
        private static IEnumerable<CodeInstruction> VirtueInsignia_OnTriggerEnter_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;
                
                SetBeamHurtingFactor(codeInstructions, i + 1);
                break;
            }

            return codeInstructions;
        }

        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Callvirt && instruction.OperandIs(Method(typeof(NewMovement), "LaunchFromPoint", new []
            {
                typeof(Vector3), typeof(float), typeof(float)
            }));

        private static void SetBeamHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(KillingFactors), "get_Instance")),
                (Ldarg_0, null),
                (Call, Method(typeof(Component), "get_gameObject")),
                (Stfld, Field(typeof(KillingFactors), "LastHurtingFactor"))));
        }
    }
}