using BeatmapSaveDataVersion2_6_0AndEarlier;
using EditorEX.CustomJSONData.CustomEvents;
using CustomJSONData.CustomBeatmap;
using System.Collections.Generic;
using static BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData;
using BeatmapSaveDataCommon;
using System;
using V2CustomSaveData = CustomJSONData.CustomBeatmap.Version2_6_0AndEarlierCustomBeatmapSaveData;
using V2SaveData = BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData;
using V2 = BeatmapSaveDataVersion2_6_0AndEarlier;
using System.Linq;

namespace EditorEX.CustomJSONData.VersionedSaveData
{
    public class Custom2_6_0AndEarlierBeatmapSaveDataVersioned
    {
        public string _version { get; private set; }
        public List<EventData> _events { get; private set; }
        public List<V2.NoteData> _notes { get; }
        public List<V2.SliderData> _sliders { get; }
        public List<V2.WaypointData> _waypoints { get; }
        public List<CustomObstacleDataSerialized> _obstacles { get; }
        public SpecialEventKeywordFiltersData _specialEventsKeywordFilters { get; }
        public CustomData _customData { get; }

        public Custom2_6_0AndEarlierBeatmapSaveDataVersioned(string version, List<EventData> events, List<V2.NoteData> notes, List<V2.SliderData> sliders, List<V2.WaypointData> waypoints, List<CustomObstacleDataSerialized> obstacles, SpecialEventKeywordFiltersData specialEventsKeywordFilters, CustomData customData)
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
                _data = new(customEventData.customData.Where(x=>!x.Key.StartsWith("NE_")));
            }

            public float _time { get; }

            public string _type { get; }

            public CustomData _data { get; }
        }

        public class CustomObstacleDataSerialized : IBeat
        {
            internal CustomObstacleDataSerialized(Version2_6_0AndEarlierCustomBeatmapSaveData.ObstacleSaveData customEventData)
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

            public float beat => _time;

            int IComparable<IBeat>.CompareTo(IBeat other)
            {
                return beat.CompareTo(other.beat);
            }

        }
    }
}
