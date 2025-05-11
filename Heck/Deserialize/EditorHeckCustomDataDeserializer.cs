using BeatmapEditor3D.DataModels;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Heck.EventData;
using EditorEX.Heck.ObjectData;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using System;
using System.Collections.Generic;
using Heck.Deserialize;
using SiraUtil.Logging;

namespace EditorEX.Heck.Deserialize
{
    internal class EditorHeckCustomDataDeserializer : IEditorObjectsDeserializer, IEditorCustomEventsDeserializer
    {
        private readonly SiraLog _siraLog;
        private readonly BeatmapObjectsDataModel _beatmapObjectsDataModel;
        private readonly Dictionary<string, Track> _tracks;
        private readonly Dictionary<string, List<object>> _pointDefinitions;
        private bool _v2;

        private EditorHeckCustomDataDeserializer(
            SiraLog siraLog,
            BeatmapObjectsDataModel beatmapObjectsDataModel,
            Dictionary<string, Track> beatmapTracks,
            Dictionary<string, List<object>> pointDefinitions,
            bool v2)
        {
            _siraLog = siraLog;
            _beatmapObjectsDataModel = beatmapObjectsDataModel;
            _tracks = beatmapTracks;
            _pointDefinitions = pointDefinitions;
            _v2 = v2;
        }

        public Dictionary<BaseEditorData?, IObjectCustomData> DeserializeObjects()
        {
            Dictionary<BaseEditorData?, IObjectCustomData> dictionary = new Dictionary<BaseEditorData?, IObjectCustomData>();
            foreach (BaseEditorData? baseEditorData in _beatmapObjectsDataModel.allBeatmapObjects)
            {
                if (dictionary.ContainsKey(baseEditorData)) continue;
                CustomData customData = CustomDataRepository.GetCustomData(baseEditorData);
                if (customData == null)
                {
                    //Plugin.Log.Info(baseEditorData.GetType().Name);
                }
                else
                {
                    dictionary.Add(baseEditorData, new EditorHeckObjectData(customData, _tracks, _v2));
                }
            }
            return dictionary;
        }

        public Dictionary<CustomEventEditorData, ICustomEventCustomData> DeserializeCustomEvents()
        {
            Dictionary<CustomEventEditorData, ICustomEventCustomData> dictionary = new Dictionary<CustomEventEditorData, ICustomEventCustomData>();
            foreach (CustomEventEditorData customEventData in CustomDataRepository.GetCustomEvents())
            {
                bool v2 = customEventData.version2_6_0AndEarlier;
                try
                {
                    string eventType = customEventData.eventType;
                    if (!(eventType == "AnimateTrack") && !(eventType == "AssignPathAnimation"))
                    {
                        if (eventType == "InvokeEvent")
                        {
                            if (!v2)
                            {
                                dictionary.Add(customEventData, new EditorInvokeEventData(customEventData));
                            }
                        }
                    }
                    else
                    {
                        dictionary.Add(customEventData, new EditorCoroutineEventData(_siraLog, customEventData, _pointDefinitions, _tracks, _v2));
                    }
                }
                catch (Exception e)
                {
                    _siraLog.Error(e);
                }
            }
            return dictionary;
        }
    }
}
