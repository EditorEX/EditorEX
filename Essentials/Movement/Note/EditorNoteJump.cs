using System;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Visuals;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using NoodleExtensions;
using NoodleExtensions.Animation;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.Note
{
    public class EditorNoteJump : MonoBehaviour
    {
        // Properties
        public float distanceToPlayer =>
            Mathf.Abs(
                _localPosition.z - (_inverseWorldRotation * _playerTransforms.headPseudoLocalPos).z
            );
        public Vector3 beatPos => (_endPos + _startPos) * 0.5f;
        public float jumpDuration => _jumpDuration;
        public Vector3 localPosition => _localPosition;

        public event Action noteJumpDidFinishEvent;

        // Jump fields

        private bool _definitePosition;
        private EditorNoodleBaseNoteData NoodleData;
        private NoteEditorData? _editorData;
        private Func<IObjectVisuals> _rotatedObject;
        private float _yAvoidanceUp = 0.45f;
        private float _yAvoidanceDown = 0.15f;
        private float _endDistanceOffset = 500f;
        private Vector3 _localPosition;
        private float _noteTime;
        private float _yAvoidance;
        private Quaternion _startRotation;
        private Quaternion _middleRotation;
        private Quaternion _endRotation;
        internal Vector3 _startOffset;
        internal Vector3 _endOffset;
        private float _gravityBase;
        private float _halfJumpDuration;
        private float _jumpDuration;
        private float _gravity;
        internal Vector3 _startPos;
        internal Vector3 _endPos;
        private float _missedTime;
        private bool _jumpStartedReported;
        private bool _missedMarkReported;
        private bool _threeQuartersMarkReported;
        private bool _halfJumpMarkReported;
        internal Quaternion _worldRotation;
        internal Quaternion _inverseWorldRotation;
        private bool _rotateTowardsPlayer;

        // Injected fields
        private PlayerTransforms _playerTransforms = null!;
        private IAudioTimeSource _audioTimeSyncController = null!;
        private AnimationHelper _animationHelper = null!;
        private EditorDeserializedData _editorDeserializedData = null!;
        private AudioDataModel _audioDataModel = null!;
        private IVariableMovementDataProvider _variableMovementDataProvider = null!;

        [Inject]
        public void Construct(
            PlayerTransforms playerTransforms,
            IAudioTimeSource audioTimeSyncController,
            AnimationHelper animationHelper,
            [InjectOptional(Id = NoodleController.ID)]
                EditorDeserializedData editorDeserializedData,
            AudioDataModel audioDataModel
        )
        {
            _playerTransforms = playerTransforms;
            _audioTimeSyncController = audioTimeSyncController;
            _animationHelper = animationHelper;
            _editorDeserializedData = editorDeserializedData;
            _audioDataModel = audioDataModel;
        }

        public void Init(
            NoteEditorData? editorData,
            IVariableMovementDataProvider variableMovementDataProvider,
            float noteTime,
            float worldRotation,
            Vector3 moveEndOffset,
            Vector3 jumpEndOffset,
            float gravityBase,
            float flipYSide,
            float endRotation,
            Func<IObjectVisuals>? getVisualRoot
        )
        {
            _editorData = editorData;
            if (!(_editorDeserializedData?.Resolve(editorData, out NoodleData) ?? false))
            {
                NoodleData = null;
            }

            _variableMovementDataProvider = variableMovementDataProvider;

            _rotatedObject = getVisualRoot;
            _worldRotation = Quaternion.Euler(0f, worldRotation, 0f);
            _inverseWorldRotation = Quaternion.Euler(0f, -worldRotation, 0f);
            _startOffset = moveEndOffset;
            _endOffset = jumpEndOffset;
            _gravityBase = gravityBase;
            _noteTime = noteTime;
            if (flipYSide > 0f)
            {
                _yAvoidance = flipYSide * _yAvoidanceUp;
            }
            else
            {
                _yAvoidance = flipYSide * _yAvoidanceDown;
            }
            _jumpStartedReported = false;
            _missedMarkReported = false;
            _threeQuartersMarkReported = false;
            _halfJumpMarkReported = false;
            _endRotation = Quaternion.Euler(0f, 0f, endRotation);
            Vector3 vector = _endRotation.eulerAngles;
            _middleRotation = Quaternion.Euler(vector);
            _startRotation = Quaternion.identity;
        }

        private float NoteJumpTimeAdjust(float original, float jumpDuration)
        {
            float? time = NoodleData?.GetTimeProperty();
            if (time.HasValue)
            {
                return time.Value * jumpDuration;
            }

            return original;
        }

        private Vector3 DefiniteNoteJump(Vector3 original, float time)
        {
            EditorNoodleBaseNoteData? noodleData = NoodleData;
            if (noodleData != null)
            {
                _animationHelper.GetDefinitePositionOffset(
                    noodleData.AnimationObject,
                    noodleData.Track,
                    time,
                    out Vector3? position
                );
                if (position.HasValue)
                {
                    _definitePosition = true;
                    return position.Value + noodleData.InternalNoteOffset;
                }
            }

            _definitePosition = false;
            return original;
        }

        public Vector3 ManualUpdate()
        {
            bool hasNoodle = NoodleData != null;

            if (!_missedMarkReported)
            {
                _halfJumpDuration = _variableMovementDataProvider.halfJumpDuration;
                _jumpDuration = _variableMovementDataProvider.jumpDuration;
                _gravity = _variableMovementDataProvider.CalculateCurrentNoteJumpGravity(
                    _gravityBase
                );
                _startPos = _variableMovementDataProvider.moveEndPosition + _startOffset;
                _endPos = _variableMovementDataProvider.jumpEndPosition + _endOffset;
                _missedTime = _noteTime + 0.15f;
            }

            float songTime = _audioTimeSyncController.songTime;
            float num = hasNoodle
                ? NoteJumpTimeAdjust(songTime - (_noteTime - _halfJumpDuration), _jumpDuration)
                : songTime - (_noteTime - _halfJumpDuration);
            float num2 = num / _jumpDuration;
            if (_startPos.x == _endPos.x)
            {
                _localPosition.x = _startPos.x;
            }
            else if (num2 < 0.25f)
            {
                _localPosition.x =
                    _startPos.x + (_endPos.x - _startPos.x) * Easing.InOutQuad(num2 * 4f);
            }
            else
            {
                _localPosition.x = _endPos.x;
            }
            _localPosition.z = Mathf.LerpUnclamped(_startPos.z, _endPos.z, num2);
            float endGrav = _gravity * _halfJumpDuration;
            _localPosition.y = _startPos.y + endGrav * num - _gravity * num * num * 0.5f;
            if (_yAvoidance != 0f && num2 < 0.25f)
            {
                float num3 = 0.5f - Mathf.Cos(num2 * 8f * 3.1415927f) * 0.5f;
                _localPosition.y = _localPosition.y + num3 * _yAvoidance;
            }

            _localPosition = DefiniteNoteJump(_localPosition, num2);

            //We do this so that when the Note is Reinited it will still recalculate the rotation
            float num2Clamped = Mathf.Clamp(num2, 0f, 0.5f);
            Quaternion quaternion;
            if (num2Clamped < 0.125f)
            {
                quaternion = Quaternion.Slerp(
                    _startRotation,
                    _middleRotation,
                    Mathf.Sin(num2Clamped * 3.1415927f * 4f)
                );
            }
            else
            {
                quaternion = Quaternion.Slerp(
                    _middleRotation,
                    _endRotation,
                    Mathf.Sin((num2Clamped - 0.125f) * 3.1415927f * 2f)
                );
            }

            _rotatedObject().GetVisualRoot().transform.localRotation = quaternion;

            if (num2 >= 0.5f && !_halfJumpMarkReported)
            {
                _halfJumpMarkReported = true;
            }
            if (num2 >= 0.75f && !_threeQuartersMarkReported)
            {
                _threeQuartersMarkReported = true;
            }
            if (songTime >= _missedTime && !_missedMarkReported)
            {
                _missedMarkReported = true;
            }
            if (_threeQuartersMarkReported && (!_definitePosition || !hasNoodle))
            {
                float num4 = (num2 - 0.75f) / 0.25f;
                num4 = num4 * num4 * num4;
                _localPosition.z -= Mathf.LerpUnclamped(0f, _endDistanceOffset, num4);
            }
            if (num2 >= 1f && !_missedMarkReported)
            {
                _missedMarkReported = true;
            }
            Vector3 vector3 = _worldRotation * _localPosition;
            transform.localPosition = vector3;
            return vector3;
        }
    }
}
