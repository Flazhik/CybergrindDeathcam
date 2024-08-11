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
    [HarmonyPatch(typeof(Mindflayer))]
    public class MindflayerPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Mindflayer), "ShootProjectiles")]
        public static IEnumerable<CodeInstruction> Mindflayer_ShootProjectiles_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeInstructions = instructions.ToList();
            for (var i = 2; i < codeInstructions.Count; i++)
            {
                if (!InjectionPoint(codeInstructions[i - 2]))
                    continue;

                RegisterProjectilesFactor(codeInstructions, generator, i);
                break;
            }

            return codeInstructions;
        }        
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mindflayer), "StartBeam")]
        public static void Mindflayer_StartBeam_Postfix(EnemyIdentifier ___eid, Mindflayer __instance)
        {
            if (__instance.tempBeam.TryGetComponent<ContinuousBeam>(out var beam))
            {
                EnemyIdentifierManager.Instance.RegisterFactor(beam.gameObject, ___eid);
            }
        }
        
        private static bool InjectionPoint(CodeInstruction instruction) =>
            instruction.opcode == Stfld && instruction.OperandIs(Field(typeof(Projectile), "target"));
        
        private static void RegisterProjectilesFactor(List<CodeInstruction> instructions, ILGenerator generator, int index)
        {
            var projectile = generator.DeclareLocal(typeof(Projectile));
            instructions.InsertRange(index, IL(
                (Dup, null),
                (Stloc_3, projectile),
                (Call, Method(typeof(EnemyIdentifierManager), "get_Instance")),
                (Ldloc_3, null),
                (Callvirt, Method(typeof(Component), "get_gameObject")),
                (Ldarg_0, null),
                (Ldfld, Field(typeof(Mindflayer), "eid")),
                (Callvirt, Method(typeof(EnemyIdentifierManager), "RegisterFactor", new []
                {
                    typeof(GameObject), typeof(EnemyIdentifier)
                })),
                (Nop, null)));
        }
    }
}