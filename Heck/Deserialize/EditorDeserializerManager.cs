using BeatmapEditor3D.DataModels;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Essentials.Patches;
using Heck.Animation;
using Heck.Deserialize;
using IPA.Utilities;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace EditorEX.Heck.Deserialize
{
    internal class EditorDeserializerManager
    {
        private readonly SiraLog _log;
        private static readonly HashSet<EditorDataDeserializer> _customDataDeserializers = new();
        private BeatmapObjectsDataModel _objectDataModel;
        private BeatmapBasicEventsDataModel _eventsDataModel;

        private EditorDeserializerManager(SiraLog log, BeatmapObjectsDataModel objectDataModel, BeatmapBasicEventsDataModel eventsDataModel)
        {
            _log = log;
            _objectDataModel = objectDataModel;
            _eventsDataModel = eventsDataModel;
        }

        internal static EditorDataDeserializer Register<T>(string id)
        {
            EditorDataDeserializer deserializer = new(id, typeof(T));
            _customDataDeserializers.Add(deserializer);
            return deserializer;
        }

        internal static EditorDataDeserializer Register(string id, Type type)
        {
            EditorDataDeserializer deserializer = new(id, type);
            _customDataDeserializers.Add(deserializer);
            return deserializer;
        }

        internal HashSet<(object? Id, EditorDeserializedData DeserializedData)> EmptyDeserialize()
        {
            HashSet<(object? Id, EditorDeserializedData DeserializedData)> result = new();
            foreach (EditorDataDeserializer dataDeserializer in _customDataDeserializers)
            {
                result.Add((dataDeserializer.Id, new EditorDeserializedData(
                    new Dictionary<CustomEventEditorData, ICustomEventCustomData>(),
                    new Dictionary<BasicEventEditorData, IEventCustomData>(),
                    new Dictionary<BaseEditorData, IObjectCustomData>())));
            }

            return result;
        }

        internal void DeserializeBeatmapData(
            bool v2,
            bool leftHanded,
            out Dictionary<string, Track> beatmapTracks,
            out HashSet<(object Id, EditorDeserializedData DeserializedData)> deserializedDatas)
        {
            _log.Trace("Deserializing BeatmapData");

            if (v2)
            {
                _log.Trace("BeatmapData is v2, converting...");
            }

            var baseObjectDatas = _objectDataModel.allBeatmapObjects.Cast<BaseEditorData>();
            var basicEventDatas = _eventsDataModel.GetAllEventsAsList().Cast<BaseEditorData>();

            // tracks are built based off the untransformed beatmapdata so modifiers like "no walls" do not prevent track creation
            TrackBuilder trackManager = new();
            foreach (BaseEditorData baseEditorData in baseObjectDatas.Concat(basicEventDatas).Concat(CustomDataRepository.GetCustomEvents()))
            {
                CustomData customData = CustomDataRepository.GetCustomData(baseEditorData);

                // for epic tracks thing
                object? trackNameRaw = customData.Get<object>(v2 ? Constants.V2_TRACK : Constants.TRACK);
                if (trackNameRaw == null)
                {
                    continue;
                }

                IEnumerable<string> trackNames;
                if (trackNameRaw is List<object> listTrack)
                {
                    trackNames = listTrack.Cast<string>();
                }
                else
                {
                    trackNames = new[] { (string)trackNameRaw };
                }

                foreach (string trackName in trackNames)
                {
                    trackManager.AddTrack(trackName);
                }
            }

            // Point definitions
            Dictionary<string, List<object>> pointDefinitions = new();

            if (v2)
            {
                IEnumerable<CustomData>? pointDefinitionsRaw =
                    CustomDataRepository.GetCustomBeatmapSaveData().customData.Get<List<object>>(Constants.V2_POINT_DEFINITIONS)?.Cast<CustomData>();
                if (pointDefinitionsRaw != null)
                {
                    foreach (CustomData pointDefintionRaw in pointDefinitionsRaw)
                    {
                        string pointName = pointDefintionRaw.GetRequired<string>(Constants.V2_NAME);
                        AddPoint(pointName, pointDefintionRaw.GetRequired<List<object>>(Constants.V2_POINTS));
                    }
                }
            }
            else
            {
                CustomData? pointDefinitionsRaw = CustomDataRepository.GetCustomBeatmapSaveData().customData.Get<CustomData>(Constants.POINT_DEFINITIONS);
                if (pointDefinitionsRaw != null)
                {
                    foreach ((string key, object? value) in pointDefinitionsRaw)
                    {
                        if (value == null)
                        {
                            throw new InvalidOperationException($"[{key}] was null.");
                        }

                        AddPoint(key, (List<object>)value);
                    }
                }
            }

            // Event definitions
            Dictionary<string, CustomEventData> eventDefinitions = new();

            if (!v2)
            {
                IEnumerable<CustomData>? eventDefinitionsRaw =
                    CustomDataRepository.GetCustomBeatmapSaveData().customData.Get<List<object>>(Constants.EVENT_DEFINITIONS)?.Cast<CustomData>();
                if (eventDefinitionsRaw != null)
                {
                    foreach (CustomData eventDefinitionRaw in eventDefinitionsRaw)
                    {
                        string eventName = eventDefinitionRaw.GetRequired<string>(Constants.NAME);
                        string type = eventDefinitionRaw.GetRequired<string>(Constants.TYPE);
                        CustomData data = eventDefinitionRaw.GetRequired<CustomData>("data");

                        if (!eventDefinitions.ContainsKey(eventName))
                        {
                            eventDefinitions.Add(eventName, new CustomEventData(-1, type, data, null));
                        }
                        else
                        {
                            _log.Error($"Duplicate event defintion name, {eventName} could not be registered");
                        }
                    }
                }
            }

            // new deserialize stuff should make these unnecessary
            ////customBeatmapData.customData["tracks"] = trackManager.Tracks;
            ////customBeatmapData.customData["pointDefinitions"] = pointDefinitions;
            ////customBeatmapData.customData["eventDefinitions"] = eventDefinitions;

            // Currently used by Chroma.GameObjectTrackController
            beatmapTracks = trackManager.Tracks;

            float bpm = PopulateBeatmap._beatmapLevelDataModel.beatsPerMinute;

            object[] inputs =
            {
                _objectDataModel,
                _eventsDataModel,
                CustomDataRepository.GetBeatmapData(),
                trackManager,
                pointDefinitions,
                trackManager.Tracks,
                v2,
                bpm
            };

            EditorDataDeserializer[] deserializers = _customDataDeserializers.Where(n => n.Enabled).ToArray();

            foreach (EditorDataDeserializer deserializer in deserializers)
            {
                deserializer.Create(inputs);
            }

            deserializedDatas = new HashSet<(object? Id, EditorDeserializedData DeserializedData)>(deserializers.Length);
            foreach (EditorDataDeserializer deserializer in deserializers)
            {
                _log.Trace($"Binding [{deserializer.Id}]");

                deserializedDatas.Add((deserializer.Id, deserializer.Deserialize()));
            }

            return;

            void AddPoint(string pointDataName, List<object> pointData)
            {
                if (!pointDefinitions.ContainsKey(pointDataName))
                {
                    pointDefinitions.Add(pointDataName, pointData);
                }
                else
                {
                    _log.Error($"Duplicate point defintion name, {pointDataName} could not be registered");
                }
            }
        }
    }
}
