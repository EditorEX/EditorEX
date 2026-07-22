using System;
using System.Linq;
using System.Reflection;
using BeatmapEditor3D.InputSystem;
using HarmonyLib;

namespace EditorEX.SDK.Integration.Patches;

[HarmonyPatch]
public class LoadCustomKeybindsPatch
{
    public static MethodInfo TargetMethod()
    {
        return typeof(Enum)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == nameof(Enum.Parse) && m.IsGenericMethodDefinition)
            .Where(m =>
            {
                var p = m.GetParameters();
                return p.Length == 2
                    && p[0].ParameterType == typeof(string)
                    && p[1].ParameterType == typeof(bool);
            })
            .Single()
            .MakeGenericMethod(typeof(InputAction));
    }

    [HarmonyPrefix]
    static bool Prefix(string value, bool ignoreCase, ref InputAction __result)
    {
        __result = CustomKeybindingPersistencePatches.RedirectLoad(value);
        return false;
    }
}
