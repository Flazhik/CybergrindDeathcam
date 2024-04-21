using System.Collections.Generic;
using System.Linq;
using CybergrindDeathcam.Components;
using HarmonyLib;
using UnityEngine;
using static HarmonyLib.AccessTools;
using static System.Reflection.Emit.OpCodes;
using static CybergrindDeathcam.Utils.ReflectionUtils;

namespace CybergrindDeathcam.Patches
{
    [HarmonyPatch(typeof(Projectile))]
    public class ProjectilePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Projectile), "TimeToDie")]
        public static bool Projectile_TimeToDie_Prefix(Projectile __instance)
        {
            KillingFactors.Instance.LastHurtingFactor = __instance.gameObject;
            return true;
        }
        
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Projectile), "Explode")]
        public static IEnumerable<CodeInstruction> Projectile_Explode_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return PatchExplosions(instructions);
        }
        
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Projectile), "CreateExplosionEffect")]
        public static IEnumerable<CodeInstruction> Projectile_CreateExplosionEffect_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return PatchExplosions(instructions);
        }

        private static IEnumerable<CodeInstruction> PatchExplosions(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;

                SetExplosionHurtingFactor(codeInstructions, i + 1);
                break;
            }

            return codeInstructions;
        }

        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stloc_2;
        
        private static void SetExplosionHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Ldloc_2, null),
                (Call, Method(typeof(KillingFactors), "get_Instance")),
                (Ldarg_0, null),
                (Callvirt, Method(typeof(KillingFactors), "GetIdentifierFor", new [] { typeof(Projectile) })),
                (Stfld, Field(typeof(Explosion), "originEnemy"))));
        }
    }
}