using BeatmapEditor3D.DataModels;
using Chroma;
using CustomJSONData.CustomBeatmap;
using EditorEX.Chroma.Events;
using EditorEX.Chroma.Lighting;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.CustomJSONData.Util;
using EditorEX.Heck.Deserialize;
using Heck.Animation;
using Heck.Deserialize;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EditorEX.Chroma.Constants;

namespace EditorEX.Chroma.Deserializer
{
    internal class EditorChromaCustomDataDeserializer : IEditorEarlyDeserializer, IEditorObjectsDeserializer, IEditorCustomEventsDeserializer, IEditorEventsDeserializer
    {
        private readonly BeatmapBasicEventsDataModel _beatmapBasicEventsDataModel;
        private readonly BeatmapObjectsDataModel _beatmapObjectsDataModel;
        private readonly Dictionary<string, Track> _tracks;
        private readonly Dictionary<string, List<object>> _pointDefinitions;
        private readonly TrackBuilder _trackBuilder;
        private readonly bool _v2;

        private EditorChromaCustomDataDeserializer(
            BeatmapBasicEventsDataModel beatmapBasicEventsDataModel,
            BeatmapObjectsDataModel beatmapObjectsDataModel,
            Dictionary<string, Track> beatmapTracks,
            Dictionary<string, List<object>> pointDefinitions,
            TrackBuilder trackBuilder,
            bool v2)
        {
            _beatmapBasicEventsDataModel = beatmapBasicEventsDataModel;
            _beatmapObjectsDataModel = beatmapObjectsDataModel;
            _tracks = beatmapTracks;
            _pointDefinitions = pointDefinitions;
            _trackBuilder = trackBuilder;
            _v2 = v2;
        }

        public void DeserializeEarly()
        {
            var beatmapData = CustomDataRepository.GetBeatmapData().customData;
            IEnumerable<CustomData>? environmentData = beatmapData.Get<List<object>>(_v2 ? V2_ENVIRONMENT : ENVIRONMENT)?.Cast<CustomData>();
            if (environmentData != null)
            {
                foreach (CustomData gameObjectData in environmentData)
                {
                    _trackBuilder.AddManyFromCustomData(gameObjectData, _v2, false);

                    CustomData? geometryData = gameObjectData.Get<CustomData?>(_v2 ? V2_GEOMETRY : GEOMETRY);
                    object? materialData = geometryData?.Get<object?>(_v2 ? V2_MATERIAL : MATERIAL);
                    if (materialData is CustomData materialCustomData)
                    {
                        _trackBuilder.AddFromCustomData(materialCustomData, _v2, false);
                    }
                }
            }

            CustomData? materialsData = beatmapData.Get<CustomData>(_v2 ? V2_MATERIALS : MATERIALS);
            if (materialsData != null)
            {
                foreach ((string _, object? value) in materialsData)
                {
                    if (value == null)
                    {
                        continue;
                    }

                    _trackBuilder.AddFromCustomData((CustomData)value, _v2, false);
                }
            }

            if (!_v2)
            {
                return;
            }

            foreach (CustomEventEditorData customEventData in CustomDataRepository.GetCustomEvents())
            {
                try
                {
                    switch (customEventData.eventType)
                    {
                        case ASSIGN_FOG_TRACK:
                            _trackBuilder.AddFromCustomData(customEventData.customData, _v2);
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e);
                }
            }
        }

        public Dictionary<CustomEventEditorData, ICustomEventCustomData> DeserializeCustomEvents()
        {
            var dictionary = new Dictionary<CustomEventEditorData, ICustomEventCustomData>();
            foreach (CustomEventEditorData customEventData in CustomDataRepository.GetCustomEvents())
            {
                bool v2 = customEventData.version2_6_0AndEarlier;
                try
                {
                    ICustomEventCustomData chromaCustomEventData;

                    switch (customEventData.eventType)
                    {
                        case ASSIGN_FOG_TRACK:
                            if (!v2)
                            {
                                continue;
                            }

                            chromaCustomEventData = new ChromaAssignFogEventData(customEventData.customData.GetTrack(_tracks, v2));
                            break;

                        case ANIMATE_COMPONENT:
                            if (v2)
                            {
                                continue;
                            }

                            chromaCustomEventData = new ChromaAnimateComponentData(customEventData.customData, _tracks, _pointDefinitions);
                            break;

                        default:
                            continue;
                    }

                    dictionary.Add(customEventData, chromaCustomEventData);
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e);
                }
            }

            return dictionary;
        }

        //TODO: Arcs
        public Dictionary<BaseEditorData, IObjectCustomData> DeserializeObjects()
        {
            var dictionary = new Dictionary<BaseEditorData, IObjectCustomData>();

            foreach (BaseBeatmapObjectEditorData beatmapObjectData in _beatmapObjectsDataModel.allBeatmapObjects)
            {
                if (dictionary.ContainsKey(beatmapObjectData)) continue;
                try
                {
                    CustomData customData = beatmapObjectData.GetCustomData();
                    if (customData == null)
                    {
                        Plugin.Log.Warn("Chroma | customData is null...");
                        continue;
                    }
                    switch (beatmapObjectData)
                    {
                        case NoteEditorData noteData:
                            dictionary.Add(beatmapObjectData, new ChromaNoteData(customData, _tracks, _pointDefinitions, _v2));
                            break;

                        case ChainEditorData sliderData:
                            dictionary.Add(beatmapObjectData, new ChromaNoteData(customData, _tracks, _pointDefinitions, _v2));
                            break;

                        case ArcEditorData arcData:
                            dictionary.Add(beatmapObjectData, new ChromaNoteData(customData, _tracks, _pointDefinitions, _v2));
                            break;

                        case ObstacleEditorData obstacleData:
                            dictionary.Add(beatmapObjectData, new ChromaObjectData(customData, _tracks, _pointDefinitions, _v2));
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e);
                }
            }

            return dictionary;
        }

        public Dictionary<BasicEventEditorData, IEventCustomData> DeserializeEvents()
        {
            List<BasicEventEditorData> beatmapEventDatas = _beatmapBasicEventsDataModel.GetAllEventsAsList().ToList();

            EditorLegacyLightHelper legacyLightHelper = null;
            if (_v2)
            {
                legacyLightHelper = new EditorLegacyLightHelper(beatmapEventDatas);
            }

            var dictionary = new Dictionary<BasicEventEditorData, IEventCustomData>();
            foreach (BasicEventEditorData beatmapEventData in beatmapEventDatas)
            {

                try
                {
                    dictionary.Add(beatmapEventData, new EditorChromaEventData(beatmapEventData, legacyLightHelper, _v2));
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e);
                }
            }

            // Horrible stupid logic to get next same type event per light id
            // what am i even doing anymore
            var allNextSameTypes = new Dictionary<int, Dictionary<int, BasicEventEditorData>>();
            for (int i = beatmapEventDatas.Count - 1; i >= 0; i--)
            {
                var beatmapEventData = beatmapEventDatas[i];
                if (!(beatmapEventData is BasicEventEditorData basicBeatmapEventData)) continue;
                if (!TryGetEventData(beatmapEventDatas[i], out EditorChromaEventData currentEventData))
                {
                    continue;
                }

                int type = (int)beatmapEventData.type;
                if (!allNextSameTypes.TryGetValue(
                        type,
                        out Dictionary<int, BasicEventEditorData> nextSameTypes))
                {
                    allNextSameTypes[type] = nextSameTypes = new Dictionary<int, BasicEventEditorData>();
                }

                currentEventData.NextSameTypeEvent = currentEventData.NextSameTypeEvent ?? new Dictionary<int, BasicEventEditorData>(nextSameTypes);
                IEnumerable<int> ids = currentEventData.LightID;
                if (ids == null)
                {
                    nextSameTypes[-1] = basicBeatmapEventData;
                    foreach (int key in nextSameTypes.Keys.ToArray())
                    {
                        nextSameTypes[key] = basicBeatmapEventData;
                    }
                }
                else
                {
                    foreach (int id in ids)
                    {
                        nextSameTypes[id] = basicBeatmapEventData;
                    }
                }
            }

            return dictionary;

            bool TryGetEventData(BasicEventEditorData beatmapEventData, out EditorChromaEventData chromaEventData)
            {
                if (dictionary.TryGetValue(beatmapEventData, out IEventCustomData eventCustomData))
                {
                    chromaEventData = (EditorChromaEventData)eventCustomData;
                    return true;
                }

                chromaEventData = null;
                return false;
            }
        }
    }
}
