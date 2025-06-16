using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Heck.Deserialize;
using Heck;
using Heck.Event;
using Vivify;
using Vivify.Managers;
using Vivify.TrackGameObject;
using Zenject;
using static Vivify.VivifyController;

// Based from https://github.com/Aeroluna/Vivify
namespace EditorEX.Vivify.Events
{
    [CustomEvent(SET_CAMERA_PROPERTY)]
    internal class EditorSetCameraProperty : ICustomEvent
    {
        private readonly CameraPropertyManager _cameraPropertyManager;
        private readonly EditorDeserializedData _deserializedData;

        private EditorSetCameraProperty(
            CameraPropertyManager cameraPropertyManager,
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData
        )
        {
            _cameraPropertyManager = cameraPropertyManager;
            _deserializedData = deserializedData;
        }

        public void Callback(CustomEventData customEventData)
        {
            if (
                !_deserializedData.Resolve(
                    CustomDataRepository.GetCustomEventConversion(customEventData),
                    out SetCameraPropertyData? eventData
                )
            )
            {
                return;
            }

            SetCameraProperties(eventData.Id, eventData.Property);
        }

        public void SetCameraProperties(string id, CameraProperty property)
        {
            if (
                !_cameraPropertyManager.Properties.TryGetValue(
                    id,
                    out CameraPropertyManager.CameraProperties properties
                )
            )
            {
                _cameraPropertyManager.Properties[id] = properties =
                    new CameraPropertyManager.CameraProperties();
            }

            if (property.HasDepthTextureMode)
            {
                properties.DepthTextureMode = property.DepthTextureMode;
            }

            if (property.HasClearFlags)
            {
                properties.ClearFlags = property.ClearFlags;
            }

            if (property.HasBackgroundColor)
            {
                properties.BackgroundColor = property.BackgroundColor;
            }

            if (property.HasCulling)
            {
                CameraProperty.CullingData? cullingData = property.Culling;
                properties.CullingTextureData =
                    cullingData != null
                        ? new CullingTextureTracker(cullingData.Tracks, cullingData.Whitelist)
                        : null;
            }

            if (property.HasBloomPrePass)
            {
                properties.BloomPrePass = property.BloomPrePass;
            }

            if (property.HasMainEffect)
            {
                properties.MainEffect = property.MainEffect;
            }
        }
    }
}
