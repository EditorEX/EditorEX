using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Heck.Deserialize;
using Heck;
using Heck.Event;
using SiraUtil.Logging;
using Vivify;
using Vivify.HarmonyPatches;
using Vivify.Managers;
using Zenject;
using static Vivify.VivifyController;

// Based from https://github.com/Aeroluna/Vivify
namespace EditorEX.Vivify.Events
{
    [CustomEvent(DESTROY_PREFAB)]
    internal class EditorDestroyPrefab : ICustomEvent
    {
        private readonly EditorDeserializedData _deserializedData;
        private readonly SiraLog _log;
        private readonly PrefabManager _prefabManager;
        private readonly CameraEffectApplier _cameraEffectApplier;

        private EditorDestroyPrefab(
            SiraLog log,
            PrefabManager prefabManager,
            CameraEffectApplier cameraEffectApplier,
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData)
        {
            _log = log;
            _prefabManager = prefabManager;
            _cameraEffectApplier = cameraEffectApplier;
            _deserializedData = deserializedData;
        }

        public void Callback(CustomEventData customEventData)
        {
            if (!_deserializedData.Resolve(CustomDataRepository.GetCustomEventConversion(customEventData), out DestroyObjectData? data))
            {
                return;
            }

            string[] names = data.Id;
            foreach (string name in names)
            {
                if (_cameraEffectApplier.CameraDatas.Remove(name))
                {
                    _log.Debug($"Destroyed camera [{name}]");
                }
                else if (_cameraEffectApplier.DeclaredTextureDatas.Remove(name))
                {
                    _log.Debug($"Destroyed screen texture [{name}]");
                }
                else if (_prefabManager.Destroy(name))
                {
                    _log.Debug($"Destroyed prefab [{name}]");
                }
                else
                {
                    _log.Error($"Could not find [{name}]");
                }
            }
        }
    }
}