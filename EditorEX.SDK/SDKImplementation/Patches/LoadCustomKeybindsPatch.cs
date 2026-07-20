using System;
using System.Linq;
using System.Reflection;
using BeatmapEditor3D.InputSystem;
using HarmonyLib;

namespace EditorEX.SDKImplementation.Patches;

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

    public static InputAction RedirectLoad(string inputActionName)
    {
        if (!Enum.TryParse(inputActionName, out InputAction inputAction))
        {
            return InjectCustomKeybindings
                    ._instance._customInputActionRegistry.GetGroups()
                    .SelectMany(x => x.GetKeybindings())
                    .FirstOrDefault(x => x.Name == inputActionName)
                    ?.GetInputAction()
                ?? InputAction.None;
        }

        return inputAction;
    }

    [HarmonyPrefix]
    static bool Prefix(string value, bool ignoreCase, ref InputAction __result)
    {
        __result = RedirectLoad(value);
        return false;
    }
}
