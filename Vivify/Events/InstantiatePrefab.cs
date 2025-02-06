using System;
using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Heck.Deserialize;
using EditorEX.Vivify.Managers;
using HarmonyLib;
using Heck;
using Heck.Animation;
using Heck.Animation.Transform;
using Heck.Deserialize;
using Heck.Event;
using Heck.ReLoad;
using SiraUtil.Logging;
using UnityEngine;
using Vivify;
using Vivify.Controllers.Sync;
using Vivify.Managers;
using Zenject;
using static Vivify.VivifyController;
using Object = UnityEngine.Object;

namespace EditorEX.Vivify.Events
{
    [CustomEvent(INSTANTIATE_PREFAB)]
    internal class EditorInstantiatePrefab : ICustomEvent, IInitializable, IDisposable, ITickable
    {
        private readonly EditorAssetBundleManager _assetBundleManager;
        private readonly EditorDeserializedData _deserializedData;
        private readonly IInstantiator _instantiator;
        private readonly ReLoader? _reLoader;
        private readonly SiraLog _log;
        private readonly PrefabManager _prefabManager;
        private readonly IAudioTimeSource _audioTimeSource;
        private readonly TransformControllerFactory _transformControllerFactory;
        private readonly bool _leftHanded;

        private readonly Dictionary<InstantiatePrefabData, GameObject> _loadedPrefabs = new();

        private Transform? _mirroredParent;

        private EditorInstantiatePrefab(
            SiraLog log,
            IInstantiator instantiator,
            EditorAssetBundleManager assetBundleManager,
            PrefabManager prefabManager,
            [Inject(Id = ID)] EditorDeserializedData deserializedData,
            IAudioTimeSource audioTimeSource,
            TransformControllerFactory transformControllerFactory,
            [InjectOptional] ReLoader? reLoader)
        {
            _log = log;
            _instantiator = instantiator;
            _assetBundleManager = assetBundleManager;
            _prefabManager = prefabManager;
            _deserializedData = deserializedData;
            _audioTimeSource = audioTimeSource;
            _transformControllerFactory = transformControllerFactory;
            _reLoader = reLoader;
            if (reLoader != null)
            {
                reLoader.Rewinded += OnRewind;
            }
        }

        public void Dispose()
        {
            if (_reLoader != null)
            {
                _reLoader.Rewinded -= OnRewind;
            }

            DestroyAllPrefabs();
        }

        public void Initialize()
        {
            foreach (CustomEventEditorData customEventEditorData in CustomDataRepository.GetCustomEvents())
            {
                if (customEventEditorData.eventType != INSTANTIATE_PREFAB ||
                    !_deserializedData.Resolve(customEventEditorData, out InstantiatePrefabData? data))
                {
                    continue;
                }

                string assetName = data.Asset;
                if (!_assetBundleManager.TryGetAsset(assetName, out GameObject? prefab))
                {
                    continue;
                }

                GameObject gameObject = Object.Instantiate(prefab);
                gameObject.SetActive(false);
                _loadedPrefabs.Add(data, gameObject);
            }

            // ReSharper disable once InvertIf
            if (_leftHanded)
            {
                _mirroredParent = new GameObject("LeftHandPrefabParent").transform;
                _mirroredParent.localScale = _mirroredParent.localScale.Mirror();
            }
        }

        public void Callback(CustomEventData customEventData)
        {
            if (!_deserializedData.Resolve(CustomDataRepository.GetCustomEventConversion(customEventData), out InstantiatePrefabData? data))
            {
                return;
            }

            if (!_loadedPrefabs.TryGetValue(data, out GameObject gameObject))
            {
                return;
            }

            gameObject.SetActive(true);
            Transform transform = gameObject.transform;
            data.TransformData.Apply(transform, false);
            if (_mirroredParent != null)
            {
                transform.SetParent(_mirroredParent);
            }

            if (data.Track != null)
            {
                foreach (Track track in data.Track)
                {
                    track.AddGameObject(gameObject);
                }

                _transformControllerFactory.Create(gameObject, data.Track);
            }

            _instantiator.SongSynchronize(gameObject, customEventData.time);

            string? id = data.Id;
            if (id != null)
            {
                _log.Debug($"Enabled [{data.Asset}] with id [{id}]");
                _prefabManager.Add(id, gameObject, data.Track);
            }
            else
            {
                string genericId = gameObject.GetHashCode().ToString();
                _log.Debug($"Enabled [{data.Asset}] without id");
                _prefabManager.Add(genericId, gameObject, data.Track);
            }
        }

        private void OnRewind()
        {
            DestroyAllPrefabs();
            Initialize();
        }

        private void DestroyAllPrefabs()
        {
            _loadedPrefabs.Values.Do(Object.Destroy);
            _loadedPrefabs.Clear();
        }

        private float _lastBeat = 0f;
        public void Tick()
        {
            if (_lastBeat > _audioTimeSource.songTime)
            {
                _prefabManager.DestroyAllPrefabs();
                DestroyAllPrefabs();
                Initialize();
            }
            
            _lastBeat = _audioTimeSource.songTime;
        }
    }
}