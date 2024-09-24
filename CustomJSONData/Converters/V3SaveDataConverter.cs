using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterEditor.CustomJSONData.Converters
{
    public static class V3SaveDataConverter
    {
        public static CustomBeatmapSaveData ConvertToV3(CustomData beatmapData, CustomData levelData, Custom2_6_0AndEarlierBeatmapSaveData oldSaveData)
        {
            ILookup<bool, Custom2_6_0AndEarlierBeatmapSaveData.NoteData> lookup = oldSaveData.notes.OrderBy((Custom2_6_0AndEarlierBeatmapSaveData.NoteData n) => n).ToLookup((Custom2_6_0AndEarlierBeatmapSaveData.NoteData n) => n.type == BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.NoteType.Bomb);
            List<BeatmapSaveData.ColorNoteData> colorNotes = lookup[false].Select((Custom2_6_0AndEarlierBeatmapSaveData.NoteData n) => new CustomBeatmapSaveData.ColorNoteData(n.time, n.lineIndex, (int)n.lineLayer, BeatmapSaveData.GetNoteColorType(n.type), n.cutDirection, 0, n.customData, true)).Cast<BeatmapSaveData.ColorNoteData>().ToList();
            List<BeatmapSaveData.BombNoteData> bombNotes = lookup[true].Select((Custom2_6_0AndEarlierBeatmapSaveData.NoteData n) => new CustomBeatmapSaveData.BombNoteData(n.time, n.lineIndex, (int)n.lineLayer, n.customData, true)).Cast<BeatmapSaveData.BombNoteData>().ToList();
            List<BeatmapSaveData.ObstacleData> obstacles = oldSaveData.obstacles.OrderBy((Custom2_6_0AndEarlierBeatmapSaveData.ObstacleData n) => n).Select((Custom2_6_0AndEarlierBeatmapSaveData.ObstacleData n) => new CustomBeatmapSaveData.ObstacleData(n.time, n.lineIndex, BeatmapSaveData.GetLayerForObstacleType(n.type), n.duration, n.width, BeatmapSaveData.GetHeightForObstacleType(n.type), n.customData, true)).Cast<BeatmapSaveData.ObstacleData>()
                .ToList();
            List<BeatmapSaveData.SliderData> sliders = oldSaveData.sliders.OrderBy((Custom2_6_0AndEarlierBeatmapSaveData.SliderData n) => n).Select((Custom2_6_0AndEarlierBeatmapSaveData.SliderData n) => new CustomBeatmapSaveData.SliderData(BeatmapSaveData.GetNoteColorType(n.colorType), n.time, n.headLineIndex, (int)n.headLineLayer, n.headControlPointLengthMultiplier, n.headCutDirection, n.tailTime, n.tailLineIndex, (int)n.tailLineLayer, n.tailControlPointLengthMultiplier, n.tailCutDirection, n.sliderMidAnchorMode, n.customData, true)).Cast<BeatmapSaveData.SliderData>()
                .ToList();
            List<BeatmapSaveData.WaypointData> waypoints = oldSaveData.waypoints.OrderBy((Custom2_6_0AndEarlierBeatmapSaveData.WaypointData n) => n).Select((Custom2_6_0AndEarlierBeatmapSaveData.WaypointData n) => new CustomBeatmapSaveData.WaypointData(n.time, n.lineIndex, (int)n.lineLayer, n.offsetDirection, n.customData, true)).Cast<BeatmapSaveData.WaypointData>()
                .ToList();
            ILookup<int, Custom2_6_0AndEarlierBeatmapSaveData.EventData> lookup2 = oldSaveData.events.OrderBy((Custom2_6_0AndEarlierBeatmapSaveData.EventData n) => n).ToLookup(delegate (Custom2_6_0AndEarlierBeatmapSaveData.EventData n)
            {
                BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType type = n.type;
                if (type <= BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType.Event14)
                {
                    if (type == BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType.Event5)
                    {
                        return 0;
                    }
                    if (type == BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType.Event14)
                    {
                        return 1;
                    }
                }
                else
                {
                    if (type == BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType.Event15)
                    {
                        return 1;
                    }
                    if (type == BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType.BpmChange)
                    {
                        return 2;
                    }
                }
                return 3;
            });
            List<BeatmapSaveData.ColorBoostEventData> colorBoosts = lookup2[0].Select((Custom2_6_0AndEarlierBeatmapSaveData.EventData n) => new CustomBeatmapSaveData.ColorBoostEventData(n.time, n.value == 1, n.customData, true)).Cast<BeatmapSaveData.ColorBoostEventData>().ToList();
            List<BeatmapSaveData.RotationEventData> rotationEvents = lookup2[1].Select((Custom2_6_0AndEarlierBeatmapSaveData.EventData n) => new CustomBeatmapSaveData.RotationEventData(n.time, (n.type == BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType.Event14) ? BeatmapSaveData.ExecutionTime.Early : BeatmapSaveData.ExecutionTime.Late, BeatmapSaveData.SpawnRotationForEventValue(n.value), n.customData, true)).Cast<BeatmapSaveData.RotationEventData>().ToList();
            List<BeatmapSaveData.BpmChangeEventData> bpmChanges = lookup2[2].Select((Custom2_6_0AndEarlierBeatmapSaveData.EventData n) => new CustomBeatmapSaveData.BpmChangeEventData(n.time, n.floatValue, n.customData, true)).Cast<BeatmapSaveData.BpmChangeEventData>().ToList();
            List<BeatmapSaveData.BasicEventData> basicEvents = lookup2[3].Select((Custom2_6_0AndEarlierBeatmapSaveData.EventData n) => new CustomBeatmapSaveData.BasicEventData(n.time, n.type, n.value, n.floatValue, n.customData, true)).Cast<BeatmapSaveData.BasicEventData>().ToList();
            BeatmapSaveData.BasicEventTypesWithKeywords basicEventTypesWithKeywords = new BeatmapSaveData.BasicEventTypesWithKeywords(oldSaveData.specialEventsKeywordFilters.keywords.Select((BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.SpecialEventsForKeyword n) => new BeatmapSaveData.BasicEventTypesWithKeywords.BasicEventTypesForKeyword(n.keyword, n.specialEvents)).ToList());
            List<CustomBeatmapSaveData.CustomEventData> customEvents = oldSaveData.customEvents.Select((Custom2_6_0AndEarlierBeatmapSaveData.CustomEventData n) => new CustomBeatmapSaveData.CustomEventData(n.time, n.type, n.data, true)).ToList();
            return new CustomBeatmapSaveData(bpmChanges, rotationEvents, colorNotes, bombNotes, obstacles, sliders, new List<BeatmapSaveData.BurstSliderData>(), waypoints, basicEvents, colorBoosts, new List<BeatmapSaveData.LightColorEventBoxGroup>(), new List<BeatmapSaveData.LightRotationEventBoxGroup>(), new List<BeatmapSaveData.LightTranslationEventBoxGroup>(), new List<BeatmapSaveData.FxEventBoxGroup>(), new BeatmapSaveData.FxEventsCollection(), basicEventTypesWithKeywords, true, true, customEvents, oldSaveData.customData, beatmapData, levelData);
        }
    }
}
