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
    [HarmonyPatch(typeof(MortarLauncher))]
    public class MortarLauncherPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MortarLauncher), "ShootHoming")]
        public static IEnumerable<CodeInstruction> MortarLauncher_ShootHoming_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;
                
                SetProjectileHurtingFactor(codeInstructions, i + 2, generator);
                break;
            }

            return codeInstructions;
        }
        
        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stfld && instruction.OperandIs(Field(typeof(Projectile), "damage"));
        
        private static void SetProjectileHurtingFactor(List<CodeInstruction> instructions, int index, ILGenerator generator)
        {
            var projectile = generator.DeclareLocal(typeof(Projectile));
            instructions.InsertRange(index, IL(
                (Stloc_S, projectile),
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_S, projectile),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(MortarLauncher), "eid")),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Dup, null)));
        }
    }
}