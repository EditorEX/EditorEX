using System;
using System.IO;
using System.Linq;
using BeatmapEditor3D.InputSystem;
using BeatmapEditor3D.InputSystem.SerializedData;
using BeatmapEditor3D.SerializedData;
using EditorEX.SDK.Input;
using Newtonsoft.Json;
using SiraUtil.Affinity;

namespace EditorEX.SDK.Integration.Patches
{
    /// <summary>
    /// Display-name and save/load redirects for custom keybindings.
    /// Static <see cref="_instance"/> bridges Harmony static call sites (e.g. <see cref="LoadCustomKeybindsPatch"/>).
    /// </summary>
    public class CustomKeybindingPersistencePatches : IAffinity
    {
        // Harmony static prefixes and LoadCustomKeybindsPatch need registry access without DI.
        internal static CustomKeybindingPersistencePatches _instance = null!;
        internal CustomInputActionRegistry _customInputActionRegistry = null!;

        private CustomKeybindingPersistencePatches(
            CustomInputActionRegistry customInputActionRegistry
        )
        {
            _customInputActionRegistry = customInputActionRegistry;
            _instance = this;
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
