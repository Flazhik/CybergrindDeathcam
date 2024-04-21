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
    [HarmonyPatch(typeof(Turret))]
    public class TurretPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Turret), "Shoot")]
        private static IEnumerable<CodeInstruction> Mass_Shoot_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;

                RegisterBeamHurtingFactor(codeInstructions, i + 1);
                break;
            }

            return codeInstructions;
        }

        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stfld && instruction.OperandIs(Field(typeof(RevolverBeam), "target"));

        private static void RegisterBeamHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(KillingFactors), "get_Instance")),
                (Ldloc_0, null),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(Turret), "eid")),
                (Callvirt, Method(typeof(KillingFactors), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Nop, null)));
        }
    }
}