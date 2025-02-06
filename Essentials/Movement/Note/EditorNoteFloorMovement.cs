using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Visuals;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using NoodleExtensions.Animation;
using System;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.Note
{
    public class EditorNoteFloorMovement : MonoBehaviour
    {
        //Events 

        public event Action floorMovementDidFinishEvent;

        // Properties

        public float distanceToPlayer => Mathf.Abs(_localPosition.z - (_inverseWorldRotation * _playerTransforms.headPseudoLocalPos).z);
        public float noteTime => _beatTime;
        public Vector3 endPos => _variableMovementDataProvider.moveEndPosition + _moveEndOffset;
        public Quaternion worldRotation => _worldRotation;
        public Quaternion inverseWorldRotation => _inverseWorldRotation;
        public Vector3 localPosition => _localPosition;

        // Movement related fields

        private Vector3 _localPosition;
        private float _beatTime;
        internal Vector3 _moveStartOffset;
        internal Vector3 _moveEndOffset;
        internal Quaternion _worldRotation;
        internal Quaternion _inverseWorldRotation;
        

        // Injected Fields

        private EditorDeserializedData _editorDeserializedData;
        private AnimationHelper _animationHelper;
        private PlayerTransforms _playerTransforms;
        private IVariableMovementDataProvider _variableMovementDataProvider;
        private IAudioTimeSource _audioTimeSyncController;

        [Inject]
        private void Construct(
            [InjectOptional(Id = "NoodleExtensions")] EditorDeserializedData editorDeserializedData,
            AnimationHelper animationHelper,
            PlayerTransforms playerTransforms,
            IAudioTimeSource audioTimeSyncController)
        {
            _editorDeserializedData = editorDeserializedData;
            _animationHelper = animationHelper;
            _playerTransforms = playerTransforms;
            _audioTimeSyncController = audioTimeSyncController;
        }

        public void Init(NoteEditorData? editorData, IVariableMovementDataProvider variableMovementDataProvider, float worldRotation, float beatTime, Vector3 moveStartOffset, Vector3 moveEndOffset, Func<IObjectVisuals> getVisualRoot)
        {
            _editorData = editorData;
            if (!(_editorDeserializedData?.Resolve(editorData, out _noodleData) ?? false))
            {
                _noodleData = null;
            }

            _variableMovementDataProvider = variableMovementDataProvider;
            
            _rotatedObject = getVisualRoot;
            _beatTime = beatTime;
            _moveStartOffset = moveStartOffset;
            _moveEndOffset = moveEndOffset;
            _worldRotation = Quaternion.Euler(0f, worldRotation, 0f);
            _inverseWorldRotation = Quaternion.Euler(0f, -worldRotation, 0f);
        }

        private Vector3 DefiniteNoteFloorMovement(Vector3 original)
        {
            EditorNoodleBaseNoteData? noodleData = _noodleData;
            if (noodleData == null)
            {
                return original;
            }

            _animationHelper.GetDefinitePositionOffset(noodleData.AnimationObject, noodleData.Track, 0, out Vector3? position);
            if (!position.HasValue)
            {
                return original;
            }

            return original + (position.Value + noodleData.InternalNoteOffset - endPos);
        }

        public Vector3 SetToStart()
        {
            _localPosition = _variableMovementDataProvider.moveStartPosition + _moveStartOffset;
            Vector3 vector = _worldRotation * _localPosition;
            transform.localPosition = vector;
            transform.localRotation = _worldRotation;
            _rotatedObject().GetVisualRoot().transform.localRotation = Quaternion.identity;
            return vector;
        }

        public Vector3 ManualUpdate()
        {
            float num = _audioTimeSyncController.songTime - (_beatTime - _variableMovementDataProvider.moveDuration - _variableMovementDataProvider.halfJumpDuration); ;
            _localPosition = Vector3.LerpUnclamped(_variableMovementDataProvider.moveStartPosition + _moveStartOffset, _variableMovementDataProvider.moveEndPosition + _moveEndOffset, num / _variableMovementDataProvider.moveDuration);
            Vector3 vector = _worldRotation * _localPosition;
            transform.localPosition = DefiniteNoteFloorMovement(vector);
            if (num >= _variableMovementDataProvider.moveDuration)
            {
                floorMovementDidFinishEvent?.Invoke();
            }

            _rotatedObject().GetVisualRoot().SetActive(num > 0f);

            return vector;
        }

        private EditorNoodleBaseNoteData? _noodleData;

        private NoteEditorData? _editorData;

        private Func<IObjectVisuals> _rotatedObject;
    }
}
