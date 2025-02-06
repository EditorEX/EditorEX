using BeatmapEditor3D.DataModels;
using Chroma;
using CustomJSONData.CustomBeatmap;
using EditorEX.Chroma.Events;
using EditorEX.Chroma.Lighting;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Heck.Deserialize;
using EditorEX.Util;
using Heck.Animation;
using Heck.Deserialize;
using IPA.Utilities;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Vivify;
using static Vivify.VivifyController;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.Vivify.Deserializer
{
    internal class EditorVivifyCustomDataDeserializer : IEditorEarlyDeserializer, IEditorObjectsDeserializer, IEditorCustomEventsDeserializer
    {
        private readonly SiraLog _siraLog;
        private readonly BeatmapObjectsDataModel _beatmapObjectsDataModel;
        private readonly Dictionary<string, Track> _tracks;
        private readonly Dictionary<string, List<object>> _pointDefinitions;
        private readonly TrackBuilder _trackBuilder;

        private EditorVivifyCustomDataDeserializer(
            SiraLog siraLog,
            BeatmapObjectsDataModel beatmapObjectsDataModel,
            Dictionary<string, Track> beatmapTracks,
            Dictionary<string, List<object>> pointDefinitions,
            TrackBuilder trackBuilder)
        {
            _siraLog = siraLog;
            _beatmapObjectsDataModel = beatmapObjectsDataModel;
            _tracks = beatmapTracks;
            _pointDefinitions = pointDefinitions;
            _trackBuilder = trackBuilder;
        }

        public void DeserializeEarly()
        {
            foreach (CustomEventEditorData customEventData in CustomDataRepository.GetCustomEvents())
            {
                try
                {
                    switch (customEventData.eventType)
                    {
                        case INSTANTIATE_PREFAB:
                            _trackBuilder.AddFromCustomData(customEventData.customData, false, false);
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    _siraLog.Error(e);
                }
            }
        }

        public Dictionary<CustomEventEditorData, ICustomEventCustomData> DeserializeCustomEvents()
        {
            var dictionary = new Dictionary<CustomEventEditorData, ICustomEventCustomData>();
            foreach (CustomEventEditorData customEventData in CustomDataRepository.GetCustomEvents())
            {
                try
                {
                    CustomData data = customEventData.customData;
                    switch (customEventData.eventType)
                    {
                        case APPLY_POST_PROCESSING:
                            dictionary.Add(customEventData, new ApplyPostProcessingData(data, _pointDefinitions));
                            break;

                        case ASSIGN_OBJECT_PREFAB:
                            dictionary.Add(customEventData, new AssignObjectPrefabData(data, _tracks));
                            break;

                        case DECLARE_CULLING_TEXTURE:
                            dictionary.Add(customEventData, new CreateCameraData(data, _tracks));
                            break;

                        case DECLARE_TEXTURE:
                            dictionary.Add(customEventData, new CreateScreenTextureData(data));
                            break;

                        case DESTROY_PREFAB:
                            dictionary.Add(customEventData, new DestroyObjectData(data));
                            break;

                        case INSTANTIATE_PREFAB:
                            dictionary.Add(customEventData, new InstantiatePrefabData(data, _tracks));
                            break;

                        case SET_MATERIAL_PROPERTY:
                            dictionary.Add(customEventData, new SetMaterialPropertyData(data, _pointDefinitions));
                            break;

                        case SET_GLOBAL_PROPERTY:
                            dictionary.Add(customEventData, new SetGlobalPropertyData(data, _pointDefinitions));
                            break;

                        case SET_CAMERA_PROPERTY:
                            dictionary.Add(customEventData, new SetCameraPropertyData(data, _tracks));
                            break;

                        case SET_ANIMATOR_PROPERTY:
                            dictionary.Add(customEventData, new SetAnimatorPropertyData(data, _pointDefinitions));
                            break;

                        case SET_RENDERING_SETTINGS:
                            dictionary.Add(customEventData, new SetRenderingSettingsData(data, _pointDefinitions));
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    _siraLog.Error(e);
                }
            }

            return dictionary;
        }

        public Dictionary<BaseEditorData?, IObjectCustomData> DeserializeObjects()
        {
            var dictionary = new Dictionary<BaseEditorData?, IObjectCustomData>();

            foreach (BaseBeatmapObjectEditorData beatmapObjectData in _beatmapObjectsDataModel.allBeatmapObjects)
            {
                if (dictionary.ContainsKey(beatmapObjectData)) continue;
                try
                {
                    CustomData customData = CustomDataRepository.GetCustomData(beatmapObjectData);
                    dictionary.Add(beatmapObjectData, new VivifyObjectData(customData, _tracks));
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
