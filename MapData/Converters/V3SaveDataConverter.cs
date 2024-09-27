using BeatmapSaveDataCommon;
using BeatmapSaveDataVersion3;
using global::CustomJSONData.CustomBeatmap;
using System.Collections.Generic;
using System.Linq;

using v2NoteSaveData = BeatmapSaveDataVersion2_6_0AndEarlier.NoteData;
using v2CustomNoteSaveData = CustomJSONData.CustomBeatmap.Version2_6_0AndEarlierCustomBeatmapSaveData.NoteSaveData;
using v3CustomNoteSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData.ColorNoteSaveData;
using v3NoteSaveData = BeatmapSaveDataVersion3.ColorNoteData;

using v3CustomBombSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData.BombNoteSaveData;
using v3BombSaveData = BeatmapSaveDataVersion3.BombNoteData;

using v2ObstacleSaveData = BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleData;
using v2CustomObstacleSaveData = CustomJSONData.CustomBeatmap.Version2_6_0AndEarlierCustomBeatmapSaveData.ObstacleSaveData;
using v3CustomObstacleSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData.ObstacleSaveData;
using v3ObstacleSaveData = BeatmapSaveDataVersion3.ObstacleData;

using v2SliderSaveData = BeatmapSaveDataVersion2_6_0AndEarlier.SliderData;
using v2CustomSliderSaveData = CustomJSONData.CustomBeatmap.Version2_6_0AndEarlierCustomBeatmapSaveData.SliderSaveData;
using v3CustomSliderSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData.SliderSaveData;
using v3SliderSaveData = BeatmapSaveDataVersion3.SliderData;

using v2WaypointSaveData = BeatmapSaveDataVersion2_6_0AndEarlier.WaypointData;
using v2CustomWaypointSaveData = CustomJSONData.CustomBeatmap.Version2_6_0AndEarlierCustomBeatmapSaveData.WaypointSaveData;
using v3CustomWaypointSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData.WaypointSaveData;
using v3WaypointSaveData = BeatmapSaveDataVersion3.WaypointData;

using v2EventSaveData = BeatmapSaveDataVersion2_6_0AndEarlier.EventData;
using v2CustomEventSaveData = CustomJSONData.CustomBeatmap.Version2_6_0AndEarlierCustomBeatmapSaveData.EventSaveData;
using v3CustomEventSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData.BasicEventSaveData;
using v3EventSaveData = BeatmapSaveDataVersion3.BasicEventData;

namespace EditorEX.MapData.Converters
{
    public static class V3SaveDataConverter
    {
        public static Version3CustomBeatmapSaveData ConvertToV3(CustomData beatmapData, Version2_6_0AndEarlierCustomBeatmapSaveData oldSaveData)
        {
            ILookup<bool, v2CustomNoteSaveData> lookup = oldSaveData.notes
                .OrderBy((v2NoteSaveData n) => n)
                .Select(x=>(v2CustomNoteSaveData)x)
                .ToLookup((v2CustomNoteSaveData n) => n.type == BeatmapSaveDataVersion2_6_0AndEarlier.NoteType.Bomb);

            List<v3NoteSaveData> colorNotes = lookup[false]
                .Select((v2CustomNoteSaveData n) => new v3CustomNoteSaveData(
                    n.time, 
                    n.lineIndex, 
                    (int)n.lineLayer,
                    (n.type == BeatmapSaveDataVersion2_6_0AndEarlier.NoteType.NoteA) ? NoteColorType.ColorA : NoteColorType.ColorB,
                    n.cutDirection,
                    0,
                    n.customData))
                .Cast<v3NoteSaveData>()
                .ToList();

            List<v3BombSaveData> bombNotes = lookup[true]
                .Select((v2CustomNoteSaveData n) => new v3CustomBombSaveData(
                    n.time, 
                    n.lineIndex, 
                    (int)n.lineLayer, 
                    n.customData))
                .Cast<v3BombSaveData>()
                .ToList();

            List<v3ObstacleSaveData> obstacles = oldSaveData.obstacles
                .Cast<v2CustomObstacleSaveData>()
                .OrderBy((v2CustomObstacleSaveData n) => n)
                .Select((v2CustomObstacleSaveData n) => new v3CustomObstacleSaveData(
                    n.time,
                    n.lineIndex,
                    BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter.GetLayerForObstacleType(n.type), 
                    n.duration, 
                    n.width,
                    BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.ObstacleConverter.GetHeightForObstacleType(n.type),
                    n.customData))
                .Cast<v3ObstacleSaveData>()
                .ToList();

            List<v3SliderSaveData> sliders = oldSaveData.sliders
                .Cast<v2CustomSliderSaveData>()
                .OrderBy((v2CustomSliderSaveData n) => n)
                .Select((v2CustomSliderSaveData n) => new v3CustomSliderSaveData(
                    (n.colorType == BeatmapSaveDataVersion2_6_0AndEarlier.ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB, 
                    n.time, 
                    n.headLineIndex, 
                    (int)n.headLineLayer, 
                    n.headControlPointLengthMultiplier, 
                    n.headCutDirection, 
                    n.tailTime, 
                    n.tailLineIndex, 
                    (int)n.tailLineLayer, 
                    n.tailControlPointLengthMultiplier, 
                    n.tailCutDirection, 
                    n.sliderMidAnchorMode, 
                    n.customData))
                .Cast<v3SliderSaveData>()
                .ToList();

            List<v3WaypointSaveData> waypoints = oldSaveData.waypoints
                .Cast<v2CustomWaypointSaveData>()
                .OrderBy((v2CustomWaypointSaveData n) => n)
                .Select((v2CustomWaypointSaveData n) => new v3CustomWaypointSaveData(
                    n.time, 
                    n.lineIndex, 
                    (int)n.lineLayer, 
                    n.offsetDirection, 
                    n.customData))
                .Cast<v3WaypointSaveData>()
                .ToList();

            ILookup<int, v2CustomEventSaveData> eventsSplit = oldSaveData.events
                .Cast<v2CustomEventSaveData>()
                .OrderBy((v2CustomEventSaveData n) => n)
                .ToLookup(n => n.type switch
                {
                    BeatmapEventType.Event5 => 0,
                    BeatmapEventType.Event14 => 1,
                    BeatmapEventType.Event15 => 1,
                    BeatmapEventType.BpmChange => 2,
                    _ => 3
                });

            List<ColorBoostEventData> colorBoosts =
                eventsSplit[0]
                    .Select(n => new Version3CustomBeatmapSaveData.ColorBoostEventSaveData(n.time, n.value == 1, n.customData))
                    .Cast<ColorBoostEventData>()
                    .ToList();

            List<RotationEventData> rotationEvents =
                eventsSplit[1]
                    .Select(n => new Version3CustomBeatmapSaveData.RotationEventSaveData(
                        n.time,
                        n.type == BeatmapEventType.Event14 ? ExecutionTime.Early : ExecutionTime.Late,
                        BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.BasicEventConverter.SpawnRotationForEventValue(n.value),
                        n.customData))
                    .Cast<RotationEventData>()
                    .ToList();

            List<BpmChangeEventData> bpmChanges =
                eventsSplit[2]
                    .Select(n => new Version3CustomBeatmapSaveData.BpmChangeEventSaveData(n.time, n.floatValue, n.customData))
                    .Cast<BpmChangeEventData>()
                    .ToList();

            List<BasicEventData> basicEvents =
                eventsSplit[3]
                    .Select(n => new v3CustomEventSaveData(n.time, n.type, n.value, n.floatValue, n.customData))
                    .Cast<BasicEventData>()
                    .ToList();

            // specialeventkeywordfiltersdata
            BasicEventTypesWithKeywords basicEventTypesWithKeywords =
                new(oldSaveData.specialEventsKeywordFilters.keywords
                    .Select(n => new BasicEventTypesWithKeywords.BasicEventTypesForKeyword(n.keyword, n.specialEvents))
                    .ToList());

            // custom events
            List<Version3CustomBeatmapSaveData.CustomEventSaveData> customEvents = oldSaveData.customEvents
                .Select(n => new Version3CustomBeatmapSaveData.CustomEventSaveData(n.time, n.type, n.customData))
                .ToList();

            // yay we're done
            return new Version3CustomBeatmapSaveData(
                oldSaveData.version,
                bpmChanges,
                rotationEvents,
                colorNotes,
                bombNotes,
                obstacles,
                sliders,
                new List<BurstSliderData>(),
                waypoints,
                basicEvents,
                colorBoosts,
                new List<LightColorEventBoxGroup>(),
                new List<LightRotationEventBoxGroup>(),
                new List<LightTranslationEventBoxGroup>(),
                new List<FxEventBoxGroup>(),
                new FxEventsCollection(),
                basicEventTypesWithKeywords,
                true,
                customEvents,
                oldSaveData.customData);
        }
    }
}
