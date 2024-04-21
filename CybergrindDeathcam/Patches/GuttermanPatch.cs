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
    [HarmonyPatch(typeof(Gutterman))]
    public class GuttermanPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Gutterman), "FixedUpdate")]
        private static IEnumerable<CodeInstruction> Gutterman_Shoot_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;
                
                SetBeamLocalVariable(codeInstructions, generator, i + 1);
                SetBeamHurtingFactor(codeInstructions, i + 15);
                break;
            }

            return codeInstructions;
        }

        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Call && instruction.operand.ToString().Contains("Instantiate");

        private static void SetBeamLocalVariable(List<CodeInstruction> instructions, ILGenerator generator, int index)
        {
            var beam = generator.DeclareLocal(typeof(GameObject));
            instructions.InsertRange(index, IL(
                (Stloc_2, beam),
                (Ldloc_2, null)));
        }
        
        private static void SetBeamHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(KillingFactors), "get_Instance")),
                (Ldloc_2, null),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(Gutterman), "eid")),
                (Callvirt, Method(typeof(KillingFactors), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Nop, null)));
        }
    }
}