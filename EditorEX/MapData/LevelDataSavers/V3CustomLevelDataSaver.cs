using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using BeatmapSaveDataCommon;
using BeatmapSaveDataVersion3;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.VersionedSaveData;
using EditorEX.MapData.Contexts;
using SiraUtil.Logging;
using Zenject;
using static EditorEX.CustomJSONData.VersionedSaveData.Custom2_6_0AndEarlierBeatmapSaveDataVersioned;
using V3 = BeatmapSaveDataVersion3;

namespace EditorEX.MapData.LevelDataSavers
{
    public class V3CustomLevelDataSaver : ICustomLevelDataSaver
    {
        [Inject]
        private SiraLog? _siraLog = null!;

        [Inject]
        private ICustomDataRepository _customDataRepository = null!;

        public bool IsVersion(Version version)
        {
            return version.Major == 3;
        }

        private CustomBeatmapSaveDataVersioned GetSaveData(
            BeatmapProjectManager projectManager,
            DifficultyBeatmapData difficultyBeatmapData
        )
        {
            var basicEventsModel = projectManager._beatmapBasicEventsDataModel;
            var levelDataModel = projectManager._beatmapObjectsDataModel;
            var eventBoxGroupsDataModel = projectManager._beatmapEventBoxGroupsDataModel;

            var bpmChanges = projectManager
                ._audioDataModel.bpmData.regions.Select(V3Converters.CreateBpmChangeEventSaveData)
                .ToList();
            var rotationEvents = new List<RotationEventData>()
                .Concat(
                    basicEventsModel
                        .GetAllDataIn(BasicBeatmapEventType.Event14)
                        .Select(x =>
                            V3Converters.CreateRotationEventSaveData(x, _customDataRepository)
                        )
                )
                .Concat(
                    basicEventsModel
                        .GetAllDataIn(BasicBeatmapEventType.Event15)
                        .Select(x =>
                            V3Converters.CreateRotationEventSaveData(x, _customDataRepository)
                        )
                )
                .ToList();
            var basicEvents = basicEventsModel
                .GetAllEventsAsList()
                .Select(x => V3Converters.CreateBasicEventSaveData(x, _customDataRepository))
                .ToList();
            var customEvents = _customDataRepository
                .GetCustomEvents()
                .Select(V3Converters.CreateCustomEventSaveData)
                .ToList();
            var colorBoostEvents = basicEventsModel
                .GetAllDataIn(BasicBeatmapEventType.Event5)
                .Select(x => V3Converters.CreateColorBoostSaveEventData(x, _customDataRepository))
                .ToList();
            var colorNotes = new List<ColorNoteData>();
            var bombNotes = new List<BombNoteData>();
            var obstacles = new List<V3.ObstacleData>();
            var sliders = new List<V3.SliderData>();
            var burstSliders = new List<BurstSliderData>();
            var waypoints = new List<V3.WaypointData>();
            foreach (
                BaseBeatmapObjectEditorData allBeatmapObject in levelDataModel.allBeatmapObjects
            )
            {
                if (!(allBeatmapObject is NoteEditorData noteEditorData))
                {
                    if (!(allBeatmapObject is WaypointEditorData w))
                    {
                        if (!(allBeatmapObject is ObstacleEditorData o))
                        {
                            if (!(allBeatmapObject is ChainEditorData c))
                            {
                                if (allBeatmapObject is ArcEditorData a)
                                {
                                    sliders.Add(
                                        V3Converters.CreateSliderSaveData(a, _customDataRepository)
                                    );
                                }
                            }
                            else
                            {
                                colorNotes.Add(
                                    V3Converters.CreateColorNoteSaveDataFromChains(
                                        c,
                                        _customDataRepository
                                    )
                                );
                                burstSliders.Add(
                                    V3Converters.CreateBurstSliderSaveData(c, _customDataRepository)
                                );
                            }
                        }
                        else
                        {
                            obstacles.Add(
                                V3Converters.CreateObstacleSaveData(o, _customDataRepository)
                            );
                        }
                    }
                    else
                    {
                        waypoints.Add(
                            V3Converters.CreateWaypointSaveData(w, _customDataRepository)
                        );
                    }
                }
                else if (noteEditorData.noteType == NoteType.Note)
                {
                    colorNotes.Add(
                        V3Converters.CreateColorNoteSaveData(noteEditorData, _customDataRepository)
                    );
                }
                else
                {
                    bombNotes.Add(
                        V3Converters.CreateBombNoteSaveData(noteEditorData, _customDataRepository)
                    );
                }
            }
            bpmChanges.Sort(LegacySavingUtil.SortByBeat);
            rotationEvents.Sort(LegacySavingUtil.SortByRotationTypeAndBeat);
            basicEvents.Sort(LegacySavingUtil.SortByEventTypeAndBeat);
            colorBoostEvents.Sort(LegacySavingUtil.SortByBeat);
            colorNotes.Sort(LegacySavingUtil.SortByBeat);
            bombNotes.Sort(LegacySavingUtil.SortByBeat);
            waypoints.Sort(LegacySavingUtil.SortByBeat);
            obstacles.Sort(LegacySavingUtil.SortByBeat);
            sliders.Sort(LegacySavingUtil.SortByBeat);
            burstSliders.Sort(LegacySavingUtil.SortByBeat);
            var vfxCollection = new FxEventsCollection();
            var tuple = (
                from e in eventBoxGroupsDataModel.GetAllEventBoxGroups()
                orderby e.beat
                select e
            )
                .Select(x =>
                    V3Converters.CreateEventBoxGroupSaveDataWithFxCollection(
                        x,
                        vfxCollection,
                        eventBoxGroupsDataModel,
                        _customDataRepository
                    )
                )
                .Aggregate(
                    (
                        new List<LightColorEventBoxGroup>(),
                        new List<LightRotationEventBoxGroup>(),
                        new List<LightTranslationEventBoxGroup>(),
                        new List<FxEventBoxGroup>()
                    ),
                    V3Converters.SplitEventBoxGroupsSaveData
                );
            List<LightColorEventBoxGroup> lightColorEventBoxGroups = tuple.Item1;
            List<LightRotationEventBoxGroup> lightRotationEventBoxGroups = tuple.Item2;
            List<LightTranslationEventBoxGroup> lightTranslationEventBoxGroups = tuple.Item3;
            List<FxEventBoxGroup> fxEventBoxGroups = tuple.Item4;
            BasicEventTypesWithKeywords basicEventTypesWithKeywords =
                new BasicEventTypesWithKeywords(
                    basicEventsModel
                        .GetBasicEventTypesForKeywordData()
                        .Select(V3Converters.CreateBasicEventTypesForKeywordSaveData)
                        .ToList()
                );

            _siraLog.Info($"Saving 1: {_customDataRepository.GetBeatmapData() == null}");
            _siraLog.Info($"Saving 2: {_customDataRepository.GetBeatmapData().customData == null}");

            var customData = _customDataRepository.GetBeatmapData().customData;

            _siraLog.Info($"Saving 3: {_customDataRepository.GetCustomEvents() == null}");
            _siraLog.Info(
                $"Saving 3: {_customDataRepository.GetCustomEvents().Select(x => new CustomEventDataSerialized(x)) == null}"
            );

            customData["customEvents"] = _customDataRepository
                .GetCustomEvents()
                .Select(x => new CustomEventDataSerialized(x));

            return new CustomBeatmapSaveDataVersioned(
                MapContext.Version.ToString(),
                bpmChanges,
                rotationEvents,
                colorNotes,
                bombNotes,
                obstacles,
                sliders,
                burstSliders,
                waypoints,
                basicEvents,
                colorBoostEvents,
                lightColorEventBoxGroups,
                lightRotationEventBoxGroups,
                lightTranslationEventBoxGroups,
                fxEventBoxGroups,
                vfxCollection,
                basicEventTypesWithKeywords,
                basicEventsModel.GetUseNormalEventsAsCompatibleEvents(),
                customData
            );
        }

        public void Save(
            BeatmapProjectManager projectManager,
            DifficultyBeatmapData difficultyBeatmapData,
            bool clearDirty
        )
        {
            if (!projectManager._beatmapDataModelsSaver.NeedsSaving())
            {
                return;
            }

            if (
                projectManager._beatmapDataModelsSaver.BeatmapNeedSaving()
                || projectManager._beatmapDataModelsSaver.LightshowNeedsSaving()
            )
            {
                var beatmapSaveData = GetSaveData(projectManager, difficultyBeatmapData);
                LegacySavingUtil.SerializeAndSave(
                    projectManager._workingBeatmapProject,
                    difficultyBeatmapData.beatmapFilename,
                    beatmapSaveData
                );
                if (clearDirty)
                {
                    projectManager._beatmapObjectsDataModel.ClearDirty();
                    projectManager._beatmapBasicEventsDataModel.ClearDirty();
                    projectManager._beatmapEventBoxGroupsDataModel.ClearDirty();
                }
            }
            if (clearDirty)
            {
                projectManager.BackupProject();
                projectManager.SaveTempProject();
            }
        }
    }
}
