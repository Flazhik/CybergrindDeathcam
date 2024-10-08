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
    [HarmonyPatch(typeof(Explosion))]
    public class ExplosionPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Explosion), "Collide")]
        public static IEnumerable<CodeInstruction> Explosion_Collide_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            SetLastHurtingFactor(codeInstructions, 0);
            return codeInstructions;
        }
        
        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Callvirt && instruction.OperandIs(Method(typeof(NewMovement), "GetHurt", new []
            {
                typeof(int), typeof(bool), typeof(float), typeof(bool), typeof(bool), typeof(float), typeof(bool)
            }));

        private static void SetLastHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldarg_0, null),
                (Call, Method(typeof(Component), "get_gameObject")),
                (Stfld, Field(typeof(EnemyIdentifierManager), "LastHurtingFactor"))));
        }
    }
}