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
    [HarmonyPatch(typeof(CancerousRodent))]
    public class CancerousRodentPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(CancerousRodent), "FireBurst")]
        public static IEnumerable<CodeInstruction> CancerousRodent_FireBurst_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;

                SetProjectileHurtingFactor(codeInstructions, i + 1);
                break;
            }

            return codeInstructions;
        }
        
        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stfld && instruction.OperandIs(Field(typeof(Projectile), "damage"));

        private static void SetProjectileHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_0, null),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(CancerousRodent), "eid")),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Nop, null)));
        }
    }
}