using System.Collections.Generic;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Deserialize;
using EditorEX.Heck.Events;
using EditorEX.Vivify.Managers;
using UnityEngine;
using Vivify;
using Vivify.Extras;
using Vivify.HarmonyPatches;
using Vivify.PostProcessing;
using Zenject;
using static Vivify.VivifyController;

namespace EditorEX.Vivify.Events
{
    internal sealed class VivifyPostProcessingPreviewSource : IPreviewStateSource
    {
        private readonly EditorDeserializedData? _editorDeserializedData;
        private readonly ICustomDataRepository _customDataRepository;
        private readonly EditorAssetBundleManager _assetBundleManager;
        private readonly EditorSetMaterialProperty _setMaterialProperty;
        private readonly CameraEffectApplier _cameraEffectApplier;

        private VivifyPostProcessingPreviewSource(
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData,
            ICustomDataRepository customDataRepository,
            EditorAssetBundleManager assetBundleManager,
            EditorSetMaterialProperty setMaterialProperty,
            CameraEffectApplier cameraEffectApplier
        )
        {
            _editorDeserializedData = deserializedData;
            _customDataRepository = customDataRepository;
            _assetBundleManager = assetBundleManager;
            _setMaterialProperty = setMaterialProperty;
            _cameraEffectApplier = cameraEffectApplier;
        }

        public void Build(IPreviewStateRegistry registry)
        {
            if (_editorDeserializedData == null)
            {
                return;
            }

            foreach (CustomEventEditorData customEvent in _customDataRepository.GetCustomEvents())
            {
                if (customEvent.eventType != APPLY_POST_PROCESSING)
                {
                    continue;
                }

                if (
                    !_editorDeserializedData.Resolve(customEvent, out ApplyPostProcessingData? data)
                    || data == null
                    || !VivifyPreviewOwnership.TryPostProcessingExclusiveEnd(
                        customEvent.beat,
                        data.Duration,
                        out float toBeat
                    )
                )
                {
                    continue;
                }

                Material? material = null;
                if (data.Asset != null)
                {
                    if (!_assetBundleManager.TryGetAsset(data.Asset, out material))
                    {
                        continue;
                    }
                }

                MaterialData materialData = new(
                    material,
                    data.Priority,
                    data.Source,
                    data.Target,
                    data.Pass
                );
                List<MaterialData> effects = _cameraEffectApplier.Effects[data.Order];
                float fromBeat = customEvent.beat;
                List<MaterialProperty>? properties = data.Properties;
                registry.Add(
                    fromBeat,
                    toBeat,
                    new VivifyResourcePreviewAction(
                        () => effects.InsertIntoSortedList(materialData),
                        () => effects.Remove(materialData),
                        material != null && properties != null
                            ? beat =>
                            {
                                float progress = HeckTrackPreviewSampler.EasedProgress(
                                    beat,
                                    fromBeat,
                                    data.Duration,
                                    repeat: 0,
                                    data.Easing,
                                    out _
                                );
                                _setMaterialProperty.ApplyAtProgress(
                                    material,
                                    properties,
                                    progress
                                );
                            }
                            : null
                    )
                );
            }
        }
    }
}
