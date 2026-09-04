using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using BeatmapSaveDataCommon;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.VersionedSaveData;
using EditorEX.MapData.Bookmarks;
using EditorEX.MapData.Contexts;
using EditorEX.MapData.Objects;
using V3 = BeatmapSaveDataVersion3;

namespace EditorEX.MapData.LevelDataSavers
{
    public class V3CustomLevelDataSaver : ICustomLevelDataSaver
    {
        private readonly ICustomDataRepository _customDataRepository;

        private V3CustomLevelDataSaver(ICustomDataRepository customDataRepository)
        {
            _customDataRepository = customDataRepository;
        }

        public bool IsVersion(Version version)
        {
            return version.Major == 3;
        }

        private CustomBeatmapSaveDataVersioned GetSaveData(BeatmapProjectManager projectManager)
        {
            var basicEventsModel = projectManager._beatmapBasicEventsDataModel;
            var levelDataModel = projectManager._beatmapObjectsDataModel;
            var eventBoxGroupsDataModel = projectManager._beatmapEventBoxGroupsDataModel;

            var bpmChanges = projectManager
                ._audioDataModel.bpmData.regions.Select(BpmChangeCodec.SaveV3)
                .ToList();
            var rotationEvents = new List<RotationEventData>()
                .Concat(
                    basicEventsModel
                        .GetAllDataIn(BasicBeatmapEventType.Event14)
                        .Select(x => RotationEventCodec.SaveV3(x, _customDataRepository))
                )
                .Concat(
                    basicEventsModel
                        .GetAllDataIn(BasicBeatmapEventType.Event15)
                        .Select(x => RotationEventCodec.SaveV3(x, _customDataRepository))
                )
                .ToList();
            var basicEvents = basicEventsModel
                .GetAllEventsAsList()
                .Select(x => BasicEventCodec.SaveV3(x, _customDataRepository))
                .ToList();
            var colorBoostEvents = basicEventsModel
                .GetAllDataIn(BasicBeatmapEventType.Event5)
                .Select(x => ColorBoostEventCodec.SaveV3(x, _customDataRepository))
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
                switch (allBeatmapObject)
                {
                    case NoteEditorData noteEditorData
                        when noteEditorData.noteType == NoteType.Note:
                        colorNotes.Add(
                            ColorNoteCodec.SaveV3(noteEditorData, _customDataRepository)
                        );
                        break;
                    case NoteEditorData bomb:
                        bombNotes.Add(BombNoteCodec.SaveV3(bomb, _customDataRepository));
                        break;
                    case WaypointEditorData waypoint:
                        waypoints.Add(WaypointCodec.SaveV3(waypoint, _customDataRepository));
                        break;
                    case ObstacleEditorData obstacle:
                        obstacles.Add(ObstacleCodec.SaveV3(obstacle, _customDataRepository));
                        break;
                    case ChainEditorData chain:
                        colorNotes.Add(
                            ColorNoteCodec.SaveV3FromChain(chain, _customDataRepository)
                        );
                        burstSliders.Add(ChainCodec.SaveV3(chain, _customDataRepository));
                        break;
                    case ArcEditorData arc:
                        sliders.Add(ArcCodec.SaveV3(arc, _customDataRepository));
                        break;
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
                    EventBoxGroupCodec.SaveV3(
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
                    EventBoxGroupCodec.SplitV3
                );
            BasicEventTypesWithKeywords basicEventTypesWithKeywords =
                new BasicEventTypesWithKeywords(
                    basicEventsModel
                        .GetBasicEventTypesForKeywordData()
                        .Select(BasicEventCodec.SaveKeywordV3)
                        .ToList()
                );

            var customData = _customDataRepository.GetBeatmapData()?.customData ?? new CustomData();
            CustomEventCodec.Write(customData, _customDataRepository.GetCustomEvents(), v3: true);
            CustomDataBookmarkCodec.Write(customData, projectManager._bookmarksDataModel, v3: true);

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
                tuple.Item1,
                tuple.Item2,
                tuple.Item3,
                tuple.Item4,
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
            if (LevelDataSaveOps.ShouldSkipSave(projectManager))
            {
                return;
            }

            if (LevelDataSaveOps.BeatmapOrLightshowOrBookmarksNeedSaving(projectManager))
            {
                var beatmapSaveData = GetSaveData(projectManager);
                LegacySavingUtil.SerializeAndSave(
                    projectManager._workingBeatmapProject,
                    difficultyBeatmapData.beatmapFilename,
                    beatmapSaveData
                );
                if (clearDirty)
                {
                    LevelDataSaveOps.ClearDifficultyDirty(projectManager);
                }
            }

            LevelDataSaveOps.BackupAndSaveTemp(projectManager, clearDirty);
        }
    }
}
