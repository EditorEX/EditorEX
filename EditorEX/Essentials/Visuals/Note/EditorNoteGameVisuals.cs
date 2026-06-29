using System.Collections.Generic;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using Chroma;
using EditorEX.Essentials.Movement.Note;
using EditorEX.Essentials.Visuals.Universal;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using EditorEX.Vivify.ObjectPrefab.Managers;
using Heck.Animation;
using NoodleExtensions;
using NoodleExtensions.Animation;
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
        private AnimationHelper _animationHelper = null!;
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

        [Inject]
        private void Construct(
            [InjectOptional(Id = "Vivify")] EditorDeserializedData vivifyEditorDeserializedData,
            [InjectOptional(Id = "NoodleExtensions")]
                EditorDeserializedData noodleEditorDeserializedData,
            [InjectOptional(Id = "Chroma")] EditorDeserializedData chromeEditorDeserializedData,
            AnimationHelper animationHelper,
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
            _animationHelper = animationHelper;
            _audioDataModel = audioDataModel;
            _colorManager = colorManager;
            _state = state;
            _prefabManager = prefabManager;
            _vivifyNotePrefabManager = vivifyNotePrefabManager;

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

            _noteCutout.SetCutout(0f);
            _arrowCutout.SetCutout(0f);
        }

        public void Enable()
        {
            _gameRoot?.SetActive(true);
            _active = true;

            _prefabManager.Despawn(_gameRoot.transform);

            if (
                !_vivifyEditorDeserializedData.Resolve(_editorData, out VivifyObjectData? data)
                || data?.Track == null
            )
            {
                return;
            }

            var prefabDictionary =
                _editorData.cutDirection == NoteCutDirection.Any
                    ? _vivifyNotePrefabManager.AnyDirectionNotePrefabs
                    : _vivifyNotePrefabManager.ColorNotePrefabs;

            _prefabManager.Spawn(
                data.Track,
                prefabDictionary,
                _gameRoot.transform,
                _editorData.beat
            );
        }

        public void Disable()
        {
            _gameRoot?.SetActive(false);
            _active = false;

            _prefabManager.Despawn(_gameRoot.transform);
        }

        public void ManualUpdate()
        {
            EditorNoodleBaseNoteData? noodleData = _noodleData;
            if (noodleData == null)
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
            float normalTime;
            if (time2.HasValue)
            {
                normalTime = time2.Value;
            }
            else
            {
                float jumpDuration = _noteJump.jumpDuration;
                float elapsedTime =
                    _audioDataModel.bpmData.BeatToSeconds(_state.beat)
                    - (
                        _audioDataModel.bpmData.BeatToSeconds(_editorData.beat)
                        - (jumpDuration * 0.5f)
                    );
                normalTime = elapsedTime / jumpDuration;
            }

            _animationHelper.GetObjectOffset(
                animationObject,
                tracks,
                normalTime,
                out _,
                out _,
                out _,
                out _,
                out float? dissolveNote,
                out float? dissolveArrow,
                out _
            );

            _noteCutout.SetCutout(1f - dissolveNote.GetValueOrDefault(1f));

            _arrowCutout.SetCutout(1f - dissolveArrow.GetValueOrDefault(1f));

            _arrowObjects[1]
                .SetActive(
                    _editorData?.cutDirection != NoteCutDirection.Any && dissolveArrow == 1f
                );

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
    }
}
