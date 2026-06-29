using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapEditor3D.Views;
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

        // Matches both Component.get_transform and GameObject.get_transform.
        private static bool IsGetTransform(CodeInstruction instruction) =>
            (instruction.opcode == OpCodes.Callvirt || instruction.opcode == OpCodes.Call)
            && instruction.operand is MethodInfo method
            && method.Name == "get_transform";

        // Removes one `<view>.transform.localPosition = <recomputed>;` read-modify-write block from an
        // UpdateObjects loop body. The block is always laid out as:
        //     <load view>                            (viewLoadLength instructions)
        //     callvirt Component.get_transform
        //     callvirt Transform.get_localPosition   <-- anchor we match on
        //     stloc    <localPosition>
        //     ...      localPosition.z = helper.BeatToPosition(beat) ...
        //     <load view>
        //     callvirt Component.get_transform
        //     ldloc    <localPosition>
        //     callvirt Transform.set_localPosition   <-- end of block
        // so the block begins (1 + viewLoadLength) instructions before get_localPosition (the
        // get_transform plus the view load) and ends at the set_localPosition call. Removing the whole
        // block is stack-neutral and also drops the BeatToPosition call that used to run for every
        // object every frame.
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

        // Removes the one-shot `<view>.transform.localPosition = new Vector3(...);` placement statement
        // from an InsertObject method. Each InsertObject contains exactly one Transform.set_localPosition
        // (the placement). The statement is laid out as:
        //     <load view>                            (a single ldloc/ldloc.s)
        //     callvirt get_transform                 <-- nearest get_transform walking back from the write
        //     ...build the Vector3...
        //     newobj   Vector3..ctor
        //     callvirt Transform.set_localPosition   <-- end of statement
        // so we anchor on the (unique) set_localPosition, walk back to the get_transform that feeds it,
        // and remove from the view load (one instruction before that get_transform) through the write.
        // Any later transform usage in the method (e.g. SetParent) keeps its own view load and is left
        // untouched.
        private static void StripLocalPositionInsert(CodeMatcher matcher, string label)
        {
            matcher
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _setLocalPosition))
                .ThrowIfInvalid($"{label}: localPosition write not found");
            int blockEnd = matcher.Pos;

            matcher
                .MatchBack(false, new CodeMatch(IsGetTransform))
                .ThrowIfInvalid($"{label}: transform load not found");
            int blockStart = matcher.Pos - 1; // the ldloc loading the view, just before `.transform`

            matcher.RemoveInstructionsInRange(blockStart, blockEnd);
        }

        // Was: .Advance(17).RemoveInstructions(20).Advance(40).RemoveInstructions(20)
        //   -> blind-skip 17 to the note-loop localPosition block, remove 20; skip 40 more to the
        //      bomb-loop block, remove 20. Those raw counts were off by one (they clipped the first
        //      instruction of the following UpdateState call) and broke on any IL shift.
        // Now: match and remove the note-loop and bomb-loop localPosition blocks exactly. Both loops
        //      read the view from a KeyValuePair (`ldloca.s kvp; call get_Value`), so viewLoadLength is 2.
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

        // Was: .Advance(155).RemoveInstructions(20)
        //   -> blind-skip 155 to the initial-placement `gameObject.transform.localPosition = new
        //      Vector3(...)` statement near the end of InsertObject and remove 20.
        // Now: anchor on the placement write and remove the whole statement (see StripLocalPositionInsert).
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
    }
}
