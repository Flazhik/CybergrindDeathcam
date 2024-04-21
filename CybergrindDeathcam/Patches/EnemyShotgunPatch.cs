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
    [HarmonyPatch(typeof(EnemyShotgun))]
    public class EnemyShotgunPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(EnemyShotgun), "Fire")]
        public static IEnumerable<CodeInstruction> EnemyShotgun_Fire_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            LocalBuilder projectile = default;
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (ProjectileOperand(codeInstructions[i]))
                    projectile = (LocalBuilder)codeInstructions[i].operand;
                
                if (!InjectionPoint(codeInstructions[i]))
                    continue;
                
                if (projectile != default)
                    SetShotgunHurtingFactor(codeInstructions, projectile, i);
                break;
            }

            return codeInstructions;
        }
        
        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stfld && instruction.OperandIs(Field(typeof(Projectile), "spreaded"));

        private static bool ProjectileOperand(CodeInstruction instruction) =>
            instruction.opcode == Ldloc_S
            && instruction.operand is LocalBuilder builder
            && builder.LocalType == typeof(Projectile);
        
        private static void SetShotgunHurtingFactor(List<CodeInstruction> instructions, LocalBuilder projectile, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(KillingFactors), "get_Instance")),
                (Ldloc_S, projectile),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(EnemyShotgun), "eid")),
                (Callvirt, Method(typeof(KillingFactors), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Nop, null)));
        }
    }
}