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
    [HarmonyPatch(typeof(RevolverBeam))]
    public class RevolverBeamPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(RevolverBeam), "HitSomething")]
        public static IEnumerable<CodeInstruction> RevolverBeam_HitSomething_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (codeInstructions[i].opcode != Stloc_S
                    || !(codeInstructions[i].operand is LocalBuilder)
                    || ((LocalBuilder)codeInstructions[i].operand).LocalIndex != 9)
                    continue;
                
                var explosionOperand = (LocalBuilder)codeInstructions[i].operand;
                RegisterBeamExplosion(codeInstructions, explosionOperand, i);
                break;
            }

            return codeInstructions;
        }        
        
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(RevolverBeam), "ExecuteHits")]
        public static IEnumerable<CodeInstruction> RevolverBeam_ExecuteHits_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!HurtingFactorInjectionPoint(codeInstructions[i]))
                    continue;
                
                SetBeamHurtingFactor(codeInstructions, i);
                break;
            }

            return codeInstructions;
        }
        
        private static bool HurtingFactorInjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Callvirt && instruction.OperandIs(Method(typeof(NewMovement), "GetHurt", new []
            {
                typeof(int), typeof(bool), typeof(float), typeof(bool), typeof(bool), typeof(float), typeof(bool)
            }));
        
        private static void SetBeamHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index - 13, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldarg_0, null),
                (Call, Method(typeof(Component), "get_gameObject")),
                (Stfld, Field(typeof(EnemyIdentifierManager), "LastHurtingFactor"))));
        }
        
        private static void RegisterBeamExplosion(List<CodeInstruction> instructions, LocalBuilder explosion, int index)
        {
            instructions.InsertRange(index + 1, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldarg_0, null),
                (Ldloc_S, explosion),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterRevolverBeamExplosion", new [] { typeof(RevolverBeam), typeof(Explosion) })),
                (Nop, null)));
        }
    }
}