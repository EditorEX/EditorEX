using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Visuals;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using Heck.Animation;
using NoodleExtensions;
using NoodleExtensions.Animation;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.Obstacle.MovementProvider
{
    public class EditorObstacleGameMovement : MonoBehaviour, IObjectMovement
    {
        // Injected fields
        private IEditorBeatmapObjectSpawnMovementData _editorBeatmapObjectSpawnMovementData;
        private EditorDeserializedData _editorDeserializedData;
        private AnimationHelper _animationHelper;
        private IReadonlyBeatmapState _state;
        private ColorManager _colorManager;
        private IAudioTimeSource _audioTimeSyncController;
        private AudioDataModel _audioDataModel;
        private IVariableMovementDataProvider _variableMovementDataProvider;

        // Obstacle related fields
        private ObstacleEditorData? _editorData;
        private float _width;
        private float _height;
        private float _length;
        private Vector3 _startPos;
        private Vector3 _midPos;
        private Vector3 _endPos;
        private float _move1Duration;
        private float _move2Duration;
        private float _startTimeOffset;
        private float _obstacleDuration;
        private bool _passedThreeQuartersOfMove2Reported;
        private bool _passedAvoidedMarkReported;
        private float _passedAvoidedMarkTime;
        private float _finishMovementTime;
        private Bounds _bounds;
        private bool _dissolving;
        private Color _color;
        private Quaternion _worldRotation;
        private Quaternion _inverseWorldRotation;

        // Object fields
        private StretchableObstacle _stretchableObstacle;
        private ObstacleViewSelection _selection;

        [Inject]
        private void Construct([InjectOptional(Id = "NoodleExtensions")] EditorDeserializedData editorDeserializedData,
            AnimationHelper animationHelper,
            IReadonlyBeatmapState state,
            ColorManager colorManager,
            IAudioTimeSource audioTimeSyncController,
            AudioDataModel audioDataModel,
            IVariableMovementDataProvider variableMovementDataProvider)
        {
            _editorDeserializedData = editorDeserializedData;
            _animationHelper = animationHelper;
            _state = state;
            _colorManager = colorManager;
            _audioTimeSyncController = audioTimeSyncController;
            _audioDataModel = audioDataModel;
            _variableMovementDataProvider = variableMovementDataProvider;
        }

        private Quaternion GetWorldRotation(ObstacleEditorData? obstacleData, float @default)
        {
            Quaternion worldRotation = Quaternion.Euler(0, @default, 0);

            if (!(_editorDeserializedData?.Resolve(obstacleData, out EditorNoodleObstacleData? noodleData) ?? false))
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
            return noodleData?.Length * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance ?? @default;
        }

        public void Init(BaseEditorData? editorData, EditorBasicBeatmapObjectSpawnMovementData movementData, Func<IObjectVisuals> getVisualRoot)
        {
            _stretchableObstacle = transform.Find("GameWallRoot").GetComponent<StretchableObstacle>();
            _selection = GetComponent<ObstacleViewSelection>();

            _editorBeatmapObjectSpawnMovementData = movementData;
            _editorData = editorData as ObstacleEditorData;
            var obstacleSpawnData = _editorBeatmapObjectSpawnMovementData.GetObstacleSpawnData(_editorData);

            float worldRotation = 0f;

            _worldRotation = GetWorldRotation(_editorData, worldRotation);
            _inverseWorldRotation = Quaternion.Inverse(_worldRotation);
            _obstacleDuration = _editorData.duration;
            _height = obstacleSpawnData.obstacleHeight;
            _color = _colorManager.obstaclesColor;
            _width = GetCustomWidth(obstacleSpawnData.obstacleWidth, _editorData);
            Vector3 vector = _variableMovementDataProvider.moveStartPosition + obstacleSpawnData.moveOffset;
            Vector3 vector2 = _variableMovementDataProvider.moveEndPosition + obstacleSpawnData.moveOffset;
            float num = (_variableMovementDataProvider.jumpEndPosition + obstacleSpawnData.moveOffset - vector2).magnitude / _variableMovementDataProvider.jumpDuration;
            _length = GetCustomLength(num * _editorData.duration, _editorData);
            _stretchableObstacle.SetAllProperties(_width * 0.98f, _height, _length, _color, _state.beat);
            _selection.SetObstacleData(_width, _height, _length);
            _selection.UpdateState();
            _bounds = _stretchableObstacle.bounds;
            _passedThreeQuartersOfMove2Reported = false;
            _passedAvoidedMarkReported = false;
            _passedAvoidedMarkTime = _move1Duration + _move2Duration * 0.5f + _obstacleDuration + 0.15f;
            _finishMovementTime = _move1Duration + _move2Duration + _obstacleDuration;
            transform.localPosition = vector;
            transform.localRotation = _worldRotation;

            EditorNoodleObstacleData? noodleData = null;
            if (!(_editorDeserializedData?.Resolve(editorData, out noodleData) ?? false))
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

            noodleData.InternalStartPos = _startPos;
            noodleData.InternalMidPos = _midPos;
            noodleData.InternalEndPos = _endPos;
            noodleData.InternalLocalRotation = localRotation;
            noodleData.InternalBoundsSize = _bounds.size;

            Vector3 noteOffset = _endPos;
            noteOffset.z = 0;
            noodleData.InternalNoteOffset = noteOffset;
        }

        public void Enable()
        {

        }

        public void Disable()
        {

        }

        public void Setup(BaseEditorData? editorData)
        {
        }

        private bool NoodleGetPosForTime(float time, out Vector3 __result)
        {
            __result = default;
            EditorNoodleObstacleData? noodleData = null;
            if (!(_editorDeserializedData?.Resolve(_editorData, out noodleData) ?? false))
            {
                return false;
            }

            float jumpTime = Mathf.Clamp((time - _move1Duration) / (_move2Duration + _obstacleDuration), 0, 1);
            _animationHelper.GetDefinitePositionOffset(noodleData.AnimationObject, noodleData.Track, jumpTime, out Vector3? position);

            if (!position.HasValue)
            {
                return false;
            }

            Vector3 noteOffset = noodleData.InternalNoteOffset;
            Vector3 definitePosition = position.Value + noteOffset;
            if (time < _move1Duration)
            {
                __result = Vector3.LerpUnclamped(_startPos, _midPos, time / _move1Duration);
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

            Vector3 vector;
            if (time < _move1Duration)
            {
                vector = Vector3.LerpUnclamped(_startPos, _midPos, (_move1Duration < Mathf.Epsilon) ? 0f : (time / _move1Duration));
            }
            else
            {
                float num = (time - _move1Duration) / _move2Duration;
                vector.x = _startPos.x;
                vector.y = _startPos.y;
                vector.z = Mathf.LerpUnclamped(_midPos.z, _endPos.z, num);
                if (_passedAvoidedMarkReported)
                {
                    float num2 = (time - _passedAvoidedMarkTime) / (_finishMovementTime - _passedAvoidedMarkTime);
                    num2 = num2 * num2 * num2;
                    vector.z -= Mathf.LerpUnclamped(0f, 500f, num2);
                }
            }
            return vector;
        }

        public void NoodleUpdate()
        {
            EditorNoodleObstacleData? noodleData = null;
            if (!(_editorDeserializedData?.Resolve(_editorData, out noodleData) ?? false) || noodleData == null)
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
                float elapsedTime =  _audioTimeSyncController.songTime - _startTimeOffset;
                normalTime = (elapsedTime - _move1Duration) / (_move2Duration + _obstacleDuration);
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
                out float? cuttable);

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
                    _inverseWorldRotation = Quaternion.Inverse(worldRotationQuatnerion);
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
            NoodleUpdate();

            float num = _audioTimeSyncController.songTime - _startTimeOffset;
            Vector3 posForTime = GetPosForTime(num);
            transform.localPosition = _worldRotation * posForTime;
            if (!_passedThreeQuartersOfMove2Reported && num > _move1Duration + _move2Duration * 0.75f)
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
