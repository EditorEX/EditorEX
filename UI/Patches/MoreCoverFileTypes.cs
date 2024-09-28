using BeatmapEditor3D.Views;
using EditorEX.Essentials.Patches.Movement;
using HarmonyLib;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.UI.Patches
{
    [AffinityPatch]
    internal class MoreCoverFileTypes : IAffinity
    {
        private readonly static string[] fileTypes = new string[] { "png", "jpg", "jpeg" };

        private static readonly FieldInfo _fileTypesGetter = AccessTools.Field(typeof(MoreCoverFileTypes), "fileTypes");

        [AffinityPatch(typeof(CoverImageInputView), nameof(CoverImageInputView.HandleOpenFileButtonClicked))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null)
                .MatchForward(false, new CodeMatch(OpCodes.Newarr, typeof(System.String)))
                .Advance(-1)
                .RemoveInstructions(6)
                .Insert(new CodeInstruction(OpCodes.Ldsfld, _fileTypesGetter))
                .InstructionEnumeration();
            return result;
        }
    }
}
