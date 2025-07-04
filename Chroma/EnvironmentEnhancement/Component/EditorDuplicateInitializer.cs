﻿using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.EnvironmentEnhancement;
using Chroma.EnvironmentEnhancement.Component;
using Chroma.HarmonyPatches.EnvironmentComponent;
using EditorEX.Chroma.HarmonyPatches.Colorizer.Initialize;
using HarmonyLib;
using Heck.Animation.Transform;
using IPA.Utilities;
using SiraUtil.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.Chroma.EnvironmentEnhancement.Component
{
    internal class EditorDuplicateInitializer
    {
        private static readonly FieldAccessor<
            Spectrogram,
            BasicSpectrogramData
        >.Accessor _spectrogramDataAccessor = FieldAccessor<
            Spectrogram,
            BasicSpectrogramData
        >.GetAccessor(nameof(Spectrogram._spectrogramData));
        private static readonly FieldAccessor<
            LightRotationEventEffect,
            BeatmapCallbacksController
        >.Accessor _lightCallbackControllerAccessor = FieldAccessor<
            LightRotationEventEffect,
            BeatmapCallbacksController
        >.GetAccessor(nameof(LightRotationEventEffect._beatmapCallbacksController));
        private static readonly FieldAccessor<
            LightPairRotationEventEffect,
            BeatmapCallbacksController
        >.Accessor _lightPairCallbackControllerAccessor = FieldAccessor<
            LightPairRotationEventEffect,
            BeatmapCallbacksController
        >.GetAccessor(nameof(LightPairRotationEventEffect._beatmapCallbacksController));
        private static readonly FieldAccessor<
            LightPairRotationEventEffect,
            IAudioTimeSource
        >.Accessor _audioTimeSourceAccessor = FieldAccessor<
            LightPairRotationEventEffect,
            IAudioTimeSource
        >.GetAccessor(nameof(LightPairRotationEventEffect._audioTimeSource));
        private static readonly FieldAccessor<
            ParticleSystemEventEffect,
            BeatmapCallbacksController
        >.Accessor _particleCallbackControllerAccessor = FieldAccessor<
            ParticleSystemEventEffect,
            BeatmapCallbacksController
        >.GetAccessor(nameof(ParticleSystemEventEffect._beatmapCallbacksController));
        private static readonly FieldAccessor<
            TrackLaneRingsRotationEffectSpawner,
            BeatmapCallbacksController
        >.Accessor _rotationEffectSpawnerCallbackControllerAccessor = FieldAccessor<
            TrackLaneRingsRotationEffectSpawner,
            BeatmapCallbacksController
        >.GetAccessor(nameof(TrackLaneRingsRotationEffectSpawner._beatmapCallbacksController));

        private readonly SiraLog _log;
        private readonly TrackLaneRingOffset _trackLaneRingOffset;
        private readonly EditorLightWithIdRegisterer _lightWithIdRegisterer;
        private readonly global::Chroma.Settings.Config _config;

        private readonly HashSet<TrackLaneRingsManager> _trackLaneRingsManagers;

        private EditorDuplicateInitializer(
            SiraLog log,
            TrackLaneRingOffset trackLaneRingOffset,
            EditorLightWithIdRegisterer lightWithIdRegisterer,
            global::Chroma.Settings.Config config
        )
        {
            _log = log;
            _trackLaneRingOffset = trackLaneRingOffset;
            _lightWithIdRegisterer = lightWithIdRegisterer;
            _config = config;
            _trackLaneRingsManagers = Resources
                .FindObjectsOfTypeAll<TrackLaneRingsManager>()
                .ToHashSet();
        }

        internal static void PrefillComponentsData(
            Transform root,
            List<IComponentData> componentDatas
        )
        {
            TrackLaneRingsManager trackLaneRingsManager =
                root.GetComponent<TrackLaneRingsManager>();
            if (trackLaneRingsManager != null)
            {
                componentDatas.Add(
                    new TrackLaneRingsManagerComponentData
                    {
                        OldTrackLaneRingsManager = trackLaneRingsManager,
                    }
                );
            }

            foreach (Transform transform in root)
            {
                PrefillComponentsData(transform, componentDatas);
            }
        }

        internal void PostfillComponentsData(
            Transform root,
            Transform original,
            List<IComponentData> componentDatas
        )
        {
            TrackLaneRingsManager trackLaneRingsManager =
                root.GetComponent<TrackLaneRingsManager>();
            if (trackLaneRingsManager != null)
            {
                _trackLaneRingsManagers.Add(trackLaneRingsManager);
                TrackLaneRingsManager originalManager =
                    original.GetComponent<TrackLaneRingsManager>();
                foreach (
                    TrackLaneRingsManagerComponentData componentData in componentDatas
                        .OfType<TrackLaneRingsManagerComponentData>()
                        .Where(n => n.OldTrackLaneRingsManager == originalManager)
                )
                {
                    componentData.NewTrackLaneRingsManager = trackLaneRingsManager;
                }
            }

            TrackLaneRingsRotationEffect rotationEffect =
                root.GetComponent<TrackLaneRingsRotationEffect>();
            if (rotationEffect != null)
            {
                Object.Destroy(rotationEffect);
            }

            foreach (Transform transform in root)
            {
                int index = transform.GetSiblingIndex();
                PostfillComponentsData(transform, original.GetChild(index), componentDatas);
            }
        }

        internal void InitializeComponents(
            Transform root,
            Transform original,
            List<GameObjectInfo> gameObjectInfos,
            List<IComponentData> componentDatas
        )
        {
            TransformController transformController = root.GetComponent<TransformController>();
            if (transformController != null)
            {
                Object.DestroyImmediate(transformController);
            }

            GetComponentAndOriginal<LightWithIdMonoBehaviour>(
                (rootComponent, originalComponent) =>
                {
                    rootComponent._lightManager = originalComponent._lightManager;
                    _lightWithIdRegisterer.MarkForTableRegister(rootComponent);
                }
            );

            GetComponentAndOriginal<LightWithIds>(
                (rootComponent, originalComponent) =>
                {
                    rootComponent._lightManager = originalComponent._lightManager;
                    IEnumerable<ILightWithId> lightsWithId = rootComponent._lightWithIds;
                    foreach (ILightWithId light in lightsWithId)
                    {
                        _lightWithIdRegisterer.MarkForTableRegister(light);
                    }
                }
            );

            GetComponentAndOriginal<TrackLaneRing>(
                (rootComponent, originalComponent) =>
                {
                    _trackLaneRingOffset.CopyRing(originalComponent, rootComponent);

                    rootComponent._transform = root;
                    rootComponent._positionOffset = originalComponent._positionOffset;
                    rootComponent._posZ = originalComponent._posZ;

                    TrackLaneRingsManager? managerToAdd = null;
                    foreach (TrackLaneRingsManager manager in _trackLaneRingsManagers)
                    {
                        TrackLaneRingsManagerComponentData? componentData = componentDatas
                            .OfType<TrackLaneRingsManagerComponentData>()
                            .FirstOrDefault(n => n.OldTrackLaneRingsManager == manager);
                        if (componentData != null)
                        {
                            managerToAdd = componentData.NewTrackLaneRingsManager;
                        }
                        else
                        {
                            TrackLaneRing[] rings = manager._rings;
                            if (rings.Contains(originalComponent))
                            {
                                managerToAdd = manager;
                            }
                        }

                        // ReSharper disable once InvertIf
                        if (managerToAdd != null)
                        {
                            managerToAdd._rings = managerToAdd._rings.AddToArray(rootComponent);

                            break;
                        }
                    }
                }
            );

            GetComponentAndOriginal<TrackLaneRingsPositionStepEffectSpawner>(
                (rootComponent, _) =>
                {
                    foreach (TrackLaneRingsManager manager in _trackLaneRingsManagers)
                    {
                        TrackLaneRingsManagerComponentData? componentData = componentDatas
                            .OfType<TrackLaneRingsManagerComponentData>()
                            .FirstOrDefault(n => n.OldTrackLaneRingsManager == manager);
                        if (componentData == null)
                        {
                            continue;
                        }

                        rootComponent._trackLaneRingsManager =
                            componentData.NewTrackLaneRingsManager!;

                        break;
                    }
                }
            );

            GetComponentAndOriginal<TrackLaneRingsRotationEffectSpawner>(
                (rootComponent, originalComponent) =>
                {
                    _rotationEffectSpawnerCallbackControllerAccessor(ref rootComponent) =
                        originalComponent._beatmapCallbacksController;
                    rootComponent._trackLaneRingsRotationEffect =
                        rootComponent.GetComponent<TrackLaneRingsRotationEffect>();
                }
            );

            GetComponentAndOriginal<Spectrogram>(
                (rootComponent, originalComponent) =>
                    _spectrogramDataAccessor(ref rootComponent) = originalComponent._spectrogramData
            );

            GetComponentAndOriginal<LightRotationEventEffect>(
                (rootComponent, originalComponent) =>
                    _lightCallbackControllerAccessor(ref rootComponent) =
                        originalComponent._beatmapCallbacksController
            );

            GetComponentAndOriginal<LightPairRotationEventEffect>(
                (rootComponent, originalComponent) =>
                {
                    _lightPairCallbackControllerAccessor(ref rootComponent) =
                        originalComponent._beatmapCallbacksController;
                    _audioTimeSourceAccessor(ref rootComponent) =
                        originalComponent._audioTimeSource;

                    Transform transformL = originalComponent._transformL;
                    Transform transformR = originalComponent._transformR;

                    rootComponent._transformL = root.GetChild(transformL.GetSiblingIndex());
                    rootComponent._transformR = root.GetChild(transformR.GetSiblingIndex());

                    // We have to enable the object to tell unity to run Start
                    rootComponent.enabled = true;
                }
            );

            GetComponentAndOriginal<ParticleSystemEventEffect>(
                (rootComponent, originalComponent) =>
                {
                    _particleCallbackControllerAccessor(ref rootComponent) =
                        originalComponent._beatmapCallbacksController;
                    rootComponent._particleSystem = root.GetComponent<ParticleSystem>();

                    rootComponent.enabled = true;
                }
            );

            GetComponentAndOriginal<Mirror>(
                (rootComponent, originalComponent) =>
                {
                    rootComponent._mirrorRenderer = originalComponent._mirrorRenderer;
                    rootComponent._mirrorMaterial = originalComponent._mirrorMaterial;
                }
            );

            SaberBurnMarkArea? saberBurnMarkArea = root.GetComponent<SaberBurnMarkArea>();
            if (saberBurnMarkArea != null)
            {
                if (_config.PrintEnvironmentEnhancementDebug)
                {
                    _log.Debug(
                        "SaberBurnMarkArea yeeted, complain to me if you would rather it not"
                    );
                }

                Object.DestroyImmediate(saberBurnMarkArea);
            }

            SaberBurnMarkSparkles? saberBurnMarkSparkles =
                root.GetComponent<SaberBurnMarkSparkles>();
            if (saberBurnMarkSparkles != null)
            {
                if (_config.PrintEnvironmentEnhancementDebug)
                {
                    _log.Debug(
                        "SaberBurnMarkSparkles yeeted, complain to me if you would rather it not"
                    );
                }

                Object.DestroyImmediate(saberBurnMarkSparkles);
            }

            GameObjectInfo newGameObjectInfo = new(root.gameObject);
            gameObjectInfos.Add(newGameObjectInfo);

            foreach (Transform transform in root)
            {
                int index = transform.GetSiblingIndex();
                InitializeComponents(
                    transform,
                    original.GetChild(index),
                    gameObjectInfos,
                    componentDatas
                );
            }

            return;

            void GetComponentAndOriginal<T>(Action<T, T> initializeDelegate)
                where T : UnityEngine.Component
            {
                T[] rootComponents = root.GetComponents<T>();
                T[] originalComponents = original.GetComponents<T>();

                for (int i = 0; i < rootComponents.Length; i++)
                {
                    initializeDelegate(rootComponents[i], originalComponents[i]);

                    if (_config.PrintEnvironmentEnhancementDebug)
                    {
                        _log.Debug($"Initialized {typeof(T).Name}");
                    }
                }
            }
        }
    }
}
