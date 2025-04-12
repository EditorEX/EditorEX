using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Heck.Deserialize;
using Heck;
using Heck.Deserialize;
using Heck.Event;
using SiraUtil.Logging;
using Vivify;
using Vivify.HarmonyPatches;
using Zenject;
using static Vivify.VivifyController;

// Based from https://github.com/Aeroluna/Vivify
namespace EditorEX.Vivify.Events
{
    [CustomEvent(DECLARE_TEXTURE)]
    internal class EditorDeclareRenderTexture : ICustomEvent
    {
        private readonly SiraLog _log;
        private readonly EditorDeserializedData _deserializedData;
        private readonly CameraEffectApplier _cameraEffectApplier;

        private EditorDeclareRenderTexture(
            SiraLog log,
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData,
            CameraEffectApplier cameraEffectApplier)
        {
            _log = log;
            _deserializedData = deserializedData;
            _cameraEffectApplier = cameraEffectApplier;
        }

        public void Callback(CustomEventData customEventData)
        {
            if (!_deserializedData.Resolve(CustomDataRepository.GetCustomEventConversion(customEventData), out CreateScreenTextureData? data))
            {
                return;
            }

            _cameraEffectApplier.DeclaredTextureDatas.Add(data.Name, data);
            _log.Debug($"Created texture [{data.Name}]");
        }
    }
}