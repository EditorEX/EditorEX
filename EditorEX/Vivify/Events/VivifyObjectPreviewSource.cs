using System;
using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Essentials.Patches;
using EditorEX.Essentials.PreviewState;
using EditorEX.Heck.Deserialize;
using EditorEX.Vivify.Managers;
using Heck.Animation;
using Heck.Animation.Transform;
using SiraUtil.Logging;
using UnityEngine;
using Vivify;
using Vivify.Controllers.Sync;
using Vivify.HarmonyPatches;
using Vivify.Managers;
using Zenject;
using static Vivify.VivifyController;
using Object = UnityEngine.Object;

namespace EditorEX.Vivify.Events
{
    internal sealed class VivifyObjectPreviewSource : IPreviewStateSource
    {
        private readonly EditorDeserializedData? _editorDeserializedData;
        private readonly ICustomDataRepository _customDataRepository;
        private readonly EditorAssetBundleManager _assetBundleManager;
        private readonly PrefabManager _prefabManager;
        private readonly CameraEffectApplier _cameraEffectApplier;
        private readonly EditorSetCameraProperty _setCameraProperty;
        private readonly IInstantiator _instantiator;
        private readonly TransformControllerFactory _transformControllerFactory;
        private readonly AudioDataModel _audioDataModel;
        private readonly SiraLog _log;

        private VivifyObjectPreviewSource(
            [InjectOptional(Id = ID)] EditorDeserializedData deserializedData,
            ICustomDataRepository customDataRepository,
            EditorAssetBundleManager assetBundleManager,
            PrefabManager prefabManager,
            CameraEffectApplier cameraEffectApplier,
            EditorSetCameraProperty setCameraProperty,
            IInstantiator instantiator,
            TransformControllerFactory transformControllerFactory,
            IEditorBeatmapModels populateBeatmap,
            SiraLog log
        )
        {
            _editorDeserializedData = deserializedData;
            _customDataRepository = customDataRepository;
            _assetBundleManager = assetBundleManager;
            _prefabManager = prefabManager;
            _cameraEffectApplier = cameraEffectApplier;
            _setCameraProperty = setCameraProperty;
            _instantiator = instantiator;
            _transformControllerFactory = transformControllerFactory;
            _audioDataModel = populateBeatmap.AudioDataModel;
            _log = log;
        }

        public void Build(IPreviewStateRegistry registry)
        {
            if (_editorDeserializedData == null)
            {
                return;
            }

            var items = new List<(float Beat, int Index, string[] Ids, Kind Kind, object Data)>();
            int index = 0;
            foreach (CustomEventEditorData customEvent in _customDataRepository.GetCustomEvents())
            {
                if (customEvent.eventType == INSTANTIATE_PREFAB)
                {
                    if (
                        !_editorDeserializedData.Resolve(
                            customEvent,
                            out InstantiatePrefabData? instantiateData
                        )
                        || instantiateData == null
                    )
                    {
                        continue;
                    }

                    items.Add(
                        (
                            customEvent.beat,
                            index++,
                            IdsOf(instantiateData.Id),
                            Kind.Instantiate,
                            instantiateData
                        )
                    );
                    continue;
                }

                if (customEvent.eventType == DECLARE_CULLING_TEXTURE)
                {
                    if (
                        !_editorDeserializedData.Resolve(
                            customEvent,
                            out CreateCameraData? cameraData
                        )
                        || cameraData == null
                    )
                    {
                        continue;
                    }

                    items.Add(
                        (
                            customEvent.beat,
                            index++,
                            IdsOf(cameraData.Name),
                            Kind.DeclareCamera,
                            cameraData
                        )
                    );
                    continue;
                }

                if (customEvent.eventType == DECLARE_TEXTURE)
                {
                    if (
                        !_editorDeserializedData.Resolve(
                            customEvent,
                            out CreateScreenTextureData? textureData
                        )
                        || textureData == null
                    )
                    {
                        continue;
                    }

                    items.Add(
                        (
                            customEvent.beat,
                            index++,
                            IdsOf(textureData.Name),
                            Kind.DeclareTexture,
                            textureData
                        )
                    );
                    continue;
                }

                if (customEvent.eventType != DESTROY_PREFAB)
                {
                    continue;
                }

                if (
                    !_editorDeserializedData.Resolve(
                        customEvent,
                        out DestroyObjectData? destroyData
                    )
                    || destroyData == null
                )
                {
                    continue;
                }

                items.Add((customEvent.beat, index++, destroyData.Id, Kind.Destroy, destroyData));
            }

            items.Sort(
                (a, b) =>
                {
                    int beat = a.Beat.CompareTo(b.Beat);
                    return beat != 0 ? beat : a.Index.CompareTo(b.Index);
                }
            );

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Kind == Kind.Destroy)
                {
                    continue;
                }

                float from = items[i].Beat;
                float to = PreviewStateOwnership.NextExclusiveEnd(
                    items,
                    i,
                    item => item.Beat,
                    (left, right) => VivifyPreviewOwnership.Conflicts(left.Ids, right.Ids)
                );
                IPreviewStateAction? action = CreateAction(items[i].Kind, items[i].Data, from);
                if (action == null)
                {
                    continue;
                }

                registry.Add(from, to, action);
            }
        }

        private IPreviewStateAction? CreateAction(Kind kind, object data, float fromBeat)
        {
            return kind switch
            {
                Kind.Instantiate => CreateInstantiateAction((InstantiatePrefabData)data, fromBeat),
                Kind.DeclareCamera => CreateDeclareCameraAction((CreateCameraData)data),
                Kind.DeclareTexture => CreateDeclareTextureAction((CreateScreenTextureData)data),
                _ => null,
            };
        }

        private IPreviewStateAction CreateInstantiateAction(
            InstantiatePrefabData data,
            float fromBeat
        )
        {
            string? registeredId = null;
            return new VivifyResourcePreviewAction(
                () =>
                {
                    if (!_assetBundleManager.TryGetAsset(data.Asset, out GameObject? prefab))
                    {
                        return;
                    }

                    GameObject gameObject = Object.Instantiate(prefab!);
                    Transform transform = gameObject.transform;
                    data.TransformData.Apply(transform, false);
                    if (data.Track != null)
                    {
                        foreach (Track track in data.Track)
                        {
                            track.AddGameObject(gameObject);
                        }

                        _transformControllerFactory.Create(gameObject, data.Track);
                    }

                    _instantiator.SongSynchronize(
                        gameObject,
                        _audioDataModel.bpmData.BeatToSeconds(fromBeat)
                    );

                    registeredId = data.Id ?? gameObject.GetHashCode().ToString();
                    if (data.Id != null)
                    {
                        _log.Debug($"Enabled [{data.Asset}] with id [{data.Id}]");
                    }
                    else
                    {
                        _log.Debug($"Enabled [{data.Asset}] without id");
                    }

                    _prefabManager.Add(registeredId, gameObject, data.Track);
                },
                () =>
                {
                    if (registeredId == null)
                    {
                        return;
                    }

                    _prefabManager.Destroy(registeredId);
                    registeredId = null;
                }
            );
        }

        private IPreviewStateAction CreateDeclareCameraAction(CreateCameraData data)
        {
            return new VivifyResourcePreviewAction(
                () =>
                {
                    _cameraEffectApplier.CameraDatas.Add(data.Name, data);
                    _log.Debug($"Created camera [{data.Name}]");
                    if (data.Property != null)
                    {
                        _setCameraProperty.SetCameraProperties(data.Name, data.Property);
                    }
                },
                () => _cameraEffectApplier.CameraDatas.Remove(data.Name)
            );
        }

        private IPreviewStateAction CreateDeclareTextureAction(CreateScreenTextureData data)
        {
            return new VivifyResourcePreviewAction(
                () =>
                {
                    _cameraEffectApplier.DeclaredTextureDatas.Add(data.Name, data);
                    _log.Debug($"Created texture [{data.Name}]");
                },
                () => _cameraEffectApplier.DeclaredTextureDatas.Remove(data.Name)
            );
        }

        private static string[] IdsOf(string? id)
        {
            return string.IsNullOrEmpty(id) ? Array.Empty<string>() : new[] { id! };
        }

        private enum Kind
        {
            Instantiate,
            DeclareCamera,
            DeclareTexture,
            Destroy,
        }
    }
}
