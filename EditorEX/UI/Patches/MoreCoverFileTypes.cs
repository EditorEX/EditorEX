using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapEditor3D.Views;
using HarmonyLib;
using SiraUtil.Affinity;

namespace EditorEX.UI.Patches
{
    internal class MoreCoverFileTypes : IAffinity
    {
        private static readonly string[] fileTypes = ["png", "jpg", "jpeg"];

        private static readonly FieldInfo _fileTypesGetter = AccessTools.Field(
            typeof(MoreCoverFileTypes),
            "fileTypes"
        );

        [AffinityPatch(
            typeof(CoverImageInputView),
            nameof(CoverImageInputView.HandleOpenFileButtonClicked)
        )]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Newarr, typeof(string)))
                .Advance(-1)
                .RemoveInstructions(6)
                .Insert(new CodeInstruction(OpCodes.Ldsfld, _fileTypesGetter))
                .InstructionEnumeration();
            return result;
        }
    }
}
