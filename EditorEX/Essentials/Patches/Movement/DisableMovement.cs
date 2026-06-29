using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapEditor3D.Views;
using BeatmapEditor3D.Visuals;
using HarmonyLib;
using SiraUtil.Affinity;
using UnityEngine;

namespace EditorEX.Essentials.Patches.Movement
{
    // EditorEX installs its own movement components (EditorNoteController, EditorObstacleController,
    // EditorArcController, EditorChainHeadController, ...) that drive each object's transform every
    // frame. The base game's BeatmapObjectView methods also write `view.transform.localPosition`, so
    // if those stock writes are left in place the two systems fight over the transform. These
    // transpilers remove the stock localPosition writes so EditorEX movement is the sole owner of the
    // transform:
    //   * UpdateObjects (note/bomb, arc, obstacle): remove the per-frame read-modify-write block,
    //     which also drops the BeatToPosition recompute that only fed it.
    //   * InsertObject (note/bomb, obstacle, arc, chain): remove the one-shot initial placement write.
    [AffinityPatch]
    public class DisableMovement : IAffinity
    {
        private static readonly MethodInfo _getLocalPosition = AccessTools.PropertyGetter(
            typeof(Transform),
            nameof(Transform.localPosition)
        );
        private static readonly MethodInfo _setLocalPosition = AccessTools.PropertySetter(
            typeof(Transform),
            nameof(Transform.localPosition)
        );
        private static readonly MethodInfo _setLocalRotation = AccessTools.PropertySetter(
            typeof(Transform),
            nameof(Transform.localRotation)
        );

        // Matches both Component.get_transform and GameObject.get_transform.
        private static bool IsGetTransform(CodeInstruction instruction) =>
            (
                (instruction.opcode == OpCodes.Callvirt || instruction.opcode == OpCodes.Call)
                && instruction.operand is MethodInfo method
                && method.Name == "get_transform"
            )
            || (
                instruction.opcode == OpCodes.Ldfld
                && instruction.operand is FieldInfo field
                && field.Name == "_transform"
            );

        private static void StripLocalPositionUpdate(
            CodeMatcher matcher,
            int viewLoadLength,
            string label
        )
        {
            matcher
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _getLocalPosition))
                .ThrowIfInvalid($"{label}: localPosition read not found");
            int blockStart = matcher.Pos - 1 - viewLoadLength;

            matcher
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _setLocalPosition))
                .ThrowIfInvalid($"{label}: localPosition write not found");
            int blockEnd = matcher.Pos;

            matcher.RemoveInstructionsInRange(blockStart, blockEnd);
        }

        private static void StripLocalPositionInsert(CodeMatcher matcher, string label)
        {
            matcher
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _setLocalPosition))
                .ThrowIfInvalid($"{label}: localPosition write not found");
            int blockEnd = matcher.Pos;

            matcher
                .MatchBack(false, new CodeMatch(IsGetTransform))
                .ThrowIfInvalid($"{label}: transform load not found");
            int blockStart = matcher.Pos - 1;

            matcher.RemoveInstructionsInRange(blockStart, blockEnd);
        }

        private static void StripLocalRotationInsert(CodeMatcher matcher, string label)
        {
            matcher
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _setLocalRotation))
                .ThrowIfInvalid($"{label}: localRotation write not found");
            int blockEnd = matcher.Pos;

            matcher
                .MatchBack(false, new CodeMatch(IsGetTransform))
                .ThrowIfInvalid($"{label}: transform load not found");
            int blockStart = matcher.Pos - 1;

            matcher.RemoveInstructionsInRange(blockStart, blockEnd);
        }

        [AffinityPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.UpdateObjects))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerNote(
            IEnumerable<CodeInstruction> instructions
        )
        {
            var matcher = new CodeMatcher(instructions);
            StripLocalPositionUpdate(matcher, 2, "NoteBeatmapObjectView.UpdateObjects (notes)");
            StripLocalPositionUpdate(matcher, 2, "NoteBeatmapObjectView.UpdateObjects (bombs)");
            return matcher.InstructionEnumeration();
        }

        [AffinityPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.InsertObject))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerNoteInsert(
            IEnumerable<CodeInstruction> instructions
        )
        {
            var matcher = new CodeMatcher(instructions);
            StripLocalPositionInsert(matcher, "NoteBeatmapObjectView.InsertObject");
            return matcher.InstructionEnumeration();
        }

        [AffinityPatch(typeof(ChainElementNoteView), nameof(ChainElementNoteView.Init))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerChainElementInit(
            IEnumerable<CodeInstruction> instructions
        )
        {
            var matcher = new CodeMatcher(instructions);
            StripLocalRotationInsert(matcher, "ChainElementNoteView.Init");
            StripLocalPositionInsert(matcher, "ChainElementNoteView.Init");
            return matcher.InstructionEnumeration();
        }

        // Was: .Advance(12).RemoveInstructions(16)
        //   -> blind-skip 12 to the localPosition block and remove 16.
        // Now: match and remove that block. The arc loop holds the view directly in a local
        //      (`ldloc.2`), so viewLoadLength is 1.
        [AffinityPatch(typeof(ArcBeatmapObjectsView), nameof(ArcBeatmapObjectsView.UpdateObjects))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerArc(
            IEnumerable<CodeInstruction> instructions
        )
        {
            var matcher = new CodeMatcher(instructions);
            StripLocalPositionUpdate(matcher, 1, "ArcBeatmapObjectsView.UpdateObjects");
            return matcher.InstructionEnumeration();
        }

        // New: strip the initial placement write from ArcBeatmapObjectsView.InsertObject, matching the
        // behaviour already applied to notes so EditorEX movement owns the arc transform from spawn.
        [AffinityPatch(typeof(ArcBeatmapObjectsView), nameof(ArcBeatmapObjectsView.InsertObject))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerArcInsert(
            IEnumerable<CodeInstruction> instructions
        )
        {
            var matcher = new CodeMatcher(instructions);
            StripLocalPositionInsert(matcher, "ArcBeatmapObjectsView.InsertObject");
            return matcher.InstructionEnumeration();
        }

        // Was: .Advance(9).RemoveInstructions(19)
        //   -> blind-skip 9 to the localPosition block and remove 19 (which stranded the view load and
        //      clipped into the following UpdateState call).
        // Now: match and remove that block. The obstacle loop reads the view from a KeyValuePair
        //      (`ldloca.s kvp; call get_Value`), so viewLoadLength is 2.
        [AffinityPatch(
            typeof(ObstacleBeatmapObjectView),
            nameof(ObstacleBeatmapObjectView.UpdateObjects)
        )]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerObstacle(
            IEnumerable<CodeInstruction> instructions
        )
        {
            var matcher = new CodeMatcher(instructions);
            StripLocalPositionUpdate(matcher, 2, "ObstacleBeatmapObjectView.UpdateObjects");
            return matcher.InstructionEnumeration();
        }

        // New: strip the initial placement write from ObstacleBeatmapObjectView.InsertObject, matching
        // the behaviour already applied to notes.
        [AffinityPatch(
            typeof(ObstacleBeatmapObjectView),
            nameof(ObstacleBeatmapObjectView.InsertObject)
        )]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerObstacleInsert(
            IEnumerable<CodeInstruction> instructions
        )
        {
            var matcher = new CodeMatcher(instructions);
            StripLocalPositionInsert(matcher, "ObstacleBeatmapObjectView.InsertObject");
            return matcher.InstructionEnumeration();
        }

        // New: strip the initial placement write from ChainBeatmapObjectsView.InsertObject, matching the
        // behaviour already applied to notes.
        [AffinityPatch(
            typeof(ChainBeatmapObjectsView),
            nameof(ChainBeatmapObjectsView.InsertObject)
        )]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerChainInsert(
            IEnumerable<CodeInstruction> instructions
        )
        {
            var matcher = new CodeMatcher(instructions);
            StripLocalPositionInsert(matcher, "ChainBeatmapObjectsView.InsertObject");
            return matcher.InstructionEnumeration();
        }

        // New: strip the per-frame localPosition block from ChainBeatmapObjectsView.UpdateObjects, the
        // same as the note/arc/obstacle loops. Without this the base game keeps writing the chain's z
        // as a flat scroll position (BeatToPosition) every frame, which fights EditorChainHeadGameMovement
        // and overwrites the head's jump z during the jump phase. The loop reads the view from a
        // KeyValuePair (`ldloca.s kvp; call get_Value`), so viewLoadLength is 2.
        [AffinityPatch(
            typeof(ChainBeatmapObjectsView),
            nameof(ChainBeatmapObjectsView.UpdateObjects)
        )]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerChain(
            IEnumerable<CodeInstruction> instructions
        )
        {
            var matcher = new CodeMatcher(instructions);
            StripLocalPositionUpdate(matcher, 2, "ChainBeatmapObjectsView.UpdateObjects");
            return matcher.InstructionEnumeration();
        }
    }
}
