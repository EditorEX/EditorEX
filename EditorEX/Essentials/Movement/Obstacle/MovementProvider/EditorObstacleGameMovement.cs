using System;
using System.Collections.Generic;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Visuals;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using Heck.Animation;
using NoodleExtensions;
using NoodleExtensions.Animation;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.Obstacle.MovementProvider
{
    public class EditorObstacleGameMovement : MonoBehaviour, IObjectMovement
    {
        // Injected fields
        private IEditorBeatmapObjectSpawnMovementData _editorBeatmapObjectSpawnMovementData = null!;
        private EditorDeserializedData _editorDeserializedData = null!;
        private AnimationHelper _animationHelper = null!;
        private IReadonlyBeatmapState _state = null!;
        private ColorManager _colorManager = null!;
        private IAudioTimeSource _audioTimeSyncController = null!;
        private AudioDataModel _audioDataModel = null!;
        private IVariableMovementDataProvider _variableMovementDataProvider = null!;
        private SiraLog _siraLog = null!;

        // Obstacle related fields
        private ObstacleEditorData? _editorData;

        // Resolved once per Init (when _editorData changes) and reused on the per-frame update path
        // (NoodleUpdate + NoodleGetPosForTime) instead of two dictionary lookups every frame.
        private EditorNoodleObstacleData? _noodleData;

        private float _width;
        private float _height;
        private float _length;
        private Vector3 _moveOffset;
        private Vector3 _startPos;
        private Vector3 _midPos;
        private Vector3 _endPos;
        private float _startTimeOffset;
        private float _obstacleDuration;
        private bool _passedThreeQuartersOfMove2Reported;
        private bool _passedAvoidedMarkReported;
        private float _passedThreeQuartersOfMove2Time;
        private float _passedAvoidedMarkTime;
        private float _finishMovementTime;
        private Bounds _bounds;
        private Color _color;
        private Quaternion _worldRotation;

        // Object fields
        private StretchableObstacle? _stretchableObstacle;
        private ObstacleViewSelection? _selection;

        [Inject]
        private void Construct(
            [InjectOptional(Id = "NoodleExtensions")] EditorDeserializedData editorDeserializedData,
            AnimationHelper animationHelper,
            IReadonlyBeatmapState state,
            ColorManager colorManager,
            IAudioTimeSource audioTimeSyncController,
            AudioDataModel audioDataModel,
            SiraLog siraLog
        )
        {
            _editorDeserializedData = editorDeserializedData;
            _animationHelper = animationHelper;
            _state = state;
            _colorManager = colorManager;
            _audioTimeSyncController = audioTimeSyncController;
            _audioDataModel = audioDataModel;
            _siraLog = siraLog;
        }

        private Quaternion GetWorldRotation(ObstacleEditorData? obstacleData, float @default)
        {
            Quaternion worldRotation = Quaternion.Euler(0, @default, 0);

            if (
                !_editorDeserializedData.Resolve(
                    obstacleData,
                    out EditorNoodleObstacleData? noodleData
                )
                || noodleData == null
            )
            {
                return worldRotation;
            }

            Quaternion? worldRotationQuaternion = noodleData.WorldRotationQuaternion;
            if (worldRotationQuaternion.HasValue)
            {
                worldRotation = worldRotationQuaternion.Value;
            }

            noodleData.InternalWorldRotation = worldRotation;

            return worldRotation;
        }

        private float GetCustomWidth(float @default, ObstacleEditorData? obstacleData)
        {
            EditorNoodleObstacleData? noodleData = null;
            _editorDeserializedData?.Resolve(obstacleData, out noodleData);
            return noodleData?.Width ?? @default;
        }

        private float GetCustomLength(float @default, ObstacleEditorData? obstacleData)
        {
            EditorNoodleObstacleData? noodleData = null;
            _editorDeserializedData?.Resolve(obstacleData, out noodleData);
            return noodleData?.Length * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance
                ?? @default;
        }

        public void Init(
            BaseEditorData? editorData,
            IVariableMovementDataProvider variableMovementDataProvider,
            EditorBasicBeatmapObjectSpawnMovementData movementData,
            Func<IObjectVisuals>? getVisualRoot
        )
        {
            _stretchableObstacle = transform
                .Find("GameWallRoot")
                .GetComponent<StretchableObstacle>();
            _selection = GetComponent<ObstacleViewSelection>();

            _variableMovementDataProvider = variableMovementDataProvider;

            _editorBeatmapObjectSpawnMovementData = movementData;
            _editorData = editorData as ObstacleEditorData;
            _noodleData = null;
            if (_editorData == null)
            {
                _siraLog.Error("EditorObstacleGameMovement: Null editorData");
                return;
            }

            var obstacleSpawnData = _editorBeatmapObjectSpawnMovementData.GetObstacleSpawnData(
                _editorData
            );

            float worldRotation = 0f;

            _worldRotation = GetWorldRotation(_editorData, worldRotation);
            _obstacleDuration = _editorData.duration;
            _height = obstacleSpawnData.obstacleHeight;
            _color = _colorManager.obstaclesColor;
            _width = GetCustomWidth(obstacleSpawnData.obstacleWidth, _editorData);
            _moveOffset = obstacleSpawnData.moveOffset;

            Vector3 startPos = _variableMovementDataProvider.moveStartPosition + _moveOffset;
            Vector3 midPos = _variableMovementDataProvider.moveEndPosition + _moveOffset;
            Vector3 endPos = _variableMovementDataProvider.jumpEndPosition + _moveOffset;
            float num = (endPos - midPos).magnitude / _variableMovementDataProvider.jumpDuration;
            _length = GetCustomLength(num * _editorData.duration, _editorData);

            _stretchableObstacle.SetAllProperties(
                _width * 0.98f,
                _height,
                _length,
                _color,
                _state.beat
            );
            _selection.SetObstacleData(_width, _height, _length);
            _selection.UpdateState();
            _bounds = _stretchableObstacle.bounds;
            _passedThreeQuartersOfMove2Reported = false;
            _passedAvoidedMarkReported = false;
            transform.localPosition = startPos;
            transform.localRotation = _worldRotation;

            _editorDeserializedData.Resolve(editorData, out EditorNoodleObstacleData? noodleData);
            _noodleData = noodleData;
            if (noodleData == null)
            {
                return;
            }

            Quaternion? localRotationQuaternion = noodleData.LocalRotationQuaternion;

            Quaternion localRotation = Quaternion.identity;
            if (localRotationQuaternion.HasValue)
            {
                localRotation = localRotationQuaternion.Value;
                transform.localRotation = _worldRotation * localRotation;
            }

            transform.localScale = Vector3.one; // This is a fix for animation due to obstacles being recycled

            if (noodleData is { Uninteractable: true })
            {
                _bounds.size = Vector3.zero;
            }
            else
            {
                //_obstacleTracker.AddActive(__instance);
            }

            noodleData.InternalStartPos = startPos;
            noodleData.InternalMidPos = midPos;
            noodleData.InternalEndPos = endPos;
            noodleData.InternalLocalRotation = localRotation;
            noodleData.InternalBoundsSize = _bounds.size;

            Vector3 noteOffset = endPos;
            noteOffset.z = 0;
            noodleData.InternalNoteOffset = noteOffset;
        }

        public void Enable() { }

        public void Disable() { }

        public void Setup(BaseEditorData? editorData) { }

        private bool NoodleGetPosForTime(float time, out Vector3 __result)
        {
            __result = default;
            EditorNoodleObstacleData? noodleData = _noodleData;
            if (noodleData == null)
            {
                return false;
            }

            float moveDuration = _variableMovementDataProvider.moveDuration;

            float jumpTime = Mathf.Clamp(
                (time - moveDuration)
                    / (_variableMovementDataProvider.jumpDuration + _obstacleDuration),
                0,
                1
            );
            _animationHelper.GetDefinitePositionOffset(
                noodleData.AnimationObject,
                noodleData.Track,
                jumpTime,
                out Vector3? position
            );

            if (!position.HasValue)
            {
                return false;
            }

            Vector3 noteOffset = noodleData.InternalNoteOffset;
            Vector3 definitePosition = position.Value + noteOffset;
            if (time < moveDuration)
            {
                __result = Vector3.LerpUnclamped(_startPos, _midPos, time / moveDuration);
                __result += definitePosition - _midPos;
            }
            else
            {
                __result = definitePosition;
            }

            return true;
        }

        private Vector3 GetPosForTime(float time)
        {
            if (NoodleGetPosForTime(time, out Vector3 result))
            {
                return result;
            }

            float moveDuration = _variableMovementDataProvider.moveDuration;

            Vector3 vector;
            if (time < moveDuration)
            {
                vector = Vector3.LerpUnclamped(
                    _startPos,
                    _midPos,
                    (moveDuration < Mathf.Epsilon) ? 0f : (time / moveDuration)
                );
            }
            else
            {
                float num = (time - moveDuration) / _variableMovementDataProvider.jumpDuration;
                vector.x = _startPos.x;
                vector.y = _startPos.y;
                vector.z = Mathf.LerpUnclamped(_midPos.z, _endPos.z, num);
                if (_passedAvoidedMarkReported)
                {
                    float num2 =
                        (time - _passedAvoidedMarkTime)
                        / (_finishMovementTime - _passedAvoidedMarkTime);
                    num2 = num2 * num2 * num2;
                    vector.z -= Mathf.LerpUnclamped(0f, 500f, num2);
                }
            }
            return vector;
        }

        public void NoodleUpdate()
        {
            EditorNoodleObstacleData? noodleData = _noodleData;
            if (noodleData == null)
            {
                return;
            }

            if (noodleData.InternalDoUnhide)
            {
                //Hide(false);
            }

            IReadOnlyList<Track>? tracks = noodleData.Track;
            NoodleObjectData.AnimationObjectData? animationObject = noodleData.AnimationObject;
            if (tracks == null && animationObject == null)
            {
                return;
            }

            float? time = noodleData.GetTimeProperty();
            float normalTime;
            if (time.HasValue)
            {
                normalTime = time.Value;
            }
            else
            {
                float elapsedTime = _audioTimeSyncController.songTime - _startTimeOffset;
                normalTime =
                    (elapsedTime - _variableMovementDataProvider.moveDuration)
                    / (_variableMovementDataProvider.jumpDuration + _obstacleDuration);
            }

            _animationHelper.GetObjectOffset(
                animationObject,
                tracks,
                normalTime,
                out Vector3? positionOffset,
                out Quaternion? rotationOffset,
                out Vector3? scaleOffset,
                out Quaternion? localRotationOffset,
                out float? dissolve,
                out _,
                out float? cuttable
            );

            if (positionOffset.HasValue)
            {
                Vector3 startPos = noodleData.InternalStartPos;
                Vector3 midPos = noodleData.InternalMidPos;
                Vector3 endPos = noodleData.InternalEndPos;

                Vector3 offset = positionOffset.Value;
                _startPos = startPos + offset;
                _midPos = midPos + offset;
                _endPos = endPos + offset;
            }

            if (rotationOffset.HasValue || localRotationOffset.HasValue)
            {
                Quaternion worldRotation = noodleData.InternalWorldRotation;
                Quaternion localRotation = noodleData.InternalLocalRotation;

                Quaternion worldRotationQuatnerion = worldRotation;
                if (rotationOffset.HasValue)
                {
                    worldRotationQuatnerion *= rotationOffset.Value;
                    _worldRotation = worldRotationQuatnerion;
                }

                worldRotationQuatnerion *= localRotation;

                if (localRotationOffset.HasValue)
                {
                    worldRotationQuatnerion *= localRotationOffset.Value;
                }
                transform.localRotation = worldRotationQuatnerion;
            }

            if (cuttable.HasValue)
            {
                if (cuttable.Value >= 1)
                {
                    _bounds.size = Vector3.zero;
                }
                else
                {
                    Vector3 boundsSize = noodleData.InternalBoundsSize;
                    _bounds.size = boundsSize;
                }
            }

            if (scaleOffset.HasValue)
            {
                transform.localScale = scaleOffset.Value;
            }
        }

        public void ManualUpdate()
        {
            if (_editorData == null)
            {
                return;
            }

            // Recompute movement timings/positions until the obstacle passes the avoided mark,
            // mirroring the gameplay ObstacleController (the variable movement data can change
            // between frames). NoodleUpdate runs afterwards so animation offsets layer on top of
            // these base positions.
            if (!_passedAvoidedMarkReported)
            {
                float moveDuration = _variableMovementDataProvider.moveDuration;
                float jumpDuration = _variableMovementDataProvider.jumpDuration;
                float halfJumpDuration = _variableMovementDataProvider.halfJumpDuration;

                _startTimeOffset =
                    _audioDataModel.bpmData.BeatToSeconds(_editorData.beat)
                    - moveDuration
                    - halfJumpDuration;
                _passedThreeQuartersOfMove2Time = moveDuration + jumpDuration * 0.75f;
                _passedAvoidedMarkTime =
                    moveDuration + halfJumpDuration + _obstacleDuration + 0.15f;
                _finishMovementTime = moveDuration + jumpDuration + _obstacleDuration;
                _startPos = _variableMovementDataProvider.moveStartPosition + _moveOffset;
                _midPos = _variableMovementDataProvider.moveEndPosition + _moveOffset;
                _endPos = _variableMovementDataProvider.jumpEndPosition + _moveOffset;
            }

            NoodleUpdate();

            float num = _audioTimeSyncController.songTime - _startTimeOffset;
            Vector3 posForTime = GetPosForTime(num);
            transform.localPosition = _worldRotation * posForTime;
            if (!_passedThreeQuartersOfMove2Reported && num > _passedThreeQuartersOfMove2Time)
            {
                _passedThreeQuartersOfMove2Reported = true;
            }
            if (!_passedAvoidedMarkReported && num > _passedAvoidedMarkTime)
            {
                _passedAvoidedMarkReported = true;
            }
        }
    }
}
