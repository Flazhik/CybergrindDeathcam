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
    [HarmonyPatch(typeof(Ferryman))]
    public class FerrymanPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Ferryman), "SpawnLightningBolt")]
        public static IEnumerable<CodeInstruction> Ferryman_SpawnLightningBolt_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            SetProjectileHurtingFactor(codeInstructions, codeInstructions.Count - 1);

            return codeInstructions;
        }

        private static void SetProjectileHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_0, null),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(Ferryman), "eid")),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Nop, null)));
        }
    }
}