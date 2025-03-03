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
    [HarmonyPatch(typeof(Mandalore))]
    public class MandalorePatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Mandalore), "FullBurst")]
        public static IEnumerable<CodeInstruction> Mandalore_FullBurst_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!FullAutoInjectionPoint(codeInstructions[i]))
                    continue;

                SetFullAutoHurtingFactor(codeInstructions, i + 2, generator);
                break;
            }

            return codeInstructions;
        }
        
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Mandalore), "FullerBurst")]
        public static IEnumerable<CodeInstruction> Mandalore_FullerBurst_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!FullerAutoInjectionPoint(codeInstructions[i]))
                    continue;

                SetFullerAutoHurtingFactor(codeInstructions, i + 2, generator);
                break;
            }

            return codeInstructions;
        }
        
        private static bool FullAutoInjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stfld && instruction.OperandIs(Field(typeof(Projectile), "precheckForCollisions"));
        
        private static bool FullerAutoInjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stfld && instruction.OperandIs(Field(typeof(Projectile), "safeEnemyType"));

        private static void SetFullAutoHurtingFactor(List<CodeInstruction> instructions, int index, ILGenerator generator)
        {
            var projectile = generator.DeclareLocal(typeof(Projectile));
            instructions.InsertRange(index, IL(
                (Stloc_S, projectile),
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_S, projectile),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(Mandalore), "eid")),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Dup, null)));
        }
        
        private static void SetFullerAutoHurtingFactor(List<CodeInstruction> instructions, int index, ILGenerator generator)
        {
            var projectile = generator.DeclareLocal(typeof(Projectile));
            instructions.InsertRange(index, IL(
                (Stloc_S, projectile),
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_S, projectile),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(Mandalore), "eid")),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Dup, null)));
        }
    }
}