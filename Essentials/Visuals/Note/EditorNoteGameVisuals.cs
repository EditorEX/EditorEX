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
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Visuals
{
    internal class EditorNoteGameVisuals : MonoBehaviour, IObjectVisuals
    {
        // Injected fields
        private VisualAssetProvider _visualAssetProvider;
        private ColorManager _colorManager;
        private IReadonlyBeatmapState _state;
        private EditorDeserializedData _editorDeserializedData;
        private AnimationHelper _animationHelper;

        // Visuals fields
        private NoteEditorData _editorData;
        private GameObject _gameRoot;

        private MaterialPropertyBlockController[] _colorPropertyBlockControllers;
        private CutoutEffect _noteCutout;
        private CutoutEffect _arrowCutout;

        private GameObject[] _arrowObjects;
        private GameObject _circleObject;
        private bool _active;

        [Inject]
        private void Construct(
            [Inject(Id = "NoodleExtensions")] EditorDeserializedData editorDeserializedData,
            AnimationHelper animationHelper,
            VisualAssetProvider visualAssetProvider,
            ColorManager colorManager,
            IReadonlyBeatmapState state)
        {
            _editorDeserializedData = editorDeserializedData;
            _animationHelper = animationHelper;
            _visualAssetProvider = visualAssetProvider;
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
            _gameRoot.GetComponent<MeshRenderer>().sharedMaterial = _visualAssetProvider.gameNoteMaterial;

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

        public void Init(BaseEditorData editorData)
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

            _noteCutout.SetCutout(0f);
            _arrowCutout.SetCutout(0f);

            if (!_editorDeserializedData.Resolve(_editorData, out EditorNoodleNoteData? noodleData))
            {
                return;
            }
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
            //float currentSeconds = PopulateBeatmap._audioDataModel.bpmData.BeatToSeconds(_state.beat);
            //float _dataSeconds = PopulateBeatmap._audioDataModel.bpmData.BeatToSeconds(_editorData.beat);
            //float time = _dataSeconds - currentSeconds;
            //float dissolve = Mathf.Pow(Mathf.Clamp(time, -0.05f, 0f) * -20f, 2f);

            if (!_editorDeserializedData.Resolve(_editorData, out EditorNoodleBaseNoteData? noodleData))
            {
                return;
            }

            List<Track>? tracks = noodleData.Track;
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
                float elapsedTime = _state.beat - (_editorData.beat - (jumpDuration * 0.5f));
                normalTime = elapsedTime / jumpDuration;
            }

            _animationHelper.GetObjectOffset(
                animationObject,
                tracks,
                normalTime,
                out Vector3? positionOffset,
                out Quaternion? rotationOffset,
                out Vector3? scaleOffset,
                out Quaternion? localRotationOffset,
                out float? dissolveNote,
                out float? dissolveArrow,
                out float? cuttable);

            _noteCutout.SetCutout(1f - dissolveNote.GetValueOrDefault(1f));

            _arrowCutout.SetCutout(1f - dissolveArrow.GetValueOrDefault(1f));

            _arrowObjects[1].SetActive(_editorData.cutDirection != NoteCutDirection.Any && dissolveArrow == 1f);
        }

        public GameObject GetVisualRoot()
        {
            return _gameRoot;
        }
    }
}
