using BeatmapEditor3D.DataModels;
using BetterEditor.CustomJSONData;
using BetterEditor.CustomJSONData.CustomEvents;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Zenject;

namespace BetterEditor.Heck.Deserializer
{
    public class EditorDeserializerManager
    {
        private IBeatmapLevelDataModel _objectDataModel;
        private IBeatmapEventsDataModel _eventsDataModel;

        [Inject]
        private EditorDeserializerManager(IBeatmapLevelDataModel objectDataModel, IBeatmapEventsDataModel eventsDataModel)
        {
            _objectDataModel = objectDataModel;
            _eventsDataModel = eventsDataModel;
        }

        public static EditorDataDeserializer Register<T>(object id)
        {
            EditorDataDeserializer deserializer = new EditorDataDeserializer(id, typeof(T));
            _customEditorDataDeserializers.Add(deserializer);
            return deserializer;
        }

        internal void DeserializeBeatmapData(
            bool v2,
            bool leftHanded,
            out Dictionary<string, Track> beatmapTracks,
            out HashSet<(object Id, EditorDeserializedData DeserializedData)> deserializedDatas)
        {
            Plugin.Log.Info("Deserializing BeatmapData");

            if (v2)
            {
                Plugin.Log.Trace("BeatmapData is v2, converting...");
            }

            var baseObjectDatas = (_objectDataModel as BeatmapLevelDataModel).allBeatmapObjects.Cast<BaseEditorData>();
            var basicEventDatas = (_eventsDataModel as BeatmapBasicEventsDataModel).GetAllEventsAsList().Cast<BaseEditorData>();

            // tracks are built based off the untransformed beatmapdata so modifiers like "no walls" do not prevent track creation
            TrackBuilder trackManager = new TrackBuilder();
            foreach (BaseEditorData baseEditorData in baseObjectDatas.Concat(basicEventDatas).Concat(CustomDataRepository.GetCustomEvents()))
            {
                CustomData customData = CustomDataRepository.GetCustomData(baseEditorData);

                // for epic tracks thing
                object trackNameRaw = customData.Get<object>(v2 ? Constants.V2_TRACK : Constants.TRACK);
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
            var pointDefinitions = new Dictionary<string, List<object>>();

            if (v2)
            {
                IEnumerable<CustomData> pointDefinitionsRaw =
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
                CustomData pointDefinitionsRaw = CustomDataRepository.GetCustomBeatmapSaveData().customData.Get<CustomData>(Constants.POINT_DEFINITIONS);
                if (pointDefinitionsRaw != null)
                {
                    foreach ((string key, object value) in pointDefinitionsRaw)
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
            /*var eventDefinitions = new Dictionary<string, CustomEventData>();

			if (!v2)
			{
				IEnumerable<CustomData> eventDefinitionsRaw =
					CustomDataRepository.GetBeatmapCustomData().Get<List<object>>(Constants.EVENT_DEFINITIONS)?.Cast<CustomData>();
				if (eventDefinitionsRaw != null)
				{
					foreach (CustomData eventDefinitionRaw in eventDefinitionsRaw)
					{
						string eventName = eventDefinitionRaw.GetRequired<string>(Constants.NAME);
						string type = eventDefinitionRaw.GetRequired<string>(Constants.TYPE);
						CustomData data = eventDefinitionRaw.GetRequired<CustomData>("data");

						if (!eventDefinitions.ContainsKey(eventName))
						{
							eventDefinitions.Add(eventName, new CustomEventData(-1, type, data, v2));
						}
						else
						{
							Plugin.Log.Error($"Duplicate event defintion name, {eventName} could not be registered");
						}
					}
				}
			}*/

            // new deserialize stuff should make these unnecessary
            ////customBeatmapData.customData["tracks"] = trackManager.Tracks;
            ////customBeatmapData.customData["pointDefinitions"] = pointDefinitions;
            ////customBeatmapData.customData["eventDefinitions"] = eventDefinitions;

            // Currently used by Chroma.GameObjectTrackController
            beatmapTracks = trackManager.Tracks;

            object[] inputs =
            {
                _objectDataModel,
                _eventsDataModel,
                CustomDataRepository.GetCustomLivePreviewBeatmapData(),
                trackManager,
                pointDefinitions,
                trackManager.Tracks,
                v2
            };

            EditorDataDeserializer[] deserializers = _customEditorDataDeserializers.Where(n => n.Enabled).ToArray();

            foreach (EditorDataDeserializer deserializer in deserializers)
            {
                deserializer.InjectedInvokeEarly(inputs);
            }

            deserializedDatas = new HashSet<(object Id, EditorDeserializedData)>(deserializers.Length);
            foreach (EditorDataDeserializer deserializer in deserializers)
            {
                float customEventTime;
                float eventTime;
                float objectTime;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Dictionary<CustomEventEditorData, ICustomEventCustomData> customEventCustomDatas = deserializer.InjectedInvokeCustomEvent(inputs);
                stopwatch.Stop();
                customEventTime = stopwatch.ElapsedMilliseconds;

                stopwatch.Restart();
                Dictionary<BasicEventEditorData, IEventCustomData> eventCustomDatas = deserializer.InjectedInvokeEvent(inputs);
                stopwatch.Stop();
                eventTime = stopwatch.ElapsedMilliseconds;

                stopwatch.Restart();
                Dictionary<BaseEditorData, IObjectCustomData> objectCustomDatas = deserializer.InjectedInvokeObject(inputs);
                stopwatch.Stop();
                objectTime = stopwatch.ElapsedMilliseconds;

                Plugin.Log.Info($"Binding [{deserializer.Id}] Time: {customEventTime}ms(custom event) {eventTime}ms(custom event) {objectTime}ms(custom event)");

                deserializedDatas.Add((deserializer.Id, new EditorDeserializedData(customEventCustomDatas, eventCustomDatas, objectCustomDatas)));
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
                    Plugin.Log.Error($"Duplicate point defintion name, {pointDataName} could not be registered");
                }
            }
        }

        private static readonly HashSet<EditorDataDeserializer> _customEditorDataDeserializers = new HashSet<EditorDataDeserializer>();
    }
}
