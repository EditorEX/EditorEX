using System;
using System.Collections;
using System.Collections.Generic;
using BeatmapEditor3D;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Essentials.Patches;
using EditorEX.Heck.Deserialize;
using EditorEX.Vivify.Managers;
using Heck;
using Heck.Deserialize;
using Heck.Event;
using SiraUtil.Logging;
using UnityEngine;
using Vivify;
using Vivify.Events;
using Vivify.Extras;
using Vivify.HarmonyPatches;
using Vivify.Managers;
using Vivify.PostProcessing;
using Zenject;
using static Vivify.VivifyController;

namespace EditorEX.Vivify.Events
{
    [CustomEvent(APPLY_POST_PROCESSING)]
    internal class EditorApplyPostProcessing : ICustomEvent, ITickable
    {
        private readonly SiraLog _log;
        private readonly EditorAssetBundleManager _assetBundleManager;
        private readonly EditorDeserializedData _deserializedData;
        private readonly IAudioTimeSource _audioTimeSource;
        private readonly IBpmController _bpmController;
        private readonly EditorSetMaterialProperty _setMaterialProperty;
        private readonly CameraEffectApplier _cameraEffectApplier;
        private readonly CoroutineDummy _coroutineDummy;
        private readonly AudioDataModel _audioDataModel;

        private EditorApplyPostProcessing(
            SiraLog log,
            EditorAssetBundleManager assetBundleManager,
            [Inject(Id = ID)] EditorDeserializedData deserializedData,
            IAudioTimeSource audioTimeSource,
            IBpmController bpmController,
            EditorSetMaterialProperty setMaterialProperty,
            CameraEffectApplier cameraEffectApplier,
            CoroutineDummy coroutineDummy,
            PopulateBeatmap populateBeatmap)
        {
            _log = log;
            _assetBundleManager = assetBundleManager;
            _deserializedData = deserializedData;
            _audioTimeSource = audioTimeSource;
            _bpmController = bpmController;
            _setMaterialProperty = setMaterialProperty;
            _cameraEffectApplier = cameraEffectApplier;
            _coroutineDummy = coroutineDummy;
            _audioDataModel = populateBeatmap._audioDataModel;
        }

        public void Callback(CustomEventData customEventData)
        {
            if (!_deserializedData.Resolve(CustomDataRepository.GetCustomEventConversion(customEventData), out ApplyPostProcessingData? data))
            {
                return;
            }

            float duration = (60f * data.Duration) / _bpmController.currentBpm; // Convert to real time;
            string? assetName = data.Asset;
            Material? material = null;
            if (assetName != null)
            {
                if (!_assetBundleManager.TryGetAsset(assetName, out material))
                {
                    return;
                }

                List<MaterialProperty>? properties = data.Properties;
                if (properties != null)
                {
                    _setMaterialProperty.SetMaterialProperties(
                        material,
                        properties,
                        duration,
                        data.Easing,
                        _audioDataModel.bpmData.BeatToSeconds(customEventData.time));
                }
            }

            List<MaterialData> effects = data.Order switch
            {
                PostProcessingOrder.BeforeMainEffect => _cameraEffectApplier.PreEffects,
                PostProcessingOrder.AfterMainEffect => _cameraEffectApplier.PostEffects,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (duration == 0)
            {
                effects.InsertIntoSortedList(
                    new MaterialData(material, data.Priority, data.Source, data.Target, data.Pass, Time.frameCount));
                _log.Debug($"Applied material [{assetName}] for single frame");
                return;
            }

            if (duration <= 0 || _audioTimeSource.songTime > _audioDataModel.bpmData.BeatToSeconds(customEventData.time) + duration)
            {
                return;
            }

            MaterialData materialData = new(material, data.Priority, data.Source, data.Target, data.Pass);
            effects.InsertIntoSortedList(materialData);
            _log.Debug($"Applied material [{assetName}] for [{duration}] seconds for event time [{_audioDataModel.bpmData.BeatToSeconds(customEventData.time)}] at song time [{_audioTimeSource.songTime}]");
            _coroutineDummy.StartCoroutine(
                KillPostProcessingCoroutine(effects, materialData, duration, _audioDataModel.bpmData.BeatToSeconds(customEventData.time)));
        }

        internal IEnumerator KillPostProcessingCoroutine(
            List<MaterialData> effects,
            MaterialData data,
            float duration,
            float startTime)
        {
            while (true)
            {
                float elapsedTime = _audioTimeSource.songTime - startTime;
                if (elapsedTime < 0)
                {
                    break;
                }

                if (elapsedTime < duration)
                {
                    yield return null;
                }
                else
                {
                    effects.Remove(data);
                    break;
                }
            }
        }

        private float _lastBeat = 0f;
        public void Tick()
        {
            var time = _audioTimeSource.songTime;
            if (_lastBeat > time)
            {
                _cameraEffectApplier.Reset();
            }

            _lastBeat = time;
        }
    }
}