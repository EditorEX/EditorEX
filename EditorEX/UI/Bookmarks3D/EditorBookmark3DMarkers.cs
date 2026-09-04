using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.Controller;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Visuals;
using EditorEX.Essentials.Features.ViewMode;
using EditorEX.Essentials.Patches;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using Zenject;

namespace EditorEX.UI.Bookmarks3D
{
    internal class EditorBookmark3DMarkers : IInitializable, ITickable, IDisposable
    {
        private const float TimeToZDistanceScale = 12f;

        private readonly BookmarksDataModel _bookmarksDataModel;
        private readonly AudioDataModel _audioDataModel;
        private readonly BeatmapObjectsHoverState _hoverState;
        private readonly IReadonlyBeatmapState _beatmapState;
        private readonly ActiveViewMode _activeViewMode;
        private readonly SignalBus _signalBus;

        private readonly Dictionary<BeatmapEditorObjectId, EditorBookmark3DMarker> _active = new();
        private readonly Stack<EditorBookmark3DMarker> _pool = new();
        private readonly HashSet<BeatmapEditorObjectId> _seen = new();
        private readonly List<BeatmapEditorObjectId> _toDespawn = new();

        private GameObject? _root;
        private Mesh? _quadMesh;
        private Material? _wallMaterial;
        private Quaternion _wallRotation = Quaternion.identity;
        private Vector3 _wallScale = new(4.2f, 0.05f, 1f);
        private float _wallLocalY;
        private TMP_FontAsset? _font;
        private Material? _fontMaterial;
        private Camera? _editorCamera;
        private bool _templatesReady;
        private bool _forceRefresh;

        private EditorBookmark3DMarkers(
            IEditorBeatmapModels editorBeatmapModels,
            IReadonlyBeatmapState beatmapState,
            ActiveViewMode activeViewMode,
            SignalBus signalBus
        )
        {
            _bookmarksDataModel = editorBeatmapModels.BookmarksDataModel;
            _audioDataModel = editorBeatmapModels.AudioDataModel;
            _hoverState = editorBeatmapModels.BeatmapObjectsHoverState;
            _beatmapState = beatmapState;
            _activeViewMode = activeViewMode;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<BookmarkCommands.BookmarksChangedSignal>(MarkDirtyAndSync);
            _signalBus.Subscribe<BookmarkSetCommands.BookmarkSetsChangedSignal>(MarkDirtyAndSync);
            _signalBus.Subscribe<BookmarkSetCommands.BookmarkSetEnabledChangedSignal>(
                MarkDirtyAndSync
            );
            _activeViewMode.ModeChanged += MarkDirtyAndSync;
            TryLoadTemplates();
        }

        public void Tick()
        {
            if (!_templatesReady)
            {
                TryLoadTemplates();
                if (!_templatesReady)
                {
                    return;
                }
            }

            bool show = _activeViewMode.Mode == null || _activeViewMode.Mode.ShowGridAndSelection;
            if (_root != null && _root.activeSelf != show)
            {
                _root.SetActive(show);
            }

            if (!show)
            {
                return;
            }

            SyncVisibleMarkers();
            HandleClick();
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<BookmarkCommands.BookmarksChangedSignal>(MarkDirtyAndSync);
            _signalBus.TryUnsubscribe<BookmarkSetCommands.BookmarkSetsChangedSignal>(
                MarkDirtyAndSync
            );
            _signalBus.TryUnsubscribe<BookmarkSetCommands.BookmarkSetEnabledChangedSignal>(
                MarkDirtyAndSync
            );
            _activeViewMode.ModeChanged -= MarkDirtyAndSync;

            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
                _root = null;
            }

            if (_wallMaterial != null)
            {
                UnityEngine.Object.Destroy(_wallMaterial);
                _wallMaterial = null;
            }
        }

        private void MarkDirtyAndSync()
        {
            _forceRefresh = true;
            SyncVisibleMarkers();
        }

        private void TryLoadTemplates()
        {
            var objectsContainer = Resources
                .FindObjectsOfTypeAll<BeatmapObjectsContainer>()
                .FirstOrDefault(container => container.gameObject.scene.IsValid());
            if (objectsContainer == null)
            {
                return;
            }

            var wrapper = objectsContainer.transform.parent;
            if (wrapper == null)
            {
                return;
            }

            var parent = wrapper.Find("AllObjectsContainer");
            if (parent == null)
            {
                return;
            }

            var currentBeatline = objectsContainer.transform.Find(
                "BeatGridContainer/CurrentBeatline/Quad"
            );
            if (currentBeatline == null)
            {
                return;
            }

            var meshFilter = currentBeatline.GetComponent<MeshFilter>();
            var meshRenderer = currentBeatline.GetComponent<MeshRenderer>();
            if (meshFilter == null || meshFilter.sharedMesh == null || meshRenderer == null)
            {
                return;
            }

            var fontSource = Resources
                .FindObjectsOfTypeAll<TMP_Text>()
                .FirstOrDefault(text =>
                    text.font != null && text.font.name.StartsWith("NotoSans-Medium")
                );
            if (fontSource == null || fontSource.font == null)
            {
                return;
            }

            _quadMesh = meshFilter.sharedMesh;
            _wallMaterial = CreateWallMaterial(meshRenderer.sharedMaterial);
            CaptureBeatlinePose(currentBeatline);
            _font = fontSource.font;
            _fontMaterial = fontSource.fontSharedMaterial;

            _root = new GameObject("EditorEXBookmark3DMarkers");
            _root.transform.SetParent(parent, false);

            _templatesReady = true;
            _forceRefresh = true;
            SyncVisibleMarkers();
        }

        private void CaptureBeatlinePose(Transform quad)
        {
            Transform line = quad.parent != null ? quad.parent : quad;
            _wallRotation =
                line.localRotation * (line == quad ? Quaternion.identity : quad.localRotation);
            _wallScale = Vector3.Scale(
                line.localScale,
                line == quad ? Vector3.one : quad.localScale
            );
            _wallLocalY = line.localPosition.y + (line == quad ? 0f : quad.localPosition.y);

            // Sit just above the floor lines so the opaque strip does not z-fight.
            _wallLocalY += 0.01f;
        }

        private static Material CreateWallMaterial(Material template)
        {
            var unlit = Shader.Find("Unlit/Color");
            Material material = unlit != null ? new Material(unlit) : new Material(template);

            material.SetInt("_SrcBlend", (int)BlendMode.One);
            material.SetInt("_DstBlend", (int)BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.SetInt("_Cull", (int)CullMode.Off);
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_EMISSION");
            material.DisableKeyword("ENABLE_BLOOM_FOG");
            material.renderQueue = 2000;
            if (material.HasProperty("_EnableFog"))
            {
                material.SetFloat("_EnableFog", 0f);
            }

            return material;
        }

        private void SyncVisibleMarkers()
        {
            if (
                !_templatesReady
                || _root == null
                || !_root.activeInHierarchy
                || _bookmarksDataModel == null
                || _audioDataModel?.bpmData == null
            )
            {
                return;
            }

            float minZ = -TimeToPosition(BeatmapObjectPlacementHelper.kObjectsDespawnTime);
            float maxZ = TimeToPosition(BeatmapObjectPlacementHelper.kObjectsPreviewTime);

            _seen.Clear();

            foreach (var pair in _bookmarksDataModel.bookmarksListBySetId)
            {
                if (!_bookmarksDataModel.IsBookmarkSetEnabled(pair.Key))
                {
                    continue;
                }

                if (!_bookmarksDataModel.bookmarkSetById.TryGetValue(pair.Key, out var set))
                {
                    continue;
                }

                foreach (var bookmark in pair.Value)
                {
                    float z = BeatToPosition(bookmark.beat);
                    if (z < minZ || z > maxZ)
                    {
                        continue;
                    }

                    _seen.Add(bookmark.id);
                    bool spawned = !_active.TryGetValue(bookmark.id, out var marker);
                    if (spawned)
                    {
                        marker = Spawn();
                        _active[bookmark.id] = marker;
                    }

                    if (spawned || _forceRefresh)
                    {
                        string text = string.IsNullOrWhiteSpace(bookmark.text)
                            ? bookmark.label
                            : bookmark.text;
                        marker!.SetData(text, set.color, bookmark.beat);
                    }

                    marker!.SetWorldZ(z);
                    marker.gameObject.SetActive(true);
                }
            }

            _toDespawn.Clear();
            foreach (var id in _active.Keys)
            {
                if (!_seen.Contains(id))
                {
                    _toDespawn.Add(id);
                }
            }

            foreach (var id in _toDespawn)
            {
                Despawn(_active[id]);
                _active.Remove(id);
            }

            _forceRefresh = false;
        }

        private EditorBookmark3DMarker Spawn()
        {
            if (_pool.Count > 0)
            {
                var pooled = _pool.Pop();
                pooled.gameObject.SetActive(true);
                return pooled;
            }

            return EditorBookmark3DMarker.Create(
                _root!.transform,
                _quadMesh!,
                _wallMaterial!,
                _wallRotation,
                _wallScale,
                _wallLocalY,
                _font!,
                _fontMaterial!
            );
        }

        private void Despawn(EditorBookmark3DMarker marker)
        {
            marker.gameObject.SetActive(false);
            _pool.Push(marker);
        }

        private void HandleClick()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (
                _beatmapState.cameraMoving
                || (_hoverState != null && _hoverState.isBeatmapObjectHovered)
            )
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            var camera = ResolveEditorCamera();
            if (camera == null)
            {
                return;
            }

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
                return;
            }

            var marker = hit.collider.GetComponentInParent<EditorBookmark3DMarker>();
            if (marker == null)
            {
                return;
            }

            _signalBus.Fire(
                new UpdatePlayHeadSignal(marker.Beat, UpdatePlayHeadSignal.SnapType.None, true)
            );
        }

        private Camera? ResolveEditorCamera()
        {
            if (_editorCamera != null)
            {
                return _editorCamera;
            }

            var controller = Resources
                .FindObjectsOfTypeAll<BeatmapEditor360CameraController>()
                .FirstOrDefault(item => item.isActiveAndEnabled);
            if (controller != null)
            {
                _editorCamera =
                    controller._uiCameraTransform.GetComponent<Camera>()
                    ?? controller._uiCameraTransform.GetComponentInChildren<Camera>();
            }

            if (_editorCamera == null)
            {
                _editorCamera = Camera.main;
            }

            return _editorCamera;
        }

        private float BeatToPosition(float beat)
        {
            float currentSeconds = _audioDataModel.bpmData.BeatToSeconds(_beatmapState.beat);
            float beatSeconds = _audioDataModel.bpmData.BeatToSeconds(beat);
            return TimeToPosition(beatSeconds - currentSeconds);
        }

        private static float TimeToPosition(float time)
        {
            return time * TimeToZDistanceScale;
        }
    }
}
