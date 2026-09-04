using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using BeatmapSaveDataVersion2_6_0AndEarlier;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.VersionedSaveData;
using EditorEX.MapData.Bookmarks;
using EditorEX.MapData.Contexts;
using EditorEX.MapData.Objects;
using V2 = BeatmapSaveDataVersion2_6_0AndEarlier;

namespace EditorEX.MapData.LevelDataSavers
{
    public class V2CustomLevelDataSaver : ICustomLevelDataSaver
    {
        private readonly ICustomDataRepository _customDataRepository;

        private V2CustomLevelDataSaver(ICustomDataRepository customDataRepository)
        {
            _customDataRepository = customDataRepository;
        }

        public bool IsVersion(Version version)
        {
            return version.Major == 2;
        }

        private Custom2_6_0AndEarlierBeatmapSaveDataVersioned GetSaveData(
            BeatmapProjectManager projectManager
        )
        {
            var basicEventsModel = projectManager._beatmapBasicEventsDataModel;
            var levelDataModel = projectManager._beatmapObjectsDataModel;

            bool supportFloatValue = MapContext.Version >= new Version(2, 5, 0);
            List<V2.EventData> events = basicEventsModel
                .GetAllEventsAsList()
                .Select(x => BasicEventCodec.SaveV2(x, supportFloatValue, _customDataRepository))
                .ToList();
            List<V2.SpecialEventsForKeyword> specialEvents = basicEventsModel
                .GetBasicEventTypesForKeywordData()
                .Select(BasicEventCodec.SaveKeywordV2)
                .SelectMany(x => x)
                .ToList();
            List<V2.NoteData> notes = new List<V2.NoteData>();
            List<V2.ObstacleData> obstacles = new List<V2.ObstacleData>();
            List<V2.SliderData> sliders = new List<V2.SliderData>();
            List<V2.WaypointData> waypoints = new List<V2.WaypointData>();
            foreach (
                BaseBeatmapObjectEditorData allBeatmapObject in levelDataModel.allBeatmapObjects
            )
            {
                switch (allBeatmapObject)
                {
                    case NoteEditorData noteEditorData
                        when noteEditorData.noteType
                            is BeatmapEditor3D.Types.NoteType.Note
                                or BeatmapEditor3D.Types.NoteType.Bomb:
                        notes.Add(ColorNoteCodec.SaveV2(noteEditorData, _customDataRepository));
                        break;
                    case WaypointEditorData waypoint:
                        waypoints.Add(WaypointCodec.SaveV2(waypoint, _customDataRepository));
                        break;
                    case ObstacleEditorData obstacle:
                        obstacles.Add(ObstacleCodec.SaveV2(obstacle, _customDataRepository));
                        break;
                    case ChainEditorData:
                        break;
                    case ArcEditorData arc:
                        sliders.Add(ArcCodec.SaveV2(arc, _customDataRepository));
                        break;
                }
            }

            events.Sort(LegacySavingUtil.SortByEventTypeAndBeat);
            notes.Sort(LegacySavingUtil.SortByBeat);
            waypoints.Sort(LegacySavingUtil.SortByBeat);
            obstacles.Sort(LegacySavingUtil.SortByBeat);
            sliders.Sort(LegacySavingUtil.SortByBeat);

            var customData =
                _customDataRepository.GetBeatmapData()?.customData
                ?? new global::CustomJSONData.CustomBeatmap.CustomData();
            CustomEventCodec.Write(customData, _customDataRepository.GetCustomEvents(), v3: false);
            CustomDataBookmarkCodec.Write(
                customData,
                projectManager._bookmarksDataModel,
                v3: false
            );

            return new Custom2_6_0AndEarlierBeatmapSaveDataVersioned(
                MapContext.Version.ToString(),
                events,
                notes,
                sliders,
                waypoints,
                obstacles,
                new SpecialEventKeywordFiltersData(specialEvents),
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
