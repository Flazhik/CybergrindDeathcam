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
    [HarmonyPatch(typeof(EnemyNailgun))]
    public class EnemyNailgunPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(EnemyNailgun), "FixedUpdate")]
        public static IEnumerable<CodeInstruction> EnemyShotgun_AltFire_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!NailInjectionPoint(codeInstructions[i]))
                    continue;

                SetNailHurtingFactor(codeInstructions, i + 1);
                break;
            }

            return codeInstructions;
        }

        private static bool NailInjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stloc_1;

        private static void SetNailHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_1, null),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(EnemyNailgun), "eid")),
                (Callvirt,
                    Method(typeof(EnemyIdentifierManager), "RegisterFactor",
                        new[] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Nop, null)));
        }
    }
}
