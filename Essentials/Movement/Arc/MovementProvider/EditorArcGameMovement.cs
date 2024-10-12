using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Visuals;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.SpawnProcessing;
using EditorEX.Essentials.Visuals;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using NoodleExtensions.Animation;
using System;
using System.Linq;
using UnityEngine;
using Zenject;
using static SliderController;

namespace EditorEX.Essentials.Movement.Arc.MovementProvider
{
    public class EditorArcGameMovement : MonoBehaviour, IObjectMovement
    {
        // Injected fields
        private IEditorBeatmapObjectSpawnMovementData _editorBeatmapObjectSpawnMovementData;
        private EditorDeserializedData _editorDeserializedData;
        private AnimationHelper _animationHelper;
        private IReadonlyBeatmapState _state;
        private ColorManager _colorManager;
        private IAudioTimeSource _audioTimeSyncController;
        private AudioDataModel _audioDataModel;

        // Arc related fields
        private LengthType _lengthType;
        private ArcEditorData _sliderEditorData;
        private SliderData _sliderData;
        private Saber _saber;
        private float _headJumpOffsetY;
        private float _sliderDuration;
        private Color _initColor;
        private bool _attractingSaber;
        private float _randomValue;
        private float _zDistanceBetweenNotes;
        private float _jumpDistance;

        // Arc movement fields
        private Vector3 _headNoteJumpStartPos;
        private Vector3 _headNoteJumpEndPos;
        private float _headNoteTime;
        private float _tailNoteTime;
        private Vector3 _localPosition;
        private Quaternion _worldRotation;
        private float _jumpDuration;
        private float _headNoteGravity;
        private float _tailNoteGravity;
        private bool _movementEndReported;
        private bool _headDidMovePastCutMarkReported;
        private bool _tailDidMovePastCutMarkReported;
        private float _timeSinceHeadNoteJump;

        // Object fields
        private SliderMeshController _sliderMeshController;
        private MaterialPropertyBlockController _materialPropertyBlockController;
        private EditorSliderIntensityEffect _sliderIntensityEffect;

        [Inject]
        private void Construct(
            [InjectOptional(Id = "NoodleExtensions")] EditorDeserializedData editorDeserializedData,
            AnimationHelper animationHelper,
            IReadonlyBeatmapState state,
            ColorManager colorManager,
            IAudioTimeSource audioTimeSyncController,
            AudioDataModel audioDataModel)
        {
            _randomValue = UnityEngine.Random.value; // Set this here insted of Init to avoid random value changing during reinits

            _editorDeserializedData = editorDeserializedData;
            _animationHelper = animationHelper;
            _state = state;
            _colorManager = colorManager;
            _audioTimeSyncController = audioTimeSyncController;
            _audioDataModel = audioDataModel;
        }

        public LengthType GetLengthFromSliderData(BaseSliderEditorData sliderNoteData, BeatmapObjectSpawnMovementData.SliderSpawnData sliderSpawnData)
        {
            float jumpDuration = sliderSpawnData.jumpDuration;
            float num = (sliderSpawnData.headJumpEndPos.z - sliderSpawnData.headJumpStartPos.z) / jumpDuration;
            float num2 = _audioDataModel.bpmData.BeatToSeconds(sliderNoteData.beat) - _audioDataModel.bpmData.BeatToSeconds(sliderNoteData.tailBeat);
            float num3 = num * num2;
            if (num3 >= 15f)
            {
                return LengthType.Long;
            }
            if (num3 >= 5f)
            {
                return LengthType.Medium;
            }
            return LengthType.Short;
        }

        private void UpdateMaterialPropertyBlock(float timeSinceHeadNoteJump, MaterialPropertyBlockController materialPropertyBlockController)
        {
            var materialPropertyBlock = materialPropertyBlockController.materialPropertyBlock;
            SliderShaderHelper.SetTimeSinceHeadNoteJump(materialPropertyBlock, timeSinceHeadNoteJump);
            //SliderShaderHelper.SetSaberAttractionPoint(materialPropertyBlock, closeSmoothedSaberInteractionPos.GetValue(TimeHelper.interpolationFactor));
            SliderShaderHelper.EnableSaberAttraction(materialPropertyBlock, false);
            SliderShaderHelper.SetColor(materialPropertyBlock, _initColor);
            if (timeSinceHeadNoteJump < _sliderDuration)
            {
                SliderShaderHelper.SetTailHeadNoteJumpOffsetDifference(materialPropertyBlock, 0f);
            }
            materialPropertyBlockController.ApplyChanges();
        }

        private SliderData CreateSliderData(float headTime, float tailTime, float controlPointLength, NoteCutDirection cutDirection, float tailControlPointLength, NoteCutDirection tailCutDirection, SliderMidAnchorMode midAnchorMode)
        {
            return SliderData.CreateSliderData(
                ColorType.None, 
                headTime, 
                _audioDataModel.bpmData.SecondsToBeat(headTime),
                _sliderEditorData.column, 
                (NoteLineLayer)_sliderEditorData.row, 
                (NoteLineLayer)_sliderEditorData.row, 
                controlPointLength, cutDirection, 
                tailTime, _sliderEditorData.tailColumn, 
                (NoteLineLayer)_sliderEditorData.tailRow, 
                (NoteLineLayer)_sliderEditorData.tailRow, 
                tailControlPointLength, 
                tailCutDirection, 
                midAnchorMode);
        }

        public void SetInitialProperties(MaterialPropertyBlockController materialPropertyBlock, float noteJumpMovementSpeed)
        {
            SliderShaderHelper.SetInitialProperties(materialPropertyBlock.materialPropertyBlock, _initColor * _sliderIntensityEffect.colorIntensity, _headNoteGravity, _tailNoteGravity, noteJumpMovementSpeed, _jumpDistance, _zDistanceBetweenNotes, _sliderMeshController.pathLength, EditorSpawnDataRepository.GetSpawnData(_sliderEditorData).hasHeadNote, EditorSpawnDataRepository.GetSpawnData(_sliderEditorData).hasTailNote, _randomValue);
        }

        public void Init(BaseEditorData editorData, EditorBasicBeatmapObjectSpawnMovementData movementData, Func<IObjectVisuals> getVisualRoot)
        {
            var arcvView = GetComponent<ArcView>();
            _sliderMeshController = arcvView._arcMeshController;
            _materialPropertyBlockController = arcvView._arcMaterialPropertyBlockController;
            _sliderIntensityEffect = GetComponent<EditorSliderIntensityEffect>();

            _sliderEditorData = editorData as ArcEditorData;

            var material = _sliderMeshController.GetComponent<MeshRenderer>().sharedMaterial;
            material.enabledKeywords = material.enabledKeywords.Where(x => x.name != "BEATMAP_EDITOR_ONLY").ToArray();

            float headTime = _audioDataModel.bpmData.BeatToSeconds(_sliderEditorData.beat);
            float tailTime = _audioDataModel.bpmData.BeatToSeconds(_sliderEditorData.tailBeat);

            _editorBeatmapObjectSpawnMovementData = movementData;
            _sliderData = CreateSliderData(headTime, tailTime, _sliderEditorData.controlPointLengthMultiplier, _sliderEditorData.cutDirection, _sliderEditorData.tailControlPointLengthMultiplier, _sliderEditorData.tailCutDirection, _sliderEditorData.midAnchorMode);

            var sliderSpawnData = _editorBeatmapObjectSpawnMovementData.GetSliderSpawnData(_sliderEditorData);

            float worldRotation = 0f;

            _lengthType = GetLengthFromSliderData(_sliderEditorData, sliderSpawnData);
            MovementInit(headTime, tailTime, worldRotation, sliderSpawnData.headJumpStartPos, sliderSpawnData.headJumpEndPos, sliderSpawnData.jumpDuration, sliderSpawnData.headJumpGravity, sliderSpawnData.tailJumpGravity);
            _sliderDuration = tailTime - headTime;
            _initColor = _colorManager.ColorForType(_sliderEditorData.colorType);
            float num = sliderSpawnData.jumpDuration * 0.5f;
            _sliderIntensityEffect.Init(_sliderDuration, num, EditorSpawnDataRepository.GetSpawnData(editorData).hasHeadNote);
            float noteJumpMovementSpeed = _editorBeatmapObjectSpawnMovementData.noteJumpMovementSpeed;
            _zDistanceBetweenNotes = (tailTime - headTime) * noteJumpMovementSpeed;
            _jumpDistance = noteJumpMovementSpeed * sliderSpawnData.jumpDuration;
            Vector3 vector = new Vector3(sliderSpawnData.headJumpEndPos.x, sliderSpawnData.headJumpStartPos.y + _headNoteGravity * num * num * 0.5f, 0f);
            Vector3 vector2 = new Vector3(sliderSpawnData.tailJumpEndPos.x, sliderSpawnData.tailJumpStartPos.y + _tailNoteGravity * num * num * 0.5f, _zDistanceBetweenNotes);
            _sliderMeshController.CreateBezierPathAndMesh(_sliderData, vector, vector2, noteJumpMovementSpeed, 1f);
            SetInitialProperties(_materialPropertyBlockController, noteJumpMovementSpeed);
            UpdateMaterialPropertyBlock(-num, _materialPropertyBlockController);

            transform.localRotation = _worldRotation;

            ManualUpdate();
        }

        public void Enable()
        {

        }

        public void Disable()
        {

        }

        public void Setup(BaseEditorData editorData)
        {
        }

        public void ManualUpdate()
        {
            MovementUpdate();
            _sliderIntensityEffect.ManualUpdate(_timeSinceHeadNoteJump);
            UpdateMaterialPropertyBlock(_timeSinceHeadNoteJump, _materialPropertyBlockController);
        }

        private void MovementUpdate()
        {
            float songTime = _audioTimeSyncController.songTime;
            _timeSinceHeadNoteJump = songTime - (_headNoteTime - _jumpDuration * 0.5f);
            float num = songTime - (_tailNoteTime - _jumpDuration * 0.5f);
            float num2 = _timeSinceHeadNoteJump / _jumpDuration;
            float num3 = num / _jumpDuration;
            _localPosition.z = Mathf.LerpUnclamped(_headNoteJumpStartPos.z, _headNoteJumpEndPos.z, num2) + 0.225f;
            Vector3 vector = _worldRotation * _localPosition;
            transform.localPosition = vector;
            if (!_headDidMovePastCutMarkReported && num2 > 0.5f)
            {
                _headDidMovePastCutMarkReported = true;
            }
            if (!_tailDidMovePastCutMarkReported && num3 > 0.5f)
            {
                _tailDidMovePastCutMarkReported = true;
            }
            if (!_movementEndReported && num3 > 0.75f)
            {
                _movementEndReported = true;
            }
        }

        private void MovementInit(float headNoteTime, float tailNoteTime, float worldRotation, Vector3 headNoteJumpStartPos, Vector3 headNoteJumpEndPos, float jumpDuration, float headNoteGravity, float tailNoteGravity)
        {
            _movementEndReported = false;
            _headDidMovePastCutMarkReported = false;
            _tailDidMovePastCutMarkReported = false;
            _worldRotation = Quaternion.Euler(0f, worldRotation, 0f);
            _headNoteJumpStartPos = headNoteJumpStartPos;
            _headNoteJumpEndPos = headNoteJumpEndPos;
            _jumpDuration = jumpDuration;
            _headNoteGravity = headNoteGravity;
            _tailNoteGravity = tailNoteGravity;
            _headNoteTime = headNoteTime;
            _tailNoteTime = tailNoteTime;
            _timeSinceHeadNoteJump = -jumpDuration * 0.5f;
        }
    }
}
