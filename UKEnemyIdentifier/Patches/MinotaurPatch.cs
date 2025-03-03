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
    [HarmonyPatch(typeof(Minotaur))]
    public class MinotaurPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minotaur), "Start")]
        public static void Minotaur_Start_Postfix(
            Minotaur __instance,
            GameObject ___hammerExplosion,
            GameObject ___hammerBigExplosion)
        {
            foreach (var explosion in ___hammerExplosion.GetComponentsInChildren<Explosion>(true))
                explosion.originEnemy = (EnemyIdentifier)GetPrivate(__instance, typeof(Minotaur), "eid");
            
            foreach (var explosion in ___hammerBigExplosion.GetComponentsInChildren<Explosion>(true))
                explosion.originEnemy = (EnemyIdentifier)GetPrivate(__instance, typeof(Minotaur), "eid");
        }
        
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Minotaur), "MeatSplash")]
        public static IEnumerable<CodeInstruction> Minotaur_MeatSplash_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;
                
                SetGooHurtingFactor(codeInstructions, i + 1);
                break;
            }

            return codeInstructions;
        }
        
        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Callvirt && instruction.OperandIs(Method(typeof(Transform), "SetParent", new []
            {
                typeof(Transform), typeof(bool)
            }));
        
        private static void SetGooHurtingFactor(List<CodeInstruction> instructions, int index) =>
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldarg_0, null),
                (Call, Method(typeof(Component), "get_gameObject")),
                (Stfld, Field(typeof(EnemyIdentifierManager), "LastHurtingFactor"))));
    }
}