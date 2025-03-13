using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Views;
using EditorEX.Heck.Deserialize;
using EditorEX.Heck.ObjectData;
using EditorEX.MapData.Contexts;
using HarmonyLib;
using Heck.Animation;
using SiraUtil.Affinity;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

// Heavily based on https://github.com/Aeroluna/Heck/blob/master/Heck/HarmonyPatches/GameObjectTracker.cs
namespace EditorEX.Heck.Patches
{
    public class EditorGameObjectTracker : IAffinity
    {
        private static readonly MethodInfo _addObject = AccessTools.Method(typeof(EditorGameObjectTracker), "AddObject");
        private static readonly MethodInfo _removeObject = AccessTools.Method(typeof(EditorGameObjectTracker), "RemoveObject");
        private static readonly MethodInfo _removeNote = AccessTools.Method(typeof(EditorGameObjectTracker), "RemoveNote");

        private static void RemoveNote(NoteBeatmapObjectView self, NoteEditorData? noteData)
        {
            if (noteData == null)
                return;
            if (self._noteObjects.TryGetValue(noteData.id, out var normalNoteView))
            {
                RemoveObject(noteData, normalNoteView);
            }
            if (self._bombObjects.TryGetValue(noteData.id, out var bombNoteView))
            {
                RemoveObject(noteData, bombNoteView);
            }
        }

        private static void AddObject(BaseEditorData? editorData, Component obj)
        {
            if (MapContext.Version.Major > 3)
            {
                return;
            }

            if (!TryGetTrack(editorData, out List<Track> track))
            {
                return;
            }

            track.ForEach(n => n.AddGameObject(obj.gameObject));
        }

        private static void RemoveObject(BaseEditorData? editorData, Component obj)
        {
            if (MapContext.Version.Major > 3)
            {
                return;
            }

            if (!TryGetTrack(editorData, out List<Track> track))
            {
                return;
            }

            track.ForEach(n => n.RemoveGameObject(obj.gameObject));
        }

        private static bool TryGetTrack(BaseEditorData? objectData, out List<Track> track)
        {
            if (!EditorDeserializedDataContainer.GetDeserializedData("Heck").Resolve(objectData, out EditorHeckObjectData? heckData) || heckData?.Track == null)
            {
                track = null;
                return false;
            }

            track = heckData.Track;
            return true;
        }


        [AffinityPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.InsertObject))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerNote(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .End() // before ret
                .Insert(new(OpCodes.Ldarg_1), new(OpCodes.Ldloc_S, 6), new(OpCodes.Call, _addObject)).InstructionEnumeration();
            return result;
        }

        [AffinityPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.DeleteObject))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerRemoveNote(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .End() // before ret
                .Insert(new(OpCodes.Ldarg_0), new(OpCodes.Ldarg_1), new(OpCodes.Call, _removeNote)).InstructionEnumeration();
            return result;
        }

        [AffinityPatch(typeof(ObstacleBeatmapObjectView), nameof(ObstacleBeatmapObjectView.InsertObject))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerObstacle(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .End() // before ret
                .Insert(new(OpCodes.Ldarg_1), new(OpCodes.Ldloc_2), new(OpCodes.Call, _addObject)).InstructionEnumeration();
            return result;
        }

        [AffinityPatch(typeof(ObstacleBeatmapObjectView), nameof(ObstacleBeatmapObjectView.DeleteObject))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerRemoveObstacle(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .End() // before ret
                .Insert(new(OpCodes.Ldarg_1), new(OpCodes.Ldloc_2), new(OpCodes.Call, _removeObject)).InstructionEnumeration();
            return result;
        }

        [AffinityPatch(typeof(ChainBeatmapObjectsView), nameof(ChainBeatmapObjectsView.InsertObject))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerChain(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .End() // before ret
                .Insert(new(OpCodes.Ldarg_1), new(OpCodes.Ldloc_0), new(OpCodes.Call, _addObject)).InstructionEnumeration();
            return result;
        }

        [AffinityPatch(typeof(ChainBeatmapObjectsView), nameof(ChainBeatmapObjectsView.DeleteObject))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerRemoveChain(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .End()// before ret
                .Insert(new(OpCodes.Ldarg_1), new(OpCodes.Ldloc_0), new(OpCodes.Call, _removeObject)).InstructionEnumeration();
            return result;
        }

        [AffinityPatch(typeof(ArcBeatmapObjectsView), nameof(ArcBeatmapObjectsView.InsertObject))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerArc(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .End().Advance(-1) // before ret
                .Insert(new(OpCodes.Ldarg_1), new(OpCodes.Ldloc_0), new(OpCodes.Call, _addObject)).InstructionEnumeration();
            return result;
        }

        [AffinityPatch(typeof(ArcBeatmapObjectsView), nameof(ArcBeatmapObjectsView.DeleteObject))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> TranspilerRemoveArc(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .End().Advance(-1) // before ret
                .Insert(new(OpCodes.Ldarg_1), new(OpCodes.Ldloc_2), new(OpCodes.Call, _removeObject)).InstructionEnumeration();
            return result;
        }
    }
}
