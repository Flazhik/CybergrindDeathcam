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
    [HarmonyPatch(typeof(DroneFlesh))]
    public class DroneFleshPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(DroneFlesh), "ShootBeam")]
        public static IEnumerable<CodeInstruction> DroneFlesh_ShootBeam_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!BeamInjectionPoint(codeInstructions[i]))
                    continue;

                SetRevolverHurtingFactor(codeInstructions, i + 1);
                break;
            }

            return codeInstructions;
        }
        
        private static bool BeamInjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stloc_0;

        private static void SetRevolverHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_0, null),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(DroneFlesh), "eid")),
                (Callvirt,
                    Method(typeof(EnemyIdentifierManager), "RegisterFactor",
                        new[] { typeof(GameObject), typeof(EnemyIdentifier) }))));
        }
    }
}