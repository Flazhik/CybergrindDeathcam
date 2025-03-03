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
    [HarmonyPatch(typeof(GabrielSecond))]
    public class GabrielSecondPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GabrielSecond), "ThrowSwords")]
        public static void GabrielSecond_ThrowSwords_Postfix(Projectile ___currentCombinedSwordsThrown, EnemyIdentifier ___eid)
        {
            EnemyIdentifierManager.Instance.RegisterFactor(___currentCombinedSwordsThrown.gameObject, ___eid);
        }        
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GabrielSecond), "SpawnSummonedSwords")]
        public static void GabrielSecond_SpawnSummonedSwords_Postfix(GameObject ___currentSwords, EnemyIdentifier ___eid)
        {
            foreach (var sword in ___currentSwords.GetComponentsInChildren<Projectile>())
                EnemyIdentifierManager.Instance.RegisterFactor(sword.gameObject, ___eid);
        }
        
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GabrielSecond), "FixedUpdate")]
        public static IEnumerable<CodeInstruction> GabrielSecond_FixedUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;
                
                SetHurtingFactor(codeInstructions, i - 14);
                break;
            }

            return codeInstructions;
        }
        
        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Callvirt && instruction.OperandIs(Method(typeof(NewMovement), "GetHurt", new []
            {
                typeof(int), typeof(bool), typeof(float), typeof(bool), typeof(bool), typeof(float), typeof(bool)
            }));
        
        private static void SetHurtingFactor(List<CodeInstruction> instructions, int index) =>
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldarg_0, null),
                (Call, Method(typeof(Component), "get_gameObject")),
                (Stfld, Field(typeof(EnemyIdentifierManager), "LastHurtingFactor"))));
    }
}