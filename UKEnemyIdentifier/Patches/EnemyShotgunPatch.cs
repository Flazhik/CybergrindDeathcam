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
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_S, projectile),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(EnemyShotgun), "eid")),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Nop, null)));
        }
    }
}