using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatSaber.TrackDefinitions;
using HarmonyLib;
using IntervalTree;
using SiraUtil.Affinity;
using Unity.Profiling;
using UnityEngine;

namespace EditorEX.Essentials.Patches;

// Optimises BasicEventObjectsView.RefreshView and the methods it calls.
//
// 1. RefreshView's two nested LINQ set-differences (O(n*m)) are replaced with
//    HashSet diffing (O(n+m)), with the sets and result lists pooled across
//    frames so steady-state playback allocates nothing here.
// 2. The three EventMarkerSpawner.RemoveSpawnedEvents methods allocate a
//    Predicate<> delegate per marker via List.Find(marker.data.InstanceEquals).
//    A transpiler redirects that Find call to an allocation-free id lookup.
// 3. AddEventObjects allocates a comparer + a Comparison<> delegate every call;
//    both are now cached statics.
public class BasicEventObjectsViewRefreshPatch : IAffinity
{
    private sealed class TrackDiffState
    {
        public List<BasicEventEditorData> prevList;
        public HashSet<BasicEventEditorData> prevSet = new();
        public HashSet<BasicEventEditorData> spareSet = new();
    }

    private readonly Dictionary<BasicBeatmapEventType, TrackDiffState> _state = new();

    // Reused every track/frame. Callees (RemoveEventObjects, AddEventObjects)
    // consume these synchronously and never retain the reference.
    private readonly List<BasicEventEditorData> _addBuffer = new();
    private readonly List<BasicEventEditorData> _removeBuffer = new();

    private static readonly List<BasicEventEditorData> _emptyEvents = new();

    // Profiler markers. The Profiler's GC Alloc column attributes allocations to the
    // innermost open marker, so each region is reported separately. View under
    // "EditorEX.BasicEventObjectsView" in the Profiler Hierarchy / Timeline.
    private static readonly ProfilerMarker _markerRefresh = new(
        "EditorEX.BasicEventObjectsView.RefreshView"
    );
    private static readonly ProfilerMarker _markerQuery = new("EditorEX.GetBasicEventsInterval");
    private static readonly ProfilerMarker _markerDiff = new("EditorEX.Diff");
    private static readonly ProfilerMarker _markerRemove = new("EditorEX.RemoveEventObjects");
    private static readonly ProfilerMarker _markerAdd = new("EditorEX.AddEventObjects");
    private static readonly ProfilerMarker _markerUpdate = new("EditorEX.UpdateEventObjects");

    [AffinityPrefix]
    [AffinityPatch(typeof(BasicEventObjectsView), "RefreshView")]
    private bool RefreshView(BasicEventObjectsView __instance)
    {
        _markerRefresh.Begin();

        float from = __instance._beatmapState.beat - 5f;
        float to = __instance._beatmapState.beat + 16f;

        var page = __instance._basicEventsState.currentEventsPage;
        var tracks = __instance._beatmapDataModel.environmentTrackDefinitionModel[page];
        int count = tracks.Count;

        for (int t = 0; t < tracks.Count; t++)
        {
            var info = tracks[t];
            var type = info.basicBeatmapEventType;

            _markerQuery.Begin();
            var currentEvents = __instance._beatmapBasicEventsDataModel.GetBasicEventsInterval(
                type,
                from,
                to
            );
            _markerQuery.End();

            var prevEvents = __instance._prevEvents.TryGetValue(type, out var pv)
                ? pv
                : _emptyEvents;

            _markerDiff.Begin();

            if (!_state.TryGetValue(type, out var st))
            {
                st = new TrackDiffState();
                _state[type] = st;
            }

            // Ensure prevSet reflects the game's current "previous" list. In steady
            // state prevEvents is the exact list we stored last frame, so its set is
            // already built and this rebuild is skipped. The count guard catches any
            // in-place mutation; the reference check catches external replacement.
            if (!ReferenceEquals(st.prevList, prevEvents) || st.prevSet.Count != prevEvents.Count)
            {
                st.prevSet.Clear();
                for (int e = 0; e < prevEvents.Count; e++)
                    st.prevSet.Add(prevEvents[e]);
                st.prevList = prevEvents;
            }
            var prevSet = st.prevSet;

            // Build the current set into the recycled spare set.
            var currentSet = st.spareSet;
            currentSet.Clear();
            for (int e = 0; e < currentEvents.Count; e++)
                currentSet.Add(currentEvents[e]);

            // currentEvents not in prevEvents -> O(n)
            _addBuffer.Clear();
            for (int e = 0; e < currentEvents.Count; e++)
            {
                var evt = currentEvents[e];
                if (!prevSet.Contains(evt))
                    _addBuffer.Add(evt);
            }

            // prevEvents not in currentEvents -> O(m)
            _removeBuffer.Clear();
            for (int e = 0; e < prevEvents.Count; e++)
            {
                var evt = prevEvents[e];
                if (!currentSet.Contains(evt))
                    _removeBuffer.Add(evt);
            }

            _markerDiff.End();

            _markerRemove.Begin();
            __instance.RemoveEventObjects(type, _removeBuffer);
            _markerRemove.End();

            _markerAdd.Begin();
            __instance.AddEventObjects(info, t, count, _addBuffer);
            _markerAdd.End();

            __instance._prevEvents[type] = currentEvents;

            // Swap: this frame's currentSet becomes next frame's prevSet; the old
            // prevSet is recycled as the spare. No HashSet allocation after warmup.
            st.spareSet = st.prevSet;
            st.prevSet = currentSet;
            st.prevList = currentEvents;
        }

        _markerUpdate.Begin();
        __instance.UpdateEventObjects();
        _markerUpdate.End();

        _markerRefresh.End();
        return false; // skip the original
    }

    // ---- AddEventObjects: cache the comparer and its Comparison<> delegate ----

    private static readonly EventEditorDataComparer<BasicEventEditorData> _comparer = new();
    private static readonly Comparison<BasicEventEditorData> _comparison = _comparer.Compare;

    [AffinityPrefix]
    [AffinityPatch(typeof(BasicEventObjectsView), "AddEventObjects")]
    private bool AddEventObjects(
        BasicEventObjectsView __instance,
        EnvironmentTracksDefinitionSO.BasicEventTrackInfo basicEventTrack,
        int i,
        int trackCount,
        List<BasicEventEditorData> eventsToAdd
    )
    {
        if (eventsToAdd == null || eventsToAdd.Count == 0)
            return false;

        var markerSpawner = __instance._eventMarkerSpawnerProvider.GetMarkerSpawner(
            basicEventTrack.trackDefinition.markerType
        );
        if (markerSpawner == null)
        {
            Debug.Log($"Null MarkerSpawner for {basicEventTrack.trackDefinition.markerType}");
            return false;
        }

        float xPos = TrackPlacementHelper.TrackToPosition(i, trackCount);
        eventsToAdd.Sort(_comparison);
        for (int e = 0; e < eventsToAdd.Count; e++)
            markerSpawner.SpawnAt(eventsToAdd[e], xPos, __instance._beatmapState.beat);

        return false;
    }

    // ---- GetBasicEventsInterval: traverse the interval tree without per-node lists ----

    // The stock IntervalTreeNode.Query(from, to) allocates a `new List` at EVERY node
    // it visits and AddRange-copies child results upward, so one query allocates
    // O(visited nodes) lists. This reimplementation appends into a single result list.
    // Behaviour is identical: same items, same order, same comparer.
    [AffinityPrefix]
    [AffinityPatch(
        typeof(BeatmapBasicEventsDataModel),
        nameof(BeatmapBasicEventsDataModel.GetBasicEventsInterval)
    )]
    private bool GetBasicEventsInterval(
        ref List<BasicEventEditorData> __result,
        BeatmapBasicEventsDataModel __instance,
        BasicBeatmapEventType eventType,
        float from,
        float to
    )
    {
        var result = new List<BasicEventEditorData>();
        if (__instance._eventsTreeByType.TryGetValue(eventType, out var tree))
        {
            var treeInstance = tree as IntervalTree<float, BasicEventEditorData>;
            if (!treeInstance.isInSync)
                treeInstance.Rebuild();
            if (treeInstance.root != null)
                QueryInto(treeInstance.root, from, to, result);
        }
        __result = result;
        return false;
    }

    private static void QueryInto(
        IntervalTreeNode<float, BasicEventEditorData> node,
        float from,
        float to,
        List<BasicEventEditorData> acc
    )
    {
        var comparer = node.comparer;

        var items = node.items;
        if (items != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                var rv = items[i];
                if (comparer.Compare(rv.From, to) > 0)
                    break;
                if (comparer.Compare(to, rv.From) >= 0 && comparer.Compare(from, rv.To) <= 0)
                    acc.Add(rv.Value);
            }
        }

        if (node.leftNode != null && comparer.Compare(from, node.center) < 0)
            QueryInto(node.leftNode, from, to, acc);
        if (node.rightNode != null && comparer.Compare(to, node.center) > 0)
            QueryInto(node.rightNode, from, to, acc);
    }

    // ---- RemoveSpawnedEvents: drop the per-marker Predicate<> allocation ----

    // Replaces `list.Find(data.InstanceEquals)` (delegate-per-marker) with an
    // id-based lookup. InstanceEquals is just `id == other.id`, so this is exact.
    public static BasicEventEditorData FindMatch(
        List<BasicEventEditorData> list,
        BaseEditorData data
    )
    {
        var id = data.id;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].id == id)
                return list[i];
        }
        return null;
    }

    private static readonly MethodInfo _findMatch = AccessTools.Method(
        typeof(BasicEventObjectsViewRefreshPatch),
        nameof(FindMatch)
    );

    [AffinityTranspiler]
    [AffinityPatch(
        typeof(BasicEventMarkerSpawner),
        nameof(BasicEventMarkerSpawner.RemoveSpawnedEvents)
    )]
    private IEnumerable<CodeInstruction> BasicRemoveTranspiler(
        IEnumerable<CodeInstruction> instructions
    ) => RedirectFind(instructions);

    [AffinityTranspiler]
    [AffinityPatch(
        typeof(LightEventMarkerSpawner),
        nameof(LightEventMarkerSpawner.RemoveSpawnedEvents)
    )]
    private IEnumerable<CodeInstruction> LightRemoveTranspiler(
        IEnumerable<CodeInstruction> instructions
    ) => RedirectFind(instructions);

    [AffinityTranspiler]
    [AffinityPatch(
        typeof(DurationEventMarkerSpawner),
        nameof(DurationEventMarkerSpawner.RemoveSpawnedEvents)
    )]
    private IEnumerable<CodeInstruction> DurationRemoveTranspiler(
        IEnumerable<CodeInstruction> instructions
    ) => RedirectFind(instructions);

    // Rewrites each `[ldftn InstanceEquals; newobj Predicate<>; callvirt List.Find]`
    // into a single `call FindMatch`. The object pushed before ldftn (the marker's
    // data) is left on the stack and becomes FindMatch's second argument.
    private static IEnumerable<CodeInstruction> RedirectFind(
        IEnumerable<CodeInstruction> instructions
    )
    {
        var codes = new List<CodeInstruction>(instructions);
        int replaced = 0;

        for (int i = 2; i < codes.Count; i++)
        {
            if (
                (codes[i].opcode == OpCodes.Callvirt || codes[i].opcode == OpCodes.Call)
                && codes[i].operand is MethodInfo find
                && find.Name == "Find"
                && codes[i - 1].opcode == OpCodes.Newobj
                && codes[i - 2].opcode == OpCodes.Ldftn
                && codes[i - 2].operand is MethodInfo pred
                && pred.Name == "InstanceEquals"
            )
            {
                var call = new CodeInstruction(OpCodes.Call, _findMatch);
                // Preserve any labels that sat on the instructions we're collapsing.
                call.labels.AddRange(codes[i - 2].labels);
                call.labels.AddRange(codes[i - 1].labels);
                call.labels.AddRange(codes[i].labels);

                codes[i - 2] = call;
                codes.RemoveRange(i - 1, 2);
                i -= 2;
                replaced++;
            }
        }

        return codes;
    }
}
