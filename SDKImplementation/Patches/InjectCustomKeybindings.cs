using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapEditor3D.InputSystem;
using EditorEX.SDK.Input;
using HarmonyLib;
using SiraUtil.Affinity;
using SiraUtil.Logging;

namespace EditorEX.SDKImplementation.Patches
{
    public class InjectCustomKeybindings : IAffinity
    {
        private static InjectCustomKeybindings _instance;
        private static readonly FieldInfo _extendedBindingGroups = AccessTools.Field(
            typeof(KeyBindings),
            "extendedBindingGroups"
        );
        private static readonly MethodInfo _redirect = AccessTools.Method(
            typeof(InjectCustomKeybindings),
            "Redirect"
        );

        private SiraLog _siraLog;
        private CustomInputActionRegistry _customInputActionRegistry;

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
                        ?.Name ?? "Unknown";
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
                        ?.Name ?? "Unknown";
                return false;
            }
            return true;
        }
    }
}
