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
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.MapData.Converters;

namespace EditorEX.MapData.LevelDataLoaders
{
    public class LevelDataLoaderV2
    {
        public static CustomEventEditorData CreateCustomEventEditorData(
            Version2_6_0AndEarlierCustomBeatmapSaveData.CustomEventSaveData data
        )
        {
            return CustomEventEditorData.CreateNew(data.time, data.type, data.customData, true);
        }

        public void Load(
            BeatmapDataModelsLoader __instance,
            string projectPath,
            string filename,
            out List<NoteEditorData> notes,
            out List<WaypointEditorData> waypoints,
            out List<ObstacleEditorData> obstacles,
            out List<ArcEditorData> sliders,
            out List<BasicEventEditorData> events,
            out List<BasicEventTypesForKeywordEditorData> basicEventTypesForKeywordEditorData
        )
        {
            CustomDataRepository.ClearAll();

            var customLevelInfoSaveData = CustomLevelInfoSaveData.Deserialize(
                File.ReadAllText(Path.Combine(projectPath, "Info.dat"))
            );

            Version2_6_0AndEarlierCustomBeatmapSaveData beatmapSaveData =
                Version2_6_0AndEarlierCustomBeatmapSaveData.Deserialize(
                    File.ReadAllText(Path.Combine(projectPath, filename))
                );

            EventData[] array = beatmapSaveData
                .events.Where(
                    (EventData e) =>
                        e.type == BeatmapEventType.Event14 || e.type == BeatmapEventType.Event15
                )
                .ToArray();
            BeatmapEditorRotationProcessor_v2 rotationProcessor =
                new BeatmapEditorRotationProcessor_v2(array);

            notes = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData
                        .notes.Where(x => x.type != NoteType.GhostNote && x.type != NoteType.None)
                        .Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.NoteSaveData>(),
                    BeatmapDataModelsLoader.CreateNoteEditorData_v2,
                    rotationProcessor
                )
                .ToList();
            rotationProcessor.ResetRotation();
            waypoints = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.waypoints.Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.WaypointSaveData>(),
                    BeatmapDataModelsLoader.CreateWaypointEditorData_v2,
                    rotationProcessor
                )
                .ToList();
            rotationProcessor.ResetRotation();
            obstacles = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.obstacles.Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.ObstacleSaveData>(),
                    BeatmapDataModelsLoader.CreateObstacleEditorData_v2,
                    rotationProcessor
                )
                .ToList();
            rotationProcessor.ResetRotation();
            sliders = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.sliders.Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.SliderSaveData>(),
                    BeatmapDataModelsLoader.CreateSliderEditorData_v2,
                    rotationProcessor
                )
                .ToList();

            Version version = new Version(beatmapSaveData.version);
            Version version2 = new Version("2.5.0");
            if (version.CompareTo(version2) < 0)
            {
                beatmapSaveData.ConvertBeatmapSaveDataPreV2_5_0();
            }

            events = LevelDataLoaderUtil
                .GetEditorData(
                    beatmapSaveData.events.Cast<Version2_6_0AndEarlierCustomBeatmapSaveData.EventSaveData>(),
                    BeatmapDataModelsLoader.CreateEventEditorData_v2
                )
                .ToList();

            var customEvents = LevelDataLoaderUtil
                .GetEditorData(beatmapSaveData.customEvents, CreateCustomEventEditorData)
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

            // Our static representation is v3.
            CustomDataRepository.SetCustomBeatmapSaveData(
                V3SaveDataConverter.ConvertToV3(
                    beatmapCustomDatasByFilename[filename],
                    beatmapSaveData
                )
            );
            CustomDataRepository.SetCustomEvents(customEvents);

            if (
                beatmapSaveData.specialEventsKeywordFilters != null
                && beatmapSaveData.specialEventsKeywordFilters.keywords != null
            )
            {
                basicEventTypesForKeywordEditorData = beatmapSaveData
                    .specialEventsKeywordFilters.keywords.Select(
                        new Func<SpecialEventsForKeyword, BasicEventTypesForKeywordEditorData>(
                            __instance.CreateBasicEventTypesForKeywordData_v2
                        )
                    )
                    .ToList();
                return;
            }
            basicEventTypesForKeywordEditorData = new List<BasicEventTypesForKeywordEditorData>();
        }
    }
}
