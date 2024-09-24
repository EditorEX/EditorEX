using BeatmapSaveDataVersion2_6_0AndEarlier;
using BetterEditor.CustomJSONData.CustomEvents;
using CustomJSONData.CustomBeatmap;
using System.Collections.Generic;
using static BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData;

namespace BetterEditor.CustomJSONData.VersionedSaveData
{
    public class Custom2_6_0AndEarlierBeatmapSaveDataVersioned
    {
        public string _version { get; private set; }
        public List<BeatmapSaveData.EventData> _events { get; private set; }
        public List<BeatmapSaveData.NoteData> _notes { get; }
        public List<BeatmapSaveData.SliderData> _sliders { get; }
        public List<BeatmapSaveData.WaypointData> _waypoints { get; }
        public List<CustomObstacleDataSerialized> _obstacles { get; }
        public BeatmapSaveData.SpecialEventKeywordFiltersData _specialEventsKeywordFilters { get; }
        public CustomData _customData { get; }

        public Custom2_6_0AndEarlierBeatmapSaveDataVersioned(string version, List<BeatmapSaveData.EventData> events, List<BeatmapSaveData.NoteData> notes, List<BeatmapSaveData.SliderData> sliders, List<BeatmapSaveData.WaypointData> waypoints, List<CustomObstacleDataSerialized> obstacles, BeatmapSaveData.SpecialEventKeywordFiltersData specialEventsKeywordFilters, CustomData customData)
        {
            _version = version;
            _events = events;
            _notes = notes;
            _sliders = sliders;
            _waypoints = waypoints;
            _obstacles = obstacles;
            _specialEventsKeywordFilters = specialEventsKeywordFilters;
            _customData = customData;
        }

        public class CustomEventDataSerialized
        {
            internal CustomEventDataSerialized(CustomEventEditorData customEventData)
            {
                _time = customEventData.beat;
                _type = customEventData.eventType;
                _data = customEventData.customData;
            }

            public float _time { get; }

            public string _type { get; }

            public CustomData _data { get; }
        }

        public class CustomObstacleDataSerialized
        {
            internal CustomObstacleDataSerialized(Custom2_6_0AndEarlierBeatmapSaveData.ObstacleData customEventData)
            {
                _time = customEventData.time;
                _lineIndex = customEventData.lineIndex;
                _type = customEventData.type;
                _duration = customEventData.duration;
                _width = customEventData.width;
                _customData = customEventData.customData;
            }

            public CustomData _customData { get; }

            public float _time { get; }

            public int _lineIndex { get; }

            public ObstacleType _type { get; }

            public float _duration { get; }

            public int _width { get; }
        }
    }
}
