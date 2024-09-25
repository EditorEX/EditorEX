using BeatmapEditor3D.DataModels;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.CustomJSONData.Util;
using EditorEX.NoodleExtensions.ObjectData;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using NoodleExtensions;
using System;
using System.Collections.Generic;
using Heck.Deserialize;
using EditorEX.Heck.Deserialize;

namespace EditorEX.NoodleExtensions.Deserialize
{
    internal class EditorNoodleCustomDataDeserializer : IEditorCustomEventsDeserializer, IEditorEarlyDeserializer, IEditorObjectsDeserializer
    {
        private readonly BeatmapObjectsDataModel _beatmapObjectsDataModel;
        private readonly Dictionary<string, Track> _tracks;
        private readonly Dictionary<string, List<object>> _pointDefinitions;
        private readonly TrackBuilder _trackBuilder;
        private readonly bool _v2;

        private EditorNoodleCustomDataDeserializer(
            BeatmapObjectsDataModel beatmapObjectsDataModel,
            Dictionary<string, Track> beatmapTracks,
            Dictionary<string, List<object>> pointDefinitions,
            TrackBuilder trackBuilder,
            bool v2)
        {
            _beatmapObjectsDataModel = beatmapObjectsDataModel;
            _tracks = beatmapTracks;
            _pointDefinitions = pointDefinitions;
            _trackBuilder = trackBuilder;
            _v2 = v2;
        }

        public void DeserializeEarly()
        {
            foreach (CustomEventEditorData customEventEditorData in CustomDataRepository.GetCustomEvents())
            {
                bool v2 = customEventEditorData.version2_6_0AndEarlier;
                try
                {
                    string eventType = customEventEditorData.eventType;
                    if (!(eventType == "AssignPlayerToTrack"))
                    {
                        if (eventType == "AssignTrackParent")
                        {
                            _trackBuilder.AddFromCustomData(customEventEditorData.GetCustomData(), v2 ? "_parentTrack" : "parentTrack", true);
                        }
                    }
                    else
                    {
                        _trackBuilder.AddFromCustomData(customEventEditorData.GetCustomData(), v2, true);
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e);
                }
            }
        }

        public Dictionary<BaseEditorData, IObjectCustomData> DeserializeObjects()
        {
            Dictionary<BaseEditorData, IObjectCustomData> dictionary = new Dictionary<BaseEditorData, IObjectCustomData>();
            foreach (BaseEditorData baseEditorData in _beatmapObjectsDataModel.allBeatmapObjects)
            {
                CustomData customData = baseEditorData.GetCustomData();
                ObstacleEditorData customObstacleData = baseEditorData as ObstacleEditorData;
                if (customObstacleData == null)
                {
                    NoteEditorData customNoteData = baseEditorData as NoteEditorData;
                    if (customNoteData == null)
                    {
                        ArcEditorData customSliderData = baseEditorData as ArcEditorData;
                        if (customSliderData == null)
                        {
                            ChainEditorData customChainData = baseEditorData as ChainEditorData;
                            if (customChainData == null)
                            {
                                dictionary.Add(baseEditorData, new EditorNoodleObjectData(baseEditorData, customData, _pointDefinitions, _tracks, _v2, false));
                            }
                            else
                            {
                                dictionary.Add(baseEditorData, new EditorNoodleObjectData(customChainData, customData, _pointDefinitions, _tracks, _v2, false));
                            }
                        }
                        else
                        {
                            dictionary.Add(baseEditorData, new EditorNoodleSliderData(customSliderData, customData, _pointDefinitions, _tracks, _v2, false));
                        }
                    }
                    else
                    {
                        dictionary.Add(baseEditorData, new EditorNoodleNoteData(customNoteData, customData, _pointDefinitions, _tracks, _v2, false));
                    }
                }
                else
                {
                    dictionary.Add(baseEditorData, new EditorNoodleObstacleData(customObstacleData, customData, _pointDefinitions, _tracks, _v2, false));
                }
            }
            return dictionary;
        }

        public Dictionary<CustomEventEditorData, ICustomEventCustomData> DeserializeCustomEvents()
        {
            Dictionary<CustomEventEditorData, ICustomEventCustomData> dictionary = new Dictionary<CustomEventEditorData, ICustomEventCustomData>();
            foreach (CustomEventEditorData customEventEditorData in CustomDataRepository.GetCustomEvents())
            {
                bool v2 = customEventEditorData.version2_6_0AndEarlier;
                try
                {
                    CustomData data = customEventEditorData.customData;
                    string eventType = customEventEditorData.eventType;
                    if (!(eventType == "AssignPlayerToTrack"))
                    {
                        if (eventType == "AssignTrackParent")
                        {
                            dictionary.Add(customEventEditorData, new NoodleParentTrackEventData(data, _tracks, v2));
                        }
                    }
                    else
                    {
                        dictionary.Add(customEventEditorData, new NoodlePlayerTrackEventData(data, _tracks, v2));
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e);
                }
            }
            return dictionary;
        }
    }
}
