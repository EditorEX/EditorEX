using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Essentials.Visuals.Note;
using EditorEX.Heck.Deserialize;
using EditorEX.Vivify.ObjectPrefab.Managers;
using Heck;
using Heck.Deserialize;
using Heck.Event;
using IPA.Utilities;
using SiraUtil.Logging;
using Vivify;
using Vivify.ObjectPrefab.Collections;
using Vivify.ObjectPrefab.Pools;
using Zenject;
using static Vivify.VivifyController;

// Based from https://github.com/Aeroluna/Vivify
namespace EditorEX.Vivify.Events
{
    //TODO: Add support for sabers and debris when analyzer is ready
    [CustomEvent(ASSIGN_OBJECT_PREFAB)]
    internal class EditorAssignObjectPrefab : IInitializable, ICustomEvent
    {
        private readonly EditorBeatmapObjectPrefabManager _beatmapObjectPrefabManager;

        //private readonly DebrisPrefabManager _debrisPrefabManager;
        private readonly EditorDeserializedData _editorDeserializedData;
        private readonly SiraLog _log;
        private readonly EditorVivifyNotePrefabManager _notePrefabManager;

        //private readonly SaberPrefabManager _saberPrefabManager;

        private EditorAssignObjectPrefab(
            SiraLog log,
            [Inject(Id = ID)] EditorDeserializedData editorDeserializedData,
            EditorBeatmapObjectPrefabManager beatmapObjectPrefabManager,
            EditorVivifyNotePrefabManager notePrefabManager
        )
        //DebrisPrefabManager debrisPrefabManager,
        //SaberPrefabManager saberPrefabManager)
        {
            _log = log;
            _editorDeserializedData = editorDeserializedData;
            _beatmapObjectPrefabManager = beatmapObjectPrefabManager;
            _notePrefabManager = notePrefabManager;
            //_debrisPrefabManager = debrisPrefabManager;
            //_saberPrefabManager = saberPrefabManager;
        }

        public void Initialize()
        {
            foreach (
                ICustomEventCustomData customEventCustomData in _editorDeserializedData
                    .CustomEventCustomDatas
                    .Values
            )
            {
                if (customEventCustomData is not AssignObjectPrefabData data)
                {
                    continue;
                }

                foreach ((string? key, AssignObjectPrefabData.IPrefabInfo value) in data.Assets)
                {
                    if (key == null)
                    {
                        continue;
                    }

                    if (value is not AssignObjectPrefabData.ObjectPrefabInfo objectPrefabInfo)
                    {
                        continue;
                    }

                    string? asset = objectPrefabInfo.Asset;
                    if (!string.IsNullOrEmpty(asset))
                    {
                        _beatmapObjectPrefabManager.PrewarmGameObjectPrefabPool(asset!, 10);
                    }

                    string? debrisAsset = objectPrefabInfo.DebrisAsset;
                    if (!string.IsNullOrEmpty(debrisAsset))
                    {
                        //_beatmapObjectPrefabManager.PrewarmGameObjectPrefabPool(asset!, 10);
                    }

                    string? anyDirectionAsset = objectPrefabInfo.AnyDirectionAsset;
                    if (!string.IsNullOrEmpty(anyDirectionAsset) && key == NOTE_PREFAB)
                    {
                        _beatmapObjectPrefabManager.PrewarmGameObjectPrefabPool(asset!, 10);
                    }

                    break;
                }
            }
        }

        public void Callback(CustomEventData customEventData)
        {
            if (
                !_editorDeserializedData.Resolve(
                    CustomDataRepository.GetCustomEventConversion(customEventData),
                    out AssignObjectPrefabData? data
                )
            )
            {
                return;
            }

            foreach ((string? key, AssignObjectPrefabData.IPrefabInfo value) in data!.Assets)
            {
                switch (value)
                {
                    case AssignObjectPrefabData.ObjectPrefabInfo objectPrefabInfo:
                        {
                            IPrefabCollection? prefabCollection = key switch
                            {
                                NOTE_PREFAB => _notePrefabManager.ColorNotePrefabs,
                                BOMB_PREFAB => _notePrefabManager.BombNotePrefabs,
                                CHAIN_PREFAB => _notePrefabManager.BurstSliderPrefabs,
                                CHAIN_ELEMENT_PREFAB => _notePrefabManager.BurstSliderElementPrefabs,
                                _ => null,
                            };

                            if (prefabCollection == null)
                            {
                                _log.Error($"[{key}] not recognized");
                                continue;
                            }

                            if (objectPrefabInfo.Track == null)
                            {
                                _log.Error("No track defined");
                                continue;
                            }

                            string? asset = objectPrefabInfo.Asset;
                            if (asset != string.Empty)
                            {
                                _log.Debug(
                                    $"Assigned track prefab: [{asset ?? "null"}] for [{key}] with load mode [{data.LoadMode}]"
                                );
                                _beatmapObjectPrefabManager.AssignTrackPrefab(
                                    (PrefabDictionary)prefabCollection,
                                    objectPrefabInfo.Track,
                                    asset,
                                    data.LoadMode
                                );
                            }

                            string? anyDirectionAsset = objectPrefabInfo.AnyDirectionAsset;
                            if (anyDirectionAsset != string.Empty && key == NOTE_PREFAB)
                            {
                                _log.Debug(
                                    $"Assigned any direction track prefab [{anyDirectionAsset ?? "null"}] for [{key}] with load mode [{data.LoadMode}]"
                                );
                                _beatmapObjectPrefabManager.AssignTrackPrefab(
                                    _notePrefabManager.AnyDirectionNotePrefabs,
                                    objectPrefabInfo.Track,
                                    anyDirectionAsset,
                                    data.LoadMode
                                );
                            }

                            string? debrisAsset = objectPrefabInfo.DebrisAsset;
                            if (debrisAsset != string.Empty)
                            {
                                break;
                                PrefabDictionary? debrisPrefabs = key switch
                                {
                                    // NOTE_PREFAB => _debrisPrefabManager.ColorNoteDebrisPrefabs,
                                    // CHAIN_PREFAB => _debrisPrefabManager.BurstSliderDebrisPrefabs,
                                    // CHAIN_ELEMENT_PREFAB => _debrisPrefabManager.BurstSliderElementDebrisPrefabs,
                                    _ => null,
                                };

                                if (debrisPrefabs == null)
                                {
                                    _log.Error($"[{key}] debris not recognized");
                                    continue;
                                }

                                _log.Debug(
                                    $"Assigned debris track prefab [{debrisAsset ?? "null"}] for [{key}] with load mode [{data.LoadMode}]"
                                );
                                _beatmapObjectPrefabManager.AssignTrackPrefab(
                                    debrisPrefabs,
                                    objectPrefabInfo.Track,
                                    debrisAsset,
                                    data.LoadMode
                                );
                            }

                            break;
                        }

                    case AssignObjectPrefabData.SaberPrefabInfo saberPrefabInfo:
                        {
                            break;
                            string? asset = saberPrefabInfo.Asset;
                            if (asset != string.Empty)
                            {
                                if (
                                    (
                                        saberPrefabInfo.Type
                                        & AssignObjectPrefabData.SaberPrefabInfo.SaberType.Left
                                    ) != 0
                                )
                                {
                                    _log.Debug(
                                        $"Assigned prefab [{asset ?? "null"}] for left saber with load mode [{data.LoadMode}]"
                                    );
                                    // _beatmapObjectPrefabManager.AssignGameObjectPrefab(
                                    //     _saberPrefabManager.SaberAPrefabs,
                                    //     asset,
                                    //     data.LoadMode,
                                    //     customEventData.time);
                                }

                                if (
                                    (
                                        saberPrefabInfo.Type
                                        & AssignObjectPrefabData.SaberPrefabInfo.SaberType.Right
                                    ) != 0
                                )
                                {
                                    _log.Debug(
                                        $"Assigned prefab [{asset ?? "null"}] for right saber with load mode [{data.LoadMode}]"
                                    );
                                    // _beatmapObjectPrefabManager.AssignGameObjectPrefab(
                                    //     _saberPrefabManager.SaberBPrefabs,
                                    //     asset,
                                    //     data.LoadMode,
                                    //     customEventData.time);
                                }
                            }

                            string? trail = saberPrefabInfo.TrailAsset;
                            if (trail != string.Empty)
                            {
                                TrailProperties trailProperties = new(
                                    saberPrefabInfo.TopPos,
                                    saberPrefabInfo.BottomPos,
                                    saberPrefabInfo.Duration,
                                    saberPrefabInfo.SamplingFrequency,
                                    saberPrefabInfo.Granularity
                                );

                                if (
                                    (
                                        saberPrefabInfo.Type
                                        & AssignObjectPrefabData.SaberPrefabInfo.SaberType.Left
                                    ) != 0
                                )
                                {
                                    _log.Debug(
                                        $"Assigned trail material [{trail ?? "null"}] for left saber with load mode [{data.LoadMode}]"
                                    );
                                    // _beatmapObjectPrefabManager.AssignTrail(
                                    //     _saberPrefabManager.SaberATrailMaterials,
                                    //     trail,
                                    //     trailProperties,
                                    //     data.LoadMode,
                                    //     customEventData.time);
                                }

                                if (
                                    (
                                        saberPrefabInfo.Type
                                        & AssignObjectPrefabData.SaberPrefabInfo.SaberType.Right
                                    ) != 0
                                )
                                {
                                    _log.Debug(
                                        $"Assigned trail material [{trail ?? "null"}] for right saber with load mode [{data.LoadMode}]"
                                    );
                                    // _beatmapObjectPrefabManager.AssignTrail(
                                    //     _saberPrefabManager.SaberBTrailMaterials,
                                    //     trail,
                                    //     trailProperties,
                                    //     data.LoadMode,
                                    //     customEventData.time);
                                }
                            }

                            break;
                        }
                }
            }
        }
    }
}
