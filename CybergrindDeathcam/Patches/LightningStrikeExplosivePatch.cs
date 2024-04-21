using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using CybergrindDeathcam.Components;
using HarmonyLib;
using static HarmonyLib.AccessTools;
using static System.Reflection.Emit.OpCodes;
using static CybergrindDeathcam.Utils.ReflectionUtils;

namespace CybergrindDeathcam.Patches
{
    [HarmonyPatch(typeof(LightningStrikeExplosive))]
    public class LightningStrikeExplosivePatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(LightningStrikeExplosive), "Start")]
        public static IEnumerable<CodeInstruction> LightningStrikeExplosive_SpawnLightningBolt_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (codeInstructions[i].opcode != Stloc_S
                    || !(codeInstructions[i].operand is LocalBuilder)
                    || ((LocalBuilder)codeInstructions[i].operand).LocalIndex != 6)
                    continue;
                
                var explosionOperand = (LocalBuilder)codeInstructions[i].operand;

                SetLightningExplosionHurtingFactor(codeInstructions, explosionOperand, i);
                break;
            }

            return codeInstructions;
        }

        private static void SetLightningExplosionHurtingFactor(List<CodeInstruction> instructions, LocalBuilder explosion, int index)
        {
            instructions.InsertRange(index + 1, IL(
                (Call, Method(typeof(KillingFactors), "get_Instance")),
                (Ldarg_0, null),
                (Ldloc_S, explosion),
                (Callvirt, Method(typeof(KillingFactors), "RegisterLightningStrikeExplosion", new [] { typeof(LightningStrikeExplosive), typeof(Explosion) })),
                (Nop, null)));
        }
    }
}