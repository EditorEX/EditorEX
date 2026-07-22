using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapEditor3D.InputSystem;
using EditorEX.SDK.Input;
using HarmonyLib;
using SiraUtil.Affinity;
using SiraUtil.Logging;

namespace EditorEX.SDK.Integration.Patches
{
    /// <summary>
    /// Injects custom binding groups into <see cref="KeyBindings.GetDefault"/>.
    /// Static <see cref="_instance"/> exists because the IL-injected <see cref="Redirect"/> call is static.
    /// </summary>
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

        // Harmony IL calls Redirect statically; keep a singleton bridge for that entry point.
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
                                    strictCombination = a.Strict,
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
    }
}
