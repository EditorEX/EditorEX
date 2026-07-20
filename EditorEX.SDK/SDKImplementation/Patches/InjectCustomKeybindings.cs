using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapEditor3D.InputSystem;
using BeatmapEditor3D.InputSystem.SerializedData;
using BeatmapEditor3D.SerializedData;
using EditorEX.SDK.Input;
using EditorEX.SDK.Util;
using HarmonyLib;
using Newtonsoft.Json;
using SiraUtil.Affinity;
using SiraUtil.Logging;

namespace EditorEX.SDKImplementation.Patches
{
    public class InjectCustomKeybindings : IAffinity
    {
        private static readonly FieldInfo _extendedBindingGroups = AccessTools.Field(
            typeof(KeyBindings),
            "extendedBindingGroups"
        );

        private static readonly MethodInfo _redirect = AccessTools.Method(
            typeof(InjectCustomKeybindings),
            "Redirect"
        );

        private SiraLog _siraLog;
        internal static InjectCustomKeybindings _instance;
        internal CustomInputActionRegistry _customInputActionRegistry;

        private InjectCustomKeybindings(
            SiraLog siraLog,
            CustomInputActionRegistry customInputActionRegistry
        )
        {
            _siraLog = siraLog;
            _customInputActionRegistry = customInputActionRegistry;
            _instance = this;
        }

        BindingGroup[] InjectBindingGroups(BindingGroup[] originalBindingGroups)
        {
            _siraLog.Info("Injecting custom keybindings...");
            var bindingsList = originalBindingGroups.ToList();
            bindingsList.AddRange(
                _customInputActionRegistry
                    .GetGroups()
                    .Select(x => new BindingGroup(
                        x.GetKeyBindingGroupType(),
                        [
                            .. x.GetKeybindings()
                                .Select(a => new InputActionBinding
                                {
                                    inputAction = a.GetInputAction(),
                                    keysCombination = a.Keys.ToList(),
                                }),
                        ]
                    ))
            );
            return bindingsList.ToArray();
        }

        public static BindingGroup[] Redirect(BindingGroup[] originalBindingGroups)
        {
            return _instance.InjectBindingGroups(originalBindingGroups);
        }

        public static string RedirectSave(Type type, InputAction inputAction)
        {
            string fallbackName = inputAction.DisplayName();
            string? enumName = type.GetEnumName(inputAction);
            return
                ((int)inputAction > Enum.GetValues(type).Cast<int>().Max())
                || enumName == null
                || enumName == string.Empty
                ? fallbackName
                : enumName;
        }

        [AffinityPatch(typeof(KeyBindings), "GetDefault")]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .End()
                .MatchStartBackwards(new CodeMatch(OpCodes.Stfld, _extendedBindingGroups))
                .Insert(new CodeInstruction(OpCodes.Call, _redirect))
                .InstructionEnumeration();
            return result;
        }

        public static InputAction RedirectLoad(string inputActionName)
        {
            if (!Enum.TryParse(inputActionName, out InputAction inputAction))
            {
                return _instance
                        ._customInputActionRegistry.GetGroups()
                        .SelectMany(x => x.GetKeybindings())
                        .FirstOrDefault(x => x.Name == inputActionName)
                        ?.GetInputAction()
                    ?? InputAction.None;
            }

            return inputAction;
        }

        [AffinityPatch(typeof(KeyBindingsDataModelLoader), "Load")]
        [AffinityPrefix]
        private bool PrefixLoad(KeyBindingsDataModelLoader __instance, string path)
        {
            CustomKeyBindingsSerializedData? customKeyBindingsSerializedData = (
                File.Exists(path)
                    ? JsonConvert.DeserializeObject<CustomKeyBindingsSerializedData>(
                        File.ReadAllText(path)
                    )
                    : null
            );
            if (customKeyBindingsSerializedData == null)
            {
                return false;
            }

            var enumerable = customKeyBindingsSerializedData.inputActionBindings.Select(
                a => new InputActionBinding
                {
                    inputAction = RedirectLoad(a.inputAction),
                    strictCombination = false,
                    keysCombination = a
                        .keyCombinations.Select<string, InputKey>(Enum.Parse<InputKey>)
                        .ToList(),
                }
            );
            __instance._keyBindingsDataModel.UpdateWith(enumerable);
            return false;
        }

        [AffinityPatch(typeof(Enum), "GetName")]
        [AffinityPrefix]
        private bool Prefix(ref string __result, Type enumType, object value)
        {
            if (enumType != typeof(InputAction))
            {
                return true;
            }
            __result = RedirectSave(enumType, (InputAction)value);
            return false;
        }

        [AffinityPatch(
            typeof(KeyBindingGroupExtensions),
            "DisplayName",
            AffinityMethodType.Normal,
            null,
            typeof(KeyBindingGroupType)
        )]
        [AffinityPrefix]
        private bool Prefix(ref string __result, KeyBindingGroupType type)
        {
            if ((int)type > 999)
            {
                __result =
                    _customInputActionRegistry
                        .GetGroups()
                        .FirstOrDefault(x => x.GetKeyBindingGroupType().Equals(type))
                        ?.Name
                    ?? "Unknown";
                return false;
            }
            return true;
        }

        [AffinityPatch(
            typeof(KeyBindingGroupExtensions),
            "DisplayName",
            AffinityMethodType.Normal,
            null,
            typeof(InputAction)
        )]
        [AffinityPrefix]
        private bool Prefix(ref string __result, InputAction inputAction)
        {
            if ((int)inputAction > 999)
            {
                __result =
                    _customInputActionRegistry
                        .GetGroups()
                        .SelectMany(x => x.GetKeybindings())
                        .FirstOrDefault(x => x.GetInputAction().Equals(inputAction))
                        ?.Name
                    ?? "Unknown";
                return false;
            }
            return true;
        }
    }
}
