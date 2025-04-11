using BeatmapEditor3D.InputSystem;
using BeatmapEditor3D.Views;
using HarmonyLib;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace EditorEX.UI.Patches
{
    internal class BetterKeybindViewingPatches : IAffinity
    {
        private static BetterKeybindViewingPatches _instance;
        private static readonly MethodInfo _redirect = AccessTools.Method(typeof(BetterKeybindViewingPatches), "Redirect");

        private SiraLog _siraLog;

        private BetterKeybindViewingPatches(SiraLog siraLog)
        {
            _instance = this;
            _siraLog = siraLog;
            _siraLog.Info("BetterKeybindViewingPatches initialized.");
        }

        Transform NewGroupTab(BindingGroup bindingGroup)
        {
            _siraLog.Info($"Creating new group tab for {bindingGroup.type.DisplayName()}");
            return null;
        }

        public static Transform Redirect(BindingGroup bindingGroup)
        {
            return _instance.NewGroupTab(bindingGroup);
        }

        [AffinityPatch(typeof(KeybindsView), nameof(KeybindsView.CreateBindingGroupUI))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            _siraLog.Info("Transpiling KeybindsView.CreateBindingGroupUI...");
            var result = new CodeMatcher(instructions)
                .Advance(2)
                .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld))
                .RemoveInstructions(2)
                .Insert(new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Call, _redirect))
                .InstructionEnumeration();

            StringBuilder sb = new StringBuilder();
            foreach (var instruction in result)
            {
                sb.AppendLine(instruction.ToString());
            }
            Debug.Log(sb.ToString());
            return result;
        }
    }
}
