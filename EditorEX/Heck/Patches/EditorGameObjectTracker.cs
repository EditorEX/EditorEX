using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Views;
using EditorEX.Heck.Deserialize;
using EditorEX.Heck.ObjectData;
using EditorEX.MapData.Contexts;
using HarmonyLib;
using Heck.Animation;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

// Heavily based on https://github.com/Aeroluna/Heck/blob/master/Heck/HarmonyPatches/GameObjectTracker.cs
namespace EditorEX.Heck.Patches
{
    public class EditorGameObjectTracker : IAffinity
    {
        private static EditorDeserializedData? _heckCache;
        private static readonly Dictionary<GameObject, List<Track>> _tracked = new();

        private static readonly MethodInfo _addObject = AccessTools.Method(
            typeof(EditorGameObjectTracker),
            nameof(AddObject)
        );
        private static readonly MethodInfo _removeObject = AccessTools.Method(
            typeof(EditorGameObjectTracker),
            nameof(RemoveObject)
        );

        private EditorGameObjectTracker([Inject(Id = "Heck")] EditorDeserializedData heckCache)
        {
            _heckCache = heckCache;
        }

        private static void AddObject(BaseEditorData? editorData, Component obj)
        {
            if (MapContext.Version.Major > 3 || obj == null)
            {
                return;
            }

            if (!TryGetTrack(editorData, out List<Track> track))
            {
                return;
            }

            GameObject gameObject = obj.gameObject;
            _tracked[gameObject] = track;
            track.ForEach(n => n.AddGameObject(gameObject));
        }

        // Views are Zenject-pooled. Untrack by GameObject so DeleteObject and ClearObjects can
        // both drop the instance before Despawn, even when editor data is already gone.
        private static void RemoveObject(Component obj)
        {
            if (MapContext.Version.Major > 3 || obj == null)
            {
                return;
            }

            GameObject gameObject = obj.gameObject;
            if (!_tracked.TryGetValue(gameObject, out List<Track> track))
            {
                return;
            }

            _tracked.Remove(gameObject);
            track.ForEach(n => n.RemoveGameObject(gameObject));
        }

        private static bool TryGetTrack(BaseEditorData? objectData, out List<Track> track)
        {
            if (
                _heckCache == null
                || !_heckCache.Resolve(objectData, out EditorHeckObjectData? heckData)
                || heckData?.Track == null
            )
            {
                track = null;
                return false;
            }

            track = heckData.Track;
            return true;
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.InsertObject))]
        [AffinityPatch(
            typeof(ObstacleBeatmapObjectView),
            nameof(ObstacleBeatmapObjectView.InsertObject)
        )]
        [AffinityPatch(
            typeof(ChainBeatmapObjectsView),
            nameof(ChainBeatmapObjectsView.InsertObject)
        )]
        [AffinityPatch(typeof(ArcBeatmapObjectsView), nameof(ArcBeatmapObjectsView.InsertObject))]
        private IEnumerable<CodeInstruction> TranspileInsert(
            IEnumerable<CodeInstruction> instructions
        )
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(IsViewDictionaryAdd))
                .Repeat(InsertAddObject)
                .InstructionEnumeration();
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.DeleteObject))]
        [AffinityPatch(typeof(NoteBeatmapObjectView), nameof(NoteBeatmapObjectView.ClearObjects))]
        [AffinityPatch(
            typeof(ObstacleBeatmapObjectView),
            nameof(ObstacleBeatmapObjectView.DeleteObject)
        )]
        [AffinityPatch(
            typeof(ObstacleBeatmapObjectView),
            nameof(ObstacleBeatmapObjectView.ClearObjects)
        )]
        [AffinityPatch(
            typeof(ChainBeatmapObjectsView),
            nameof(ChainBeatmapObjectsView.DeleteObject)
        )]
        [AffinityPatch(
            typeof(ChainBeatmapObjectsView),
            nameof(ChainBeatmapObjectsView.ClearObjects)
        )]
        [AffinityPatch(typeof(ArcBeatmapObjectsView), nameof(ArcBeatmapObjectsView.DeleteObject))]
        [AffinityPatch(typeof(ArcBeatmapObjectsView), nameof(ArcBeatmapObjectsView.ClearObjects))]
        private IEnumerable<CodeInstruction> TranspileRemove(
            IEnumerable<CodeInstruction> instructions
        )
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(IsPoolDespawn))
                .Repeat(InsertRemoveObject)
                .InstructionEnumeration();
        }

        private static void InsertAddObject(CodeMatcher matcher)
        {
            CodeInstruction viewLoad = matcher.InstructionAt(-1);
            matcher
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(viewLoad.opcode, viewLoad.operand),
                    new CodeInstruction(OpCodes.Call, _addObject)
                )
                .Advance(4);
        }

        private static void InsertRemoveObject(CodeMatcher matcher)
        {
            matcher
                .Insert(
                    new CodeInstruction(OpCodes.Dup),
                    new CodeInstruction(OpCodes.Call, _removeObject)
                )
                .Advance(3);
        }

        private static bool IsPoolDespawn(CodeInstruction instruction) =>
            instruction.opcode == OpCodes.Callvirt
            && instruction.operand is MethodInfo { Name: "Despawn" };

        private static bool IsViewDictionaryAdd(CodeInstruction instruction) =>
            instruction.opcode == OpCodes.Callvirt
            && instruction.operand is MethodInfo { Name: "Add" } method
            && method.DeclaringType is { IsGenericType: true } type
            && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }
}
