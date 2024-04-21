using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using CybergrindDeathcam.Components;
using HarmonyLib;
using UnityEngine;
using static HarmonyLib.AccessTools;
using static System.Reflection.Emit.OpCodes;
using static CybergrindDeathcam.Utils.ReflectionUtils;

namespace CybergrindDeathcam.Patches
{
    [HarmonyPatch(typeof(Mass))]
    public class MassPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Mass), "ShootHoming")]
        private static IEnumerable<CodeInstruction> Mass_ShootHoming_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;

                SetProjectileHurtingFactor(codeInstructions, i + 2, generator);
                break;
            }

            return codeInstructions;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Mass), "ShootExplosive")]
        private static IEnumerable<CodeInstruction> Mass_ShootExplosive_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i]))
                    continue;

                SetProjectileHurtingFactor(codeInstructions, i + 2, generator);
                break;
            }

            return codeInstructions;
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mass), "ShootSpear")]
        private static void Mass_ShootSpear_Postfix(EnemyIdentifier ___eid, Mass __instance)
        {
            if (__instance.tempSpear.TryGetComponent<MassSpear>(out var component))
            {
                KillingFactors.Instance.RegisterFactor(component.gameObject, ___eid);
            }
        }
        
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Mass), "SlamImpact")]
        private static IEnumerable<CodeInstruction> Mass_SlamImpact_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            SetWaveHurtingFactor(codeInstructions, codeInstructions.Count - 1);

            return codeInstructions;
        }        
        
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Mass), "SwingEnd")]
        private static IEnumerable<CodeInstruction> Mass_SwingEnd_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            SetSwingEndWaveHurtingFactor(codeInstructions, codeInstructions.Count - 1);

            return codeInstructions;
        }
        
        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stfld && instruction.OperandIs(Field(typeof(Projectile), "target"));

        private static void SetWaveHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(KillingFactors), "get_Instance")),
                (Ldloc_1, null),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(Mass), "eid")),
                (Callvirt, Method(typeof(KillingFactors), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Nop, null)));
        }
        
        private static void SetSwingEndWaveHurtingFactor(List<CodeInstruction> instructions, int index)
        {
            instructions.InsertRange(index, IL(
                (Call, Method(typeof(KillingFactors), "get_Instance")),
                (Ldloc_0, null),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(Mass), "eid")),
                (Callvirt, Method(typeof(KillingFactors), "RegisterFactor", new [] { typeof(GameObject), typeof(EnemyIdentifier) })),
                (Nop, null)));
        }
        
        private static void SetProjectileHurtingFactor(List<CodeInstruction> instructions, int index, ILGenerator generator)
        {
            var projectile = generator.DeclareLocal(typeof(Projectile));
            instructions.InsertRange(index, IL(
                (Stloc_S, projectile),
                (Call, Method(typeof(KillingFactors), "get_Instance")),
                (Ldloc_S, projectile),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(Mass), "eid")),
                (Callvirt, Method(typeof(KillingFactors), "RegisterFactor", new []
                {
                    typeof(GameObject), typeof(EnemyIdentifier)
                })),
                (Dup, null)));
        }
    }
}