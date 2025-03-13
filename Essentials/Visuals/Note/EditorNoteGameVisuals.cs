using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Note;
using EditorEX.Essentials.Visuals.Universal;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using HarmonyLib;
using Heck.Animation;
using NoodleExtensions;
using NoodleExtensions.Animation;
using System.Collections.Generic;
using BeatmapEditor3D;
using Chroma;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Visuals.Note
{
    internal class EditorNoteGameVisuals : MonoBehaviour, IObjectVisuals
    {
        // Injected fields
        private VisualAssetProvider _visualAssetProvider;
        private ColorManager _colorManager;
        private IReadonlyBeatmapState _state;
        private AudioDataModel _audioDataModel;
        private EditorDeserializedData _noodleEditorDeserializedData;
        private EditorDeserializedData _chromeEditorDeserializedData;
        private AnimationHelper _animationHelper;

        // Visuals fields
        private NoteEditorData? _editorData;
        private GameObject _gameRoot;

        private MaterialPropertyBlockController[] _colorPropertyBlockControllers;
        private CutoutEffect _noteCutout;
        private CutoutEffect _arrowCutout;

        private GameObject[] _arrowObjects;
        private GameObject _circleObject;
        private bool _active;

        [Inject]
        private void Construct(
            [InjectOptional(Id = "NoodleExtensions")] EditorDeserializedData noodleEditorDeserializedData,
            [InjectOptional(Id = "Chroma")] EditorDeserializedData chromeEditorDeserializedData,
            AnimationHelper animationHelper,
            VisualAssetProvider visualAssetProvider,
            ColorManager colorManager,
            IReadonlyBeatmapState state,
            AudioDataModel audioDataModel)
        {
            _noodleEditorDeserializedData = noodleEditorDeserializedData;
            _chromeEditorDeserializedData = chromeEditorDeserializedData;
            _visualAssetProvider = visualAssetProvider;
            _animationHelper = animationHelper;
            _audioDataModel = audioDataModel;
            _colorManager = colorManager;
            _state = state;

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
            _gameRoot = Instantiate(_visualAssetProvider.gameNotePrefab.transform.Find("NoteCube"), transform, false).gameObject;
            _gameRoot.name = "GamerNoterCuber";

            _arrowObjects = new[] { _gameRoot.transform.Find("NoteArrow").gameObject, _gameRoot.transform.Find("NoteArrowGlow").gameObject };
            _circleObject = _gameRoot.transform.Find("NoteCircleGlow").gameObject;

            _colorPropertyBlockControllers = new[]
            {
                _gameRoot.GetComponent<MaterialPropertyBlockController>(),
                _circleObject.GetComponent<MaterialPropertyBlockController>(),
                _arrowObjects[1].GetComponent<MaterialPropertyBlockController>()
            };

            _noteCutout = _gameRoot.GetComponent<CutoutEffect>();
            _arrowCutout = _arrowObjects[0].GetComponent<CutoutEffect>();

            Disable();
        }

        public void Init(BaseEditorData? editorData)
        {
            _editorData = editorData as NoteEditorData;

            if (_active)
            {
                Enable();
            }
            else
            {
                Disable();
            }

            var noteColor = _colorManager.ColorForType(_editorData.type);

            _colorPropertyBlockControllers.Do(x =>
            {
                x.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, noteColor.ColorWithAlpha(1f));
                x.ApplyChanges();
            });

            bool anyDirection = _editorData.cutDirection == NoteCutDirection.Any;
            _arrowObjects.Do(x => x.SetActive(!anyDirection));
            _circleObject.SetActive(anyDirection);
            _circleObject.GetComponent<MeshRenderer>().enabled = anyDirection;

            _noteCutout.SetCutout(0f);
            _arrowCutout.SetCutout(0f);
        }

        public void Enable()
        {
            _gameRoot?.SetActive(true);
            _active = true;
        }

        public void Disable()
        {
            _gameRoot?.SetActive(false);
            _active = false;
        }

        public void ManualUpdate()
        {
            EditorNoodleBaseNoteData? noodleData = null;
            if (!(_noodleEditorDeserializedData?.Resolve(_editorData, out noodleData) ?? false) || noodleData == null)
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
                float jumpDuration = GetComponent<EditorNoteJump>().jumpDuration;
                float elapsedTime = _audioDataModel.bpmData.BeatToSeconds(_state.beat) - (_audioDataModel.bpmData.BeatToSeconds(_editorData.beat) - (jumpDuration * 0.5f));
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
                out _);

            _noteCutout.SetCutout(1f - dissolveNote.GetValueOrDefault(1f));

            _arrowCutout.SetCutout(1f - dissolveArrow.GetValueOrDefault(1f));

            _arrowObjects[1].SetActive(_editorData?.cutDirection != NoteCutDirection.Any && dissolveArrow == 1f);

            if (!(_chromeEditorDeserializedData?.Resolve(_editorData, out ChromaObjectData? chromaData) ?? false) || chromaData == null)
            {
                return;
            }

            IReadOnlyList<Track>? chromaTracks = chromaData.Track;
            PointDefinition<Vector4>? pathPointDefinition = chromaData.LocalPathColor;
            if (chromaTracks == null && pathPointDefinition == null)
            {
                return;
            }

            global::Chroma.Animation.AnimationHelper.GetColorOffset(pathPointDefinition, chromaTracks, normalTime, out Color? colorOffset);

            if (colorOffset == null)
            {
                return;
            }

            _colorPropertyBlockControllers.Do(x =>
            {
                x.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, colorOffset.Value.ColorWithAlpha(1f));
                x.ApplyChanges();
            });
        }

        public GameObject GetVisualRoot()
        {
            return _gameRoot;
        }
    }
}
