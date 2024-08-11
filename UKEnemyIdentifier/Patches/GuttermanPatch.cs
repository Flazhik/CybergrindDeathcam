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
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_2, null),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(Gutterman), "eid")),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Nop, null)));
        }
    }
}