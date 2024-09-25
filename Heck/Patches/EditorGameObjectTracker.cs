using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Views;
using EditorEX.Heck.Deserialize;
using EditorEX.Heck.ObjectData;
using HarmonyLib;
using Heck.Animation;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

// Heavily based on https://github.com/Aeroluna/Heck/blob/master/Heck/HarmonyPatches/GameObjectTracker.cs
namespace EditorEX.Heck.Patches
{
    [HarmonyPatch]
    public static class EditorGameObjectTracker
    {
        private static readonly MethodInfo _addObject = AccessTools.Method(typeof(EditorGameObjectTracker), "AddObject", null, null);
        private static readonly MethodInfo _removeObject = AccessTools.Method(typeof(EditorGameObjectTracker), "RemoveObject", null, null);
        private static readonly MethodInfo _removeNote = AccessTools.Method(typeof(EditorGameObjectTracker), "RemoveNote", null, null);

        [HarmonyPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.InsertObject))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspilerNote(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null)
                .End().Advance(-1) // before ret
                .Insert(new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloc_S, 5),
                    new CodeInstruction(OpCodes.Call, _addObject)
                }).InstructionEnumeration();
            return result;
        }

        private static void RemoveNote(NoteBeatmapObjectView self, NoteEditorData noteData)
        {
            if (self._noteObjects.TryGetValue(noteData.id, out var normalNoteView))
            {
                RemoveObject(noteData, normalNoteView);
            }
            if (self._bombObjects.TryGetValue(noteData.id, out var bombNoteView))
            {
                RemoveObject(noteData, bombNoteView);
            }
        }

        [HarmonyPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.DeleteObject))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspilerRemoveNote(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null)
                .End().Advance(-1) // before ret
                .Insert(new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _removeNote)
                }).InstructionEnumeration();
            return result;
        }

        [HarmonyPatch(typeof(ObstacleBeatmapObjectView), nameof(ObstacleBeatmapObjectView.InsertObject))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspilerObstacle(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null)
                .End().Advance(-1) // before ret
                .Insert(new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Call, _addObject)
                }).InstructionEnumeration();
            return result;
        }

        [HarmonyPatch(typeof(ObstacleBeatmapObjectView), nameof(ObstacleBeatmapObjectView.DeleteObject))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspilerRemoveObstacle(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null)
                .End().Advance(-1) // before ret
                .Insert(new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, _removeObject)
                }).InstructionEnumeration();
            return result;
        }

        [HarmonyPatch(typeof(ChainBeatmapObjectsView), nameof(ChainBeatmapObjectsView.InsertObject))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspilerChain(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null)
                .End().Advance(-1) // before ret
                .Insert(new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, _addObject)
                }).InstructionEnumeration();
            return result;
        }

        [HarmonyPatch(typeof(ObstacleBeatmapObjectView), nameof(ChainBeatmapObjectsView.DeleteObject))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspilerRemoveChain(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null)
                .End().Advance(-1) // before ret
                .Insert(new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, _removeObject)
                }).InstructionEnumeration();
            return result;
        }

        [HarmonyPatch(typeof(ArcBeatmapObjectsView), nameof(ArcBeatmapObjectsView.InsertObject))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspilerArc(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null)
                .End().Advance(-1) // before ret
                .Insert(new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, _addObject)
                }).InstructionEnumeration();
            return result;
        }

        [HarmonyPatch(typeof(ObstacleBeatmapObjectView), nameof(ArcBeatmapObjectsView.DeleteObject))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspilerRemoveArc(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null)
                .End().Advance(-1) // before ret
                .Insert(new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, _removeObject)
                }).InstructionEnumeration();
            return result;
        }

        private static void AddObject(BaseEditorData editorData, GameObject obj)
        {
            if (!TryGetTrack(editorData, out List<Track> track))
            {
                return;
            }

            //Plugin.Log.Info($"Added {obj.name} to {track.Count} track(s)");

            track.ForEach(n => n.AddGameObject(obj));
        }

        private static void RemoveObject(BaseEditorData editorData, Component obj)
        {
            if (!TryGetTrack(editorData, out List<Track> track))
            {
                return;
            }

            track.ForEach(n => n.RemoveGameObject(obj.gameObject));
        }

        private static bool TryGetTrack(BaseEditorData objectData, out List<Track> track)
        {
            if (!EditorDeserializedDataContainer.GetDeserializedData("Heck").Resolve(objectData, out EditorHeckObjectData heckData) || heckData.Track == null)
            {
                track = null;
                return false;
            }

            track = heckData.Track;
            return true;
        }
    }
}
