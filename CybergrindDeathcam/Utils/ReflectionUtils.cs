using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CybergrindDeathcam.Utils
{
    public static class ReflectionUtils
    {
        private const BindingFlags BindingFlagsFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        public static object GetPrivate<T>(T instance, Type classType, string field)
        {
            var privateField = classType.GetField(field, BindingFlagsFields);
            return privateField.GetValue(instance);
        }
        
        public static IEnumerable<CodeInstruction> IL(params (OpCode, object)[] instructions) =>
            instructions.Select(i => new CodeInstruction(i.Item1, i.Item2)).ToList();
    }
}