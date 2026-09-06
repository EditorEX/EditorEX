using System.Collections.Generic;
using Vivify;
using Vivify.Managers;
using Vivify.TrackGameObject;

namespace EditorEX.Vivify.Events
{
    internal class EditorSetCameraProperty
    {
        private readonly CameraPropertyManager _cameraPropertyManager;

        private EditorSetCameraProperty(CameraPropertyManager cameraPropertyManager)
        {
            _cameraPropertyManager = cameraPropertyManager;
        }

        public void SetCameraProperties(string id, CameraProperty property)
        {
            CameraPropertyManager.CameraProperties properties = PropertiesFor(id);

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

        internal void ClearChannels(string id, IReadOnlyList<string> channels)
        {
            if (
                !_cameraPropertyManager.Properties.TryGetValue(
                    id,
                    out CameraPropertyManager.CameraProperties properties
                )
            )
            {
                return;
            }

            for (int i = 0; i < channels.Count; i++)
            {
                switch (channels[i])
                {
                    case "depthTextureMode":
                        properties.DepthTextureMode = null;
                        break;
                    case "clearFlags":
                        properties.ClearFlags = null;
                        break;
                    case "backgroundColor":
                        properties.BackgroundColor = null;
                        break;
                    case "culling":
                        properties.CullingTextureData = null;
                        break;
                    case "bloomPrePass":
                        properties.BloomPrePass = null;
                        break;
                    case "mainEffect":
                        properties.MainEffect = null;
                        break;
                }
            }
        }

        private CameraPropertyManager.CameraProperties PropertiesFor(string id)
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

            return properties;
        }
    }
}
