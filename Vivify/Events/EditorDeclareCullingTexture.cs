using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Heck.Deserialize;
using Heck;
using Heck.Event;
using SiraUtil.Logging;
using Vivify;
using Vivify.HarmonyPatches;
using Zenject;
using static Vivify.VivifyController;

// Based from https://github.com/Aeroluna/Vivify
namespace EditorEX.Vivify.Events
{
    [CustomEvent(DECLARE_CULLING_TEXTURE)]
    internal class EditorDeclareCullingTexture : ICustomEvent
    {
        private readonly SiraLog _log;
        private readonly EditorSetCameraProperty _setCameraProperty;
        private readonly CameraEffectApplier _cameraEffectApplier;
        private readonly EditorDeserializedData _deserializedData;

        private EditorDeclareCullingTexture(
            SiraLog log,
            EditorSetCameraProperty setCameraProperty,
            CameraEffectApplier cameraEffectApplier,
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData)
        {
            _log = log;
            _setCameraProperty = setCameraProperty;
            _cameraEffectApplier = cameraEffectApplier;
            _deserializedData = deserializedData;
        }

        public void Callback(CustomEventData customEventData)
        {
            if (!_deserializedData.Resolve(CustomDataRepository.GetCustomEventConversion(customEventData), out CreateCameraData? data))
            {
                return;
            }

            string name = data.Name;
            _cameraEffectApplier.CameraDatas.Add(name, data);
            _log.Debug($"Created camera [{name}]");

            if (data.Property != null)
            {
                _setCameraProperty.SetCameraProperties(name, data.Property);
            }

            /*
                GameObject[] gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                List<int> layers = new List<int>();
                gameObjects.Select(n => n.layer).ToList().ForEach(n =>
                {
                    if (!layers.Contains(n))
                    {
                        layers.Add(n);
                    }
                });
                layers.Sort();
                Plugin.Logger.Log($"used layers: {string.Join(", ", layers)}");*/
        }
    }
}