using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using Chroma;
using EditorEX.Essentials.Movement;
using EditorEX.Essentials.Movement.Note;
using EditorEX.Essentials.Visuals.Universal;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using EditorEX.Vivify.Events;
using EditorEX.Vivify.ObjectPrefab.Managers;
using Heck.Animation;
using NoodleExtensions;
using UnityEngine;
using Vivify;
using Zenject;

namespace EditorEX.Essentials.Visuals.Note
{
    internal class EditorNoteGameVisuals : MonoBehaviour, IObjectVisuals
    {
        // Injected fields
        private VisualAssetProvider _visualAssetProvider = null!;
        private ColorManager _colorManager = null!;
        private IReadonlyBeatmapState _state = null!;
        private AudioDataModel _audioDataModel = null!;
        private EditorDeserializedData _vivifyEditorDeserializedData = null!;
        private EditorDeserializedData _noodleEditorDeserializedData = null!;
        private EditorDeserializedData _chromeEditorDeserializedData = null!;
        private EditorBeatmapObjectPrefabManager _prefabManager = null!;
        private EditorVivifyNotePrefabManager _vivifyNotePrefabManager = null!;

        // Visuals fields
        private NoteEditorData? _editorData;

        // Resolved once per Init (when _editorData changes) and reused on the per-frame ManualUpdate
        // path instead of doing dictionary lookups every frame for every note.
        private EditorNoodleBaseNoteData? _noodleData;
        private ChromaObjectData? _chromaData;

        private GameObject _gameRoot;

        private MaterialPropertyBlockController[] _colorPropertyBlockControllers;
        private CutoutEffect _noteCutout;
        private CutoutEffect _arrowCutout;
        private EditorNoteJump _noteJump;

        private GameObject[] _arrowObjects;
        private GameObject _circleObject;
        private bool _active;
        private bool _hasAssignedPrefab;
        private float _lastPrefabElapsed = -1f;
        private float _lastNoteCutout = float.NaN;
        private float _lastArrowCutout = float.NaN;
        private readonly VivifyPrefabPreviewSync.Targets _prefabTargets = new();

        [Inject]
        private void Construct(
            [InjectOptional(Id = "Vivify")] EditorDeserializedData vivifyEditorDeserializedData,
            [InjectOptional(Id = "NoodleExtensions")]
                EditorDeserializedData noodleEditorDeserializedData,
            [InjectOptional(Id = "Chroma")] EditorDeserializedData chromeEditorDeserializedData,
            VisualAssetProvider visualAssetProvider,
            ColorManager colorManager,
            IReadonlyBeatmapState state,
            AudioDataModel audioDataModel,
            EditorBeatmapObjectPrefabManager prefabManager,
            EditorVivifyNotePrefabManager vivifyNotePrefabManager
        )
        {
            _vivifyEditorDeserializedData = vivifyEditorDeserializedData;
            _noodleEditorDeserializedData = noodleEditorDeserializedData;
            _chromeEditorDeserializedData = chromeEditorDeserializedData;
            _visualAssetProvider = visualAssetProvider;
            _audioDataModel = audioDataModel;
            _colorManager = colorManager;
            _state = state;
            _prefabManager = prefabManager;
            _vivifyNotePrefabManager = vivifyNotePrefabManager;
            _vivifyNotePrefabManager.Subscribe(
                _vivifyNotePrefabManager.ColorNotePrefabs,
                OnAssignedPrefabsChanged
            );
            _vivifyNotePrefabManager.Subscribe(
                _vivifyNotePrefabManager.AnyDirectionNotePrefabs,
                OnAssignedPrefabsChanged
            );

            if (_visualAssetProvider.gameNotePrefab == null)
            {
                _visualAssetProvider.onFinishLoading += SetupObjectAndUnbind;
            }
            else
            {
                SetupObject();
            }
        }

        private void SetupObjectAndUnbind()
        {
            _visualAssetProvider.onFinishLoading -= SetupObjectAndUnbind;
            SetupObject();
        }

        private void SetupObject()
        {
            _gameRoot = Instantiate(
                _visualAssetProvider.gameNotePrefab.transform.Find("NoteCube"),
                transform,
                false
            ).gameObject;
            _gameRoot.name = "GamerNoterCuber";

            _arrowObjects =
            [
                _gameRoot.transform.Find("NoteArrow").gameObject,
                _gameRoot.transform.Find("NoteArrowGlow").gameObject,
            ];
            _circleObject = _gameRoot.transform.Find("NoteCircleGlow").gameObject;

            _colorPropertyBlockControllers =
            [
                _gameRoot.GetComponent<MaterialPropertyBlockController>(),
                _circleObject.GetComponent<MaterialPropertyBlockController>(),
                _arrowObjects[1].GetComponent<MaterialPropertyBlockController>(),
            ];

            _noteCutout = _gameRoot.GetComponent<CutoutEffect>();
            _arrowCutout = _arrowObjects[0].GetComponent<CutoutEffect>();

            _noteJump = GetComponent<EditorNoteJump>();

            Disable();
        }

        public void Init(BaseEditorData? editorData)
        {
            _editorData = editorData as NoteEditorData;
            _lastNoteCutout = float.NaN;
            _lastArrowCutout = float.NaN;

            EditorNoodleBaseNoteData? noodleData = null;
            _noodleEditorDeserializedData?.Resolve(_editorData, out noodleData);
            _noodleData = noodleData;

            ChromaObjectData? chromaData = null;
            _chromeEditorDeserializedData?.Resolve(_editorData, out chromaData);
            _chromaData = chromaData;

            if (_active)
            {
                Enable();
            }
            else
            {
                Disable();
            }

            var noteColor = _colorManager.ColorForType(_editorData.type);

            Color noteColorWithAlpha = noteColor.ColorWithAlpha(1f);
            foreach (MaterialPropertyBlockController controller in _colorPropertyBlockControllers)
            {
                controller.materialPropertyBlock.SetColor(
                    ColorNoteVisuals._colorId,
                    noteColorWithAlpha
                );
                controller.ApplyChanges();
            }

            bool anyDirection = _editorData.cutDirection == NoteCutDirection.Any;
            foreach (GameObject arrowObject in _arrowObjects)
            {
                arrowObject.SetActive(!anyDirection);
            }
            _circleObject.SetActive(anyDirection);
            _circleObject.GetComponent<MeshRenderer>().enabled = anyDirection;
        }

        public void Enable()
        {
            _gameRoot?.SetActive(true);
            _active = true;
            _hasAssignedPrefab = false;
            _lastPrefabElapsed = -1f;
            _prefabTargets.Clear();

            _prefabManager.Despawn(_gameRoot.transform);

            if (
                _vivifyEditorDeserializedData == null
                || !_vivifyEditorDeserializedData.Resolve(_editorData, out VivifyObjectData? data)
                || data?.Track == null
            )
            {
                return;
            }

            var prefabDictionary =
                _editorData.cutDirection == NoteCutDirection.Any
                    ? _vivifyNotePrefabManager.AnyDirectionNotePrefabs
                    : _vivifyNotePrefabManager.ColorNotePrefabs;

            // Gameplay passes NoteFloorMovement._beatTime, which is seconds.
            float noteSeconds = _audioDataModel.bpmData.BeatToSeconds(_editorData.beat);
            _prefabManager.Spawn(data.Track, prefabDictionary, _gameRoot.transform, noteSeconds);
            _hasAssignedPrefab = true;
            _prefabTargets.Capture(_gameRoot);
            TickAssignedPrefab(noteSeconds, _audioDataModel.bpmData.BeatToSeconds(_state.beat));
        }

        public void Disable()
        {
            _gameRoot?.SetActive(false);
            _active = false;
            _hasAssignedPrefab = false;
            _lastPrefabElapsed = -1f;
            _prefabTargets.Clear();

            _prefabManager.Despawn(_gameRoot.transform);
        }

        public void ManualUpdate()
        {
            float noteSeconds = 0f;
            float playheadSeconds = 0f;
            if (_editorData != null)
            {
                noteSeconds = _audioDataModel.bpmData.BeatToSeconds(_editorData.beat);
                playheadSeconds = _audioDataModel.bpmData.BeatToSeconds(_state.beat);
                TickAssignedPrefab(noteSeconds, playheadSeconds);
            }

            EditorNoodleBaseNoteData? noodleData = _noodleData;
            if (noodleData == null)
            {
                return;
            }

            if (
                DissolveCutout.TryGetChanged(
                    noodleData.InternalDissolve,
                    ref _lastNoteCutout,
                    out float noteCutout
                )
            )
            {
                _noteCutout.SetCutout(noteCutout);
            }

            if (
                DissolveCutout.TryGetChanged(
                    noodleData.InternalDissolveArrow,
                    ref _lastArrowCutout,
                    out float arrowCutout
                )
            )
            {
                _arrowCutout.SetCutout(arrowCutout);
                _arrowObjects[1]
                    .SetActive(
                        _editorData?.cutDirection != NoteCutDirection.Any
                            && noodleData.InternalDissolveArrow == 1f
                    );
            }

            ChromaObjectData? chromaData = _chromaData;
            if (chromaData == null)
            {
                return;
            }

            IReadOnlyList<Track>? chromaTracks = chromaData.Track;
            PointDefinition<Vector4>? pathPointDefinition = chromaData.LocalPathColor;
            if (chromaTracks == null && pathPointDefinition == null)
            {
                return;
            }

            IReadOnlyList<Track>? tracks = noodleData.Track;
            NoodleObjectData.AnimationObjectData? animationObject = noodleData.AnimationObject;
            if (tracks == null && animationObject == null)
            {
                return;
            }

            float? time2 = noodleData.GetTimeProperty();
            if (
                !AnimationNormalTime.TryCompute(
                    time2,
                    playheadSeconds,
                    noteSeconds,
                    _noteJump.jumpDuration,
                    out float normalTime
                )
            )
            {
                return;
            }

            global::Chroma.Animation.AnimationHelper.GetColorOffset(
                pathPointDefinition,
                chromaTracks,
                normalTime,
                out Color? colorOffset
            );

            if (colorOffset == null)
            {
                return;
            }

            Color animatedColor = colorOffset.Value.ColorWithAlpha(1f);
            foreach (MaterialPropertyBlockController controller in _colorPropertyBlockControllers)
            {
                controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, animatedColor);
                controller.ApplyChanges();
            }
        }

        public GameObject GetVisualRoot()
        {
            return _gameRoot;
        }

        private void OnDestroy()
        {
            if (_vivifyNotePrefabManager == null)
            {
                return;
            }

            _vivifyNotePrefabManager.Unsubscribe(
                _vivifyNotePrefabManager.ColorNotePrefabs,
                OnAssignedPrefabsChanged
            );
            _vivifyNotePrefabManager.Unsubscribe(
                _vivifyNotePrefabManager.AnyDirectionNotePrefabs,
                OnAssignedPrefabsChanged
            );
        }

        private void OnAssignedPrefabsChanged(Track track)
        {
            if (
                !_active
                || _editorData == null
                || _vivifyEditorDeserializedData == null
                || !_vivifyEditorDeserializedData.Resolve(_editorData, out VivifyObjectData? data)
                || data?.Track == null
                || !data.Track.Contains(track)
            )
            {
                return;
            }

            Enable();
        }

        private void TickAssignedPrefab(float noteSeconds, float playheadSeconds)
        {
            if (_gameRoot == null || !_active || !_hasAssignedPrefab)
            {
                return;
            }

            float elapsed = VivifyPrefabPreviewSync.NoteSeekSeconds(playheadSeconds, noteSeconds);
            VivifyPrefabPreviewSync.Apply(_prefabTargets, _lastPrefabElapsed, elapsed);
            _lastPrefabElapsed = elapsed;
        }
    }
}
