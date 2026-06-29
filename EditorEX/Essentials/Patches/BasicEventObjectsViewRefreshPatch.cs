using System.Collections.Generic;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using SiraUtil.Affinity;

namespace EditorEX.Essentials.Patches;

// Replaces BasicEventObjectsView.RefreshView's two nested LINQ set-difference
// expressions (currentEvents.Where(e => prevEvents.All(p => p != e)) and the
// inverse) with HashSet-based diffing. Vanilla is O(n*m) per track per refresh;
// this is O(n+m). RefreshView is signal-driven and fires every frame during
// playback, so this is the hot path.
public class BasicEventObjectsViewRefreshPatch : IAffinity
{
    // Parallel to the game's _prevEvents. For each event type we cache the HashSet
    // built for the list the game stored as "previous", keyed by that exact List
    // instance. Next frame that list comes back as prevEvents, so the set is free.
    private readonly Dictionary<
        BasicBeatmapEventType,
        (List<BasicEventEditorData> list, HashSet<BasicEventEditorData> set)
    > _setCache = new();

    private static readonly List<BasicEventEditorData> _emptyEvents = new();

    [AffinityPrefix]
    [AffinityPatch(typeof(BasicEventObjectsView), "RefreshView")]
    private bool RefreshView(BasicEventObjectsView __instance)
    {
        float from = __instance._beatmapState.beat - 5f;
        float to = __instance._beatmapState.beat + 16f;

        var page = __instance._basicEventsState.currentEventsPage;
        var tracks = __instance._beatmapDataModel.environmentTrackDefinitionModel[page];
        int count = tracks.Count;

        for (int i = 0; i < tracks.Count; i++)
        {
            var info = tracks[i];
            var type = info.basicBeatmapEventType;

            var currentEvents = __instance._beatmapBasicEventsDataModel.GetBasicEventsInterval(
                type,
                from,
                to
            );
            var prevEvents = __instance._prevEvents.TryGetValue(type, out var v) ? v : _emptyEvents;

            // Reuse last frame's set when prevEvents is the same instance we cached;
            // rebuild on first frame / page change / unexpected mutation.
            var prevSet = GetOrBuildSet(type, prevEvents);

            // Needed for eventsToRemove, and becomes next frame's prevSet.
            var currentSet = new HashSet<BasicEventEditorData>(currentEvents);

            // currentEvents not in prevEvents -> O(n)
            var eventsToAdd = new List<BasicEventEditorData>();
            for (int e = 0; e < currentEvents.Count; e++)
            {
                var evt = currentEvents[e];
                if (!prevSet.Contains(evt))
                    eventsToAdd.Add(evt);
            }

            // prevEvents not in currentEvents -> O(m)
            var eventsToRemove = new List<BasicEventEditorData>();
            for (int e = 0; e < prevEvents.Count; e++)
            {
                var evt = prevEvents[e];
                if (!currentSet.Contains(evt))
                    eventsToRemove.Add(evt);
            }

            // Called unconditionally to match vanilla behaviour (it passed
            // possibly-empty lists too).
            __instance.RemoveEventObjects(type, eventsToRemove);
            __instance.AddEventObjects(info, i, count, eventsToAdd);

            __instance._prevEvents[type] = currentEvents;
            _setCache[type] = (currentEvents, currentSet);
        }

        __instance.UpdateEventObjects();
        return false; // skip the original
    }

    private HashSet<BasicEventEditorData> GetOrBuildSet(
        BasicBeatmapEventType type,
        List<BasicEventEditorData> list
    )
    {
        if (
            _setCache.TryGetValue(type, out var cached)
            && ReferenceEquals(cached.list, list)
            && cached.set.Count == list.Count // count guard against in-place mutation
        )
        {
            return cached.set;
        }
        return new HashSet<BasicEventEditorData>(list);
    }
}
