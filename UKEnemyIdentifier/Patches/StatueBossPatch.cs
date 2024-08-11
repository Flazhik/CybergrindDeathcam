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
    [HarmonyPatch(typeof(StatueBoss))]
    public class StatueBossPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StatueBoss), "OrbSpawn")]
        public static IEnumerable<CodeInstruction> StatueBoss_OrbSpawn_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!OrbSpawnInjectionPoint(codeInstructions[i]))
                    continue;

                SetOrbHurtingFactor(codeInstructions, i + 1);
                break;
            }

            return codeInstructions;
        }        
        
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StatueBoss), "StompHit")]
        public static IEnumerable<CodeInstruction> StatueBoss_StompHit_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!StompHitInjectionPoint(codeInstructions[i]))
                    continue;

                SetStompHitHurtingFactor(codeInstructions, i + 1);
                break;
            }

            return codeInstructions;
        }
        
        private static bool OrbSpawnInjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stfld && instruction.OperandIs(Field(typeof(Projectile), "target"));
        
        private static bool StompHitInjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stfld && instruction.OperandIs(Field(typeof(PhysicalShockwave), "enemyType"));
        
        private static void SetOrbHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_1, null),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(StatueBoss), "eid")),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Nop, null)));
        }        
        
        private static void SetStompHitHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_2, null),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(StatueBoss), "eid")),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Nop, null)));
        }
    }
}