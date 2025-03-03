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
    [HarmonyPatch(typeof(EnemyRevolver))]
    public class EnemyRevolverPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(EnemyRevolver), "Fire")]
        public static IEnumerable<CodeInstruction> EnemyRevolver_Fire_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            SetRevolverHurtingFactor(codeInstructions);
            return codeInstructions;
        }
        
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(EnemyRevolver), "AltFire")]
        public static IEnumerable<CodeInstruction> EnemyRevolver_AltFire_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            SetRevolverHurtingFactor(codeInstructions);
            return codeInstructions;
        }

        private static void SetRevolverHurtingFactor(List<CodeInstruction> instructions)
        {
            var injection = IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_1, null),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(EnemyRevolver), "eid")),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })));
            instructions.InsertRange(instructions.Count - 1, injection);
        }
    }
}