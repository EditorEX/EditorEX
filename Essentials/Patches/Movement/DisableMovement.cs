using BeatmapEditor3D.Views;
using BeatmapEditor3D.Visuals;
using HarmonyLib;
using SiraUtil.Affinity;
using System.Collections.Generic;

namespace EditorEX.Essentials.Patches.Movement
{
    //TODO: Make these safer lol.
    [AffinityPatch]
    public class DisableMovement : IAffinity
    {
        [AffinityPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.UpdateObjects))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerNote(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null).Advance(17).RemoveInstructions(20).Advance(40).RemoveInstructions(20).InstructionEnumeration();
            return result;
        }

        [AffinityPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.InsertObject))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerNoteInsert(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null).Advance(155).RemoveInstructions(20).InstructionEnumeration();
            return result;
        }

        [AffinityPatch(typeof(ArcBeatmapObjectsView), nameof(ArcBeatmapObjectsView.UpdateObjects))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerArc(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null).Advance(12).RemoveInstructions(16).InstructionEnumeration();
            return result;
        }

        [AffinityPatch(typeof(ObstacleBeatmapObjectView), nameof(ObstacleBeatmapObjectView.UpdateObjects))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerObstacle(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null).Advance(9).RemoveInstructions(19).InstructionEnumeration();
            return result;
        }
    }
}
