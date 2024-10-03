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
        public event Action floorMovementDidFinishEvent;

        public float distanceToPlayer => Mathf.Abs(_localPosition.z - (_inverseWorldRotation * _playerTransforms.headPseudoLocalPos).z);

        public Vector3 startPos => _startPos;

        public Vector3 endPos => _endPos;

        public float startTime => _startTime;

        public float moveDuration => _moveDuration;

        public Quaternion worldRotation => _worldRotation;

        public Quaternion inverseWorldRotation => _inverseWorldRotation;

        public Vector3 localPosition => _localPosition;

        [Inject(Id = "NoodleExtensions")] private EditorDeserializedData _editorDeserializedData;

        [Inject] private AnimationHelper _animationHelper;

        public void Init(NoteEditorData editorData, float worldRotation, Vector3 startPos, Vector3 endPos, float moveDuration, float startTime, Func<IObjectVisuals> getVisualRoot)
        {
            _editorData = editorData;
            if (!_editorDeserializedData.Resolve(_editorData, out NoodleData))
            {
                NoodleData = null;
            }

            _rotatedObject = getVisualRoot;
            _worldRotation = Quaternion.Euler(0f, worldRotation, 0f);
            _inverseWorldRotation = Quaternion.Euler(0f, -worldRotation, 0f);
            _startPos = startPos;
            _endPos = endPos;
            _moveDuration = moveDuration;
            _startTime = startTime;
        }

        private Vector3 DefiniteNoteFloorMovement(Vector3 original)
        {
            EditorNoodleBaseNoteData? noodleData = NoodleData;
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
            _localPosition = _startPos;
            Vector3 vector = _worldRotation * _localPosition;
            transform.localPosition = vector;
            transform.localRotation = _worldRotation;
            _rotatedObject().GetVisualRoot().transform.localRotation = Quaternion.identity;
            return vector;
        }

        public Vector3 ManualUpdate()
        {
            float num = _audioTimeSyncController.songTime - _startTime;
            _localPosition = Vector3.Lerp(_startPos, _endPos, num / _moveDuration);
            Vector3 vector = _worldRotation * _localPosition;
            transform.localPosition = DefiniteNoteFloorMovement(vector);
            if (num >= _moveDuration)
            {
                floorMovementDidFinishEvent?.Invoke();
            }

            _rotatedObject().GetVisualRoot().SetActive(num > 0f);

            return vector;
        }

        private EditorNoodleBaseNoteData NoodleData;

        private NoteEditorData _editorData;

        private Func<IObjectVisuals> _rotatedObject;

        [Inject]
        private readonly PlayerTransforms _playerTransforms;

        [Inject]
        private readonly IAudioTimeSource _audioTimeSyncController;

        internal Vector3 _startPos;

        internal Vector3 _endPos;

        internal float _moveDuration;

        internal float _startTime;

        internal Quaternion _worldRotation;

        internal Quaternion _inverseWorldRotation;

        internal Vector3 _localPosition;
    }
}
