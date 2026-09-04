using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Scripts.SerializedData;
using BeatmapEditor3D.SerializedData;
using BeatmapSaveDataCommon;
using BeatmapSaveDataVersion2_6_0AndEarlier;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.MapData.Converters;
using EditorEX.MapData.Objects;

namespace EditorEX.MapData.LevelDataLoaders
{
    public class LevelDataLoaderV2 : ICustomLevelDataLoader
    {
        private readonly ICustomDataRepository _customDataRepository;

        private LevelDataLoaderV2(ICustomDataRepository customDataRepository)
        {
            _customDataRepository = customDataRepository;
        }

        public bool IsVersion(Version version)
        {
            return version.Major == 2;
        }

        public DifficultyLoadResult Load(
            BeatmapDataModelsLoader loader,
            string projectPath,
            string beatmapFilename,
            string? lightshowFilename
        )
        {
            _customDataRepository.ClearAll();

            var result = new DifficultyLoadResult();
            var customLevelInfoSaveData = CustomLevelInfoSaveData.Deserialize(
                File.ReadAllText(Path.Combine(projectPath, "Info.dat"))
            );

            Version2_6_0AndEarlierCustomBeatmapSaveData beatmapSaveData =
                Version2_6_0AndEarlierCustomBeatmapSaveData.Deserialize(
                    File.ReadAllText(Path.Combine(projectPath, beatmapFilename))
                );

            EventData[] array = beatmapSaveData
                .events.Where(e =>
                    e.type == BeatmapEventType.Event14 || e.type == BeatmapEventType.Event15
                )
                .ToArray();
            BeatmapEditorRotationProcessor_v2 rotationProcessor =
                new BeatmapEditorRotationProcessor_v2(array);

            result.Notes = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData
                        .notes.Where(x => x.type != NoteType.GhostNote && x.type != NoteType.None)
                        .Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.NoteSaveData>(),
                    ColorNoteCodec.LoadV2,
                    rotationProcessor,
                    _customDataRepository
                )
                .ToList();
            rotationProcessor.ResetRotation();
            result.Waypoints = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.waypoints.Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.WaypointSaveData>(),
                    WaypointCodec.LoadV2,
                    rotationProcessor,
                    _customDataRepository
                )
                .ToList();
            rotationProcessor.ResetRotation();
            result.Obstacles = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.obstacles.Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.ObstacleSaveData>(),
                    ObstacleCodec.LoadV2,
                    rotationProcessor,
                    _customDataRepository
                )
                .ToList();
            rotationProcessor.ResetRotation();
            result.Arcs = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.sliders.Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.SliderSaveData>(),
                    ArcCodec.LoadV2,
                    rotationProcessor,
                    _customDataRepository
                )
                .ToList();

            Version version = new Version(beatmapSaveData.version);
            if (version.CompareTo(new Version("2.5.0")) < 0)
            {
                beatmapSaveData.ConvertBeatmapSaveDataPreV2_5_0();
            }

            result.BasicEvents = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.events.Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.EventSaveData>(),
                    BasicEventCodec.LoadV2,
                    _customDataRepository
                )
                .ToList();

            var customEvents = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.customEvents,
                    CustomEventCodec.LoadV2,
                    _customDataRepository
                )
                .ToList();

            Dictionary<string, CustomData> beatmapCustomDatasByFilename = customLevelInfoSaveData
                .difficultyBeatmapSets.SelectMany(x =>
                    x.difficultyBeatmaps.Select(x =>
                        (
                            x.beatmapFilename,
                            (x as CustomLevelInfoSaveData.DifficultyBeatmap).customData
                        )
                    )
                )
                .ToDictionary(x => x.beatmapFilename, x => x.customData);

            _customDataRepository.SetCustomBeatmapSaveData(
                V3SaveDataConverter.ConvertToV3(
                    beatmapCustomDatasByFilename[beatmapFilename],
                    beatmapSaveData
                )
            );
            _customDataRepository.SetCustomEvents(customEvents);

            if (
                beatmapSaveData.specialEventsKeywordFilters != null
                && beatmapSaveData.specialEventsKeywordFilters.keywords != null
            )
            {
                result.BasicEventTypesForKeyword = beatmapSaveData
                    .specialEventsKeywordFilters.keywords.Select(
                        loader.CreateBasicEventTypesForKeywordData_v2
                    )
                    .ToList();
            }

            return result;
        }
    }
}
