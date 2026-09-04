using System;
using System.IO;
using System.Linq;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Scripts.SerializedData;
using BeatmapEditor3D.SerializedData;
using BeatmapSaveDataCommon;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.MapData.Objects;

namespace EditorEX.MapData.LevelDataLoaders
{
    public class LevelDataLoaderV3 : ICustomLevelDataLoader
    {
        private readonly ICustomDataRepository _customDataRepository;

        internal LevelDataLoaderV3(ICustomDataRepository customDataRepository)
        {
            _customDataRepository = customDataRepository;
        }

        public bool IsVersion(Version version)
        {
            return version.Major == 3;
        }

        public DifficultyLoadResult Load(
            BeatmapDataModelsLoader loader,
            string projectPath,
            string beatmapFilename,
            string? lightshowFilename
        )
        {
            _ = loader;
            _customDataRepository.ClearAll();

            var result = new DifficultyLoadResult();
            string fullPath = Path.Combine(projectPath, beatmapFilename);
            Version3CustomBeatmapSaveData beatmapSaveData =
                Version3CustomBeatmapSaveData.Deserialize(File.ReadAllText(fullPath));

            BeatmapEditorRotationProcessor_v3 rotationProcessor =
                new BeatmapEditorRotationProcessor_v3(beatmapSaveData.rotationEvents);

            result.Notes = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.colorNotes.Cast<Version3CustomBeatmapSaveData.ColorNoteSaveData>(),
                    ColorNoteCodec.LoadV3,
                    rotationProcessor,
                    _customDataRepository
                )
                .ToList();
            rotationProcessor.ResetRotation();
            result.Notes.AddRange(
                LevelDataLoaderUtil.GetEditorData(
                    beatmapSaveData.bombNotes.Cast<Version3CustomBeatmapSaveData.BombNoteSaveData>(),
                    BombNoteCodec.LoadV3,
                    rotationProcessor,
                    _customDataRepository
                )
            );
            rotationProcessor.ResetRotation();
            result.Waypoints = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.waypoints.Cast<Version3CustomBeatmapSaveData.WaypointSaveData>(),
                    WaypointCodec.LoadV3,
                    rotationProcessor,
                    _customDataRepository
                )
                .ToList();
            rotationProcessor.ResetRotation();
            result.Obstacles = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.obstacles.Cast<Version3CustomBeatmapSaveData.ObstacleSaveData>(),
                    ObstacleCodec.LoadV3,
                    rotationProcessor,
                    _customDataRepository
                )
                .ToList();
            rotationProcessor.ResetRotation();
            result.Arcs = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.sliders.Cast<Version3CustomBeatmapSaveData.SliderSaveData>(),
                    ArcCodec.LoadV3,
                    rotationProcessor,
                    _customDataRepository
                )
                .ToList();
            rotationProcessor.ResetRotation();
            result.Chains = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.burstSliders.Cast<Version3CustomBeatmapSaveData.BurstSliderSaveData>(),
                    ChainCodec.LoadV3,
                    rotationProcessor,
                    _customDataRepository
                )
                .ToList();

            result.BasicEvents = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.basicBeatmapEvents.Cast<Version3CustomBeatmapSaveData.BasicEventSaveData>(),
                    BasicEventCodec.LoadV3,
                    _customDataRepository
                )
                .ToList();
            result.BasicEvents.AddRange(
                LevelDataLoaderUtil.GetEditorData(
                    beatmapSaveData.colorBoostBeatmapEvents.Cast<Version3CustomBeatmapSaveData.ColorBoostEventSaveData>(),
                    ColorBoostEventCodec.LoadV3,
                    _customDataRepository
                )
            );
            result.BasicEvents.AddRange(
                LevelDataLoaderUtil.GetEditorData(
                    beatmapSaveData.rotationEvents.Cast<Version3CustomBeatmapSaveData.RotationEventSaveData>(),
                    RotationEventCodec.LoadV3,
                    _customDataRepository
                )
            );

            var customEvents = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.customEvents,
                    CustomEventCodec.LoadV3,
                    _customDataRepository
                )
                .ToList();

            _customDataRepository.SetCustomBeatmapSaveData(beatmapSaveData);
            _customDataRepository.SetCustomEvents(customEvents);

            result.BpmChanges = beatmapSaveData.bpmEvents?.ToList() ?? new();
            result.EventBoxGroups = EventBoxGroupCodec.LoadV3(beatmapSaveData);
            result.BasicEventTypesForKeyword = (
                beatmapSaveData.basicEventTypesWithKeywords?.data
                ?? Enumerable.Empty<BasicEventTypesWithKeywords.BasicEventTypesForKeyword>()
            )
                .Select(d =>
                    BasicEventTypesForKeywordEditorData.CreateNew(
                        d.keyword,
                        d.eventTypes.Select(t => (BasicBeatmapEventType)t).ToList()
                    )
                )
                .ToList();
            result.UseNormalEventsAsCompatibleEvents =
                beatmapSaveData.useNormalEventsAsCompatibleEvents;

            return result;
        }
    }
}
