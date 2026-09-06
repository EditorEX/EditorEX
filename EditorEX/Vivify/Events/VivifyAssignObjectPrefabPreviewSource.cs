using System.Collections.Generic;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Essentials.PreviewState;
using EditorEX.Essentials.Visuals.Note;
using EditorEX.Heck.Deserialize;
using EditorEX.Vivify.ObjectPrefab.Managers;
using Heck.Animation;
using SiraUtil.Logging;
using Vivify;
using Vivify.ObjectPrefab.Collections;
using Vivify.ObjectPrefab.Managers;
using Vivify.ObjectPrefab.Pools;
using Zenject;
using static Vivify.VivifyController;

namespace EditorEX.Vivify.Events
{
    internal sealed class VivifyAssignObjectPrefabPreviewSource : IPreviewStateSource
    {
        private readonly EditorDeserializedData? _editorDeserializedData;
        private readonly ICustomDataRepository _customDataRepository;
        private readonly EditorBeatmapObjectPrefabManager _beatmapObjectPrefabManager;
        private readonly EditorVivifyNotePrefabManager _notePrefabManager;
        private readonly SiraLog _log;

        private VivifyAssignObjectPrefabPreviewSource(
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData,
            ICustomDataRepository customDataRepository,
            EditorBeatmapObjectPrefabManager beatmapObjectPrefabManager,
            EditorVivifyNotePrefabManager notePrefabManager,
            SiraLog log
        )
        {
            _editorDeserializedData = deserializedData;
            _customDataRepository = customDataRepository;
            _beatmapObjectPrefabManager = beatmapObjectPrefabManager;
            _notePrefabManager = notePrefabManager;
            _log = log;
        }

        public void Build(IPreviewStateRegistry registry)
        {
            if (_editorDeserializedData == null)
            {
                return;
            }

            var items =
                new List<(
                    float Beat,
                    int Index,
                    string Key,
                    Track Track,
                    string? Asset,
                    LoadMode LoadMode,
                    PrefabDictionary Dictionary
                )>();
            int index = 0;
            foreach (CustomEventEditorData customEvent in _customDataRepository.GetCustomEvents())
            {
                if (customEvent.eventType != ASSIGN_OBJECT_PREFAB)
                {
                    continue;
                }

                if (
                    !_editorDeserializedData.Resolve(customEvent, out AssignObjectPrefabData? data)
                    || data == null
                )
                {
                    continue;
                }

                foreach ((string? key, AssignObjectPrefabData.IPrefabInfo value) in data.Assets)
                {
                    if (
                        key == null
                        || value is not AssignObjectPrefabData.ObjectPrefabInfo objectPrefabInfo
                        || objectPrefabInfo.Track == null
                    )
                    {
                        continue;
                    }

                    TryAdd(
                        items,
                        customEvent.beat,
                        ref index,
                        key,
                        objectPrefabInfo.Track,
                        objectPrefabInfo.Asset,
                        data.LoadMode,
                        CollectionFor(key)
                    );

                    if (key == NOTE_PREFAB)
                    {
                        TryAdd(
                            items,
                            customEvent.beat,
                            ref index,
                            key + ".anyDirection",
                            objectPrefabInfo.Track,
                            objectPrefabInfo.AnyDirectionAsset,
                            data.LoadMode,
                            _notePrefabManager.AnyDirectionNotePrefabs
                        );
                    }
                }
            }

            items.Sort(
                (a, b) =>
                {
                    int beat = a.Beat.CompareTo(b.Beat);
                    return beat != 0 ? beat : a.Index.CompareTo(b.Index);
                }
            );

            for (int i = 0; i < items.Count; i++)
            {
                float from = items[i].Beat;
                float to = PreviewStateOwnership.NextExclusiveEnd(
                    items,
                    i,
                    item => item.Beat,
                    (left, right) =>
                        VivifyPreviewOwnership.ConflictsPrefabAssignment(
                            left.Key,
                            left.Track,
                            right.Key,
                            right.Track,
                            right.LoadMode == LoadMode.Single
                        )
                );
                registry.Add(
                    from,
                    to,
                    CreateAction(
                        items[i].Dictionary,
                        items[i].Track,
                        items[i].Asset,
                        items[i].LoadMode,
                        items[i].Key
                    )
                );
            }
        }

        private void TryAdd(
            List<(
                float Beat,
                int Index,
                string Key,
                Track Track,
                string? Asset,
                LoadMode LoadMode,
                PrefabDictionary Dictionary
            )> items,
            float beat,
            ref int index,
            string key,
            IReadOnlyList<Track> tracks,
            string? asset,
            LoadMode loadMode,
            PrefabDictionary? dictionary
        )
        {
            if (asset == string.Empty || dictionary == null)
            {
                return;
            }

            foreach (Track track in tracks)
            {
                items.Add((beat, index++, key, track, asset, loadMode, dictionary));
            }
        }

        private PrefabDictionary? CollectionFor(string key)
        {
            PrefabDictionary? collection = key switch
            {
                NOTE_PREFAB => _notePrefabManager.ColorNotePrefabs,
                BOMB_PREFAB => _notePrefabManager.BombNotePrefabs,
                CHAIN_PREFAB => _notePrefabManager.BurstSliderPrefabs,
                CHAIN_ELEMENT_PREFAB => _notePrefabManager.BurstSliderElementPrefabs,
                _ => null,
            };
            if (collection == null)
            {
                _log.Error($"[{key}] not recognized");
            }

            return collection;
        }

        private IPreviewStateAction CreateAction(
            PrefabDictionary dictionary,
            Track track,
            string? asset,
            LoadMode loadMode,
            string key
        )
        {
            HashSet<PrefabPool?>? snapshot = null;
            return new VivifyResourcePreviewAction(
                () =>
                {
                    snapshot = _beatmapObjectPrefabManager.Snapshot(dictionary, track);
                    _log.Debug($"Assigned track prefab: [{asset ?? "null"}] for [{key}]");
                    _beatmapObjectPrefabManager.AssignTrackPrefab(
                        dictionary,
                        [track],
                        asset,
                        loadMode
                    );
                },
                () =>
                {
                    if (snapshot != null)
                    {
                        _beatmapObjectPrefabManager.Restore(dictionary, track, snapshot);
                    }
                }
            );
        }
    }
}
