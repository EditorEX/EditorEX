using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D;
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

        internal V2CustomLevelDataSaver(ICustomDataRepository customDataRepository)
        {
            _customDataRepository = customDataRepository;
        }

        public bool IsVersion(Version version)
        {
            return version.Major == 2;
        }

        internal static Custom2_6_0AndEarlierBeatmapSaveDataVersioned Build(
            DifficultySaveInput input,
            ICustomDataRepository repo
        )
        {
            Version version = input.MapVersion ?? MapContext.Version ?? new Version(2, 6, 0);
            bool supportFloatValue = version >= new Version(2, 5, 0);
            List<V2.EventData> events = input
                .BasicEvents.Select(x => BasicEventCodec.SaveV2(x, supportFloatValue, repo))
                .ToList();
            List<V2.SpecialEventsForKeyword> specialEvents = input
                .BasicEventTypesForKeyword.Select(BasicEventCodec.SaveKeywordV2)
                .SelectMany(x => x)
                .ToList();
            List<V2.NoteData> notes = new List<V2.NoteData>();
            List<V2.ObstacleData> obstacles = new List<V2.ObstacleData>();
            List<V2.SliderData> sliders = new List<V2.SliderData>();
            List<V2.WaypointData> waypoints = new List<V2.WaypointData>();
            foreach (NoteEditorData note in input.Notes)
            {
                if (
                    note.noteType
                    is BeatmapEditor3D.Types.NoteType.Note
                        or BeatmapEditor3D.Types.NoteType.Bomb
                )
                {
                    notes.Add(ColorNoteCodec.SaveV2(note, repo));
                }
            }

            foreach (WaypointEditorData waypoint in input.Waypoints)
            {
                waypoints.Add(WaypointCodec.SaveV2(waypoint, repo));
            }

            foreach (ObstacleEditorData obstacle in input.Obstacles)
            {
                obstacles.Add(ObstacleCodec.SaveV2(obstacle, repo));
            }

            foreach (ArcEditorData arc in input.Arcs)
            {
                sliders.Add(ArcCodec.SaveV2(arc, repo));
            }

            events.Sort(LegacySavingUtil.SortByEventTypeAndBeat);
            notes.Sort(LegacySavingUtil.SortByBeat);
            waypoints.Sort(LegacySavingUtil.SortByBeat);
            obstacles.Sort(LegacySavingUtil.SortByBeat);
            sliders.Sort(LegacySavingUtil.SortByBeat);

            var sourceCustomData =
                repo.GetBeatmapData()?.customData ?? repo.GetCustomBeatmapSaveData()?.customData;
            var customData =
                sourceCustomData == null
                    ? new global::CustomJSONData.CustomBeatmap.CustomData()
                    : new global::CustomJSONData.CustomBeatmap.CustomData(sourceCustomData);
            CustomEventCodec.Write(customData, repo.GetCustomEvents(), v3: false);
            CustomDataBookmarkCodec.Write(customData, input.Bookmarks, v3: false);

            return new Custom2_6_0AndEarlierBeatmapSaveDataVersioned(
                version.ToString(),
                events,
                notes,
                sliders,
                waypoints,
                obstacles,
                new SpecialEventKeywordFiltersData(specialEvents),
                customData
            );
        }

        private Custom2_6_0AndEarlierBeatmapSaveDataVersioned GetSaveData(
            BeatmapProjectManager projectManager
        )
        {
            var basicEventsModel = projectManager._beatmapBasicEventsDataModel;
            var levelDataModel = projectManager._beatmapObjectsDataModel;
            var input = new DifficultySaveInput
            {
                BasicEvents = basicEventsModel.GetAllEventsAsList().ToList(),
                BasicEventTypesForKeyword = basicEventsModel
                    .GetBasicEventTypesForKeywordData()
                    .ToList(),
                Bookmarks = CustomDataBookmarkCodec.Flatten(projectManager._bookmarksDataModel),
                MapVersion = MapContext.Version,
            };

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
                        input.Notes.Add(noteEditorData);
                        break;
                    case WaypointEditorData waypoint:
                        input.Waypoints.Add(waypoint);
                        break;
                    case ObstacleEditorData obstacle:
                        input.Obstacles.Add(obstacle);
                        break;
                    case ChainEditorData:
                        break;
                    case ArcEditorData arc:
                        input.Arcs.Add(arc);
                        break;
                }
            }

            return Build(input, _customDataRepository);
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
