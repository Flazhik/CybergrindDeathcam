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
    [HarmonyPatch(typeof(SisyphusPrime))]
    public class SisyphusPrimePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SisyphusPrime), "CreateShockwave")]
        public static void SisyphusPrime_CreateShockwave_Postfix(EnemyIdentifier ___eid, ref PhysicalShockwave __result)
        {
            EnemyIdentifierManager.Instance.RegisterFactor(__result.gameObject, ___eid);
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SisyphusPrime), "Start")]
        public static void SisyphusPrime_Start_Postfix(SisyphusPrime __instance)
        {
            foreach (var explosion in __instance.explosion.GetComponentsInChildren<Explosion>(true))
                explosion.originEnemy = (EnemyIdentifier)GetPrivate(__instance, typeof(SisyphusPrime), "eid");
            
            foreach (var explosion in __instance.sparkleExplosion.GetComponentsInChildren<Explosion>(true))
                explosion.originEnemy = (EnemyIdentifier)GetPrivate(__instance, typeof(SisyphusPrime), "eid");
        }
        
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(SisyphusPrime), "RiderKickActivate")]
        public static IEnumerable<CodeInstruction> SisyphusPrime_RiderKickActivate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            LocalBuilder shockwaveLocalVariable = default;
            
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (IsGetHurtInstruction(codeInstructions[i]))
                {
                    var hurtingInstructions = SetHurtingFactor();
                    codeInstructions.InsertRange(i - 13, hurtingInstructions);
                    i += hurtingInstructions.Count;
                }

                if (codeInstructions[i].opcode == Ldloc_S
                    && codeInstructions[i].operand is LocalBuilder
                    && ((LocalBuilder)codeInstructions[i].operand).LocalIndex == 7)
                
                    shockwaveLocalVariable = (LocalBuilder)codeInstructions[i].operand;

                if (IsShockwaveInstruction(codeInstructions[i]) && shockwaveLocalVariable != default)
                {
                    var hurtingInstructions = SetShockwaveHurtingFactor(shockwaveLocalVariable);
                    codeInstructions.InsertRange(i + 1, hurtingInstructions);
                    i += hurtingInstructions.Count;
                }
            }

            return codeInstructions;
        }        
        
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(SisyphusPrime), "DropAttackActivate")]
        public static IEnumerable<CodeInstruction> SisyphusPrime_DropAttackActivate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            LocalBuilder shockwaveLocalVariable = default;
            
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (IsGetHurtInstruction(codeInstructions[i]))
                {
                    var hurtingInstructions = SetHurtingFactor();
                    codeInstructions.InsertRange(i - 13, hurtingInstructions);
                    i += hurtingInstructions.Count;
                }

                if (codeInstructions[i].opcode == Ldloc_S
                    && codeInstructions[i].operand is LocalBuilder
                    && ((LocalBuilder)codeInstructions[i].operand).LocalIndex == 8)
                    shockwaveLocalVariable = (LocalBuilder)codeInstructions[i].operand;
                
                if (IsShockwaveInstruction(codeInstructions[i]) && shockwaveLocalVariable != default)
                {
                    var hurtingInstructions = SetShockwaveHurtingFactor(shockwaveLocalVariable);
                    codeInstructions.InsertRange(i + 1, hurtingInstructions);
                    i += hurtingInstructions.Count;
                }
            }

            return codeInstructions;
        }

        private static bool IsGetHurtInstruction(CodeInstruction instruction) =>
            instruction.opcode == Callvirt && instruction.OperandIs(Method(typeof(NewMovement), "GetHurt", new []
            {
                typeof(int), typeof(bool), typeof(float), typeof(bool), typeof(bool), typeof(float), typeof(bool)
            }));

        private static bool IsShockwaveInstruction(CodeInstruction instruction) =>
            instruction.opcode == Stfld && instruction.OperandIs(Field(typeof(PhysicalShockwave), "damage"));

        private static List<CodeInstruction> SetHurtingFactor() => IL(
            (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
            (Ldarg_0, null),
            (Call, Method(typeof(Component), "get_gameObject")),
            (Stfld, Field(typeof(EnemyIdentifierManager), "LastHurtingFactor")))
            .ToList();
        
        private static List<CodeInstruction> SetShockwaveHurtingFactor(LocalBuilder operand) => IL(
            (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
            (Ldloc_S, operand),
            (Callvirt, Method(typeof(Component), "get_gameObject")),
            (Ldarg_0, null),
            (Ldfld, Field(typeof(SisyphusPrime), "eid")),
            (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })))
            .ToList();
    }
}
