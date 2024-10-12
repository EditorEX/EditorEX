using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapSaveDataVersion2_6_0AndEarlier;
using EditorEX.CustomJSONData;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.SpawnProcessing;
using EditorEX.Essentials.Visuals;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using Heck.Animation;
using Heck.Deserialize;
using Newtonsoft.Json;
using NoodleExtensions;
using NoodleExtensions.Animation;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.Note.MovementProvider
{
    public class EditorNoteGameMovement : MonoBehaviour, IObjectMovement
    {
        // Properties

        public MovementPhase movementPhase { get; private set; }

        public float distanceToPlayer
        {
            get
            {
                if (movementPhase != MovementPhase.Jumping)
                {
                    return _floorMovement.distanceToPlayer;
                }
                return _jump.distanceToPlayer;
            }
        }

        // Movement related fields

        private EditorNoteFloorMovement _floorMovement;
        private EditorNoteJump _jump;
        private IEditorBeatmapObjectSpawnMovementData _editorBeatmapObjectSpawnMovementData;

        private float _zOffset = 0.25f;
        private Vector3 _position;
        private Vector3 _prevPosition;
        private Vector3 _localPosition;
        private Vector3 _prevLocalPosition;

        // Injected fields

        private EditorDeserializedData _editorDeserializedData;
        private AnimationHelper _animationHelper;
        private IReadonlyBeatmapState _state;
        private AudioDataModel _audioDataModel;

        [Inject]
        private void Construct(
            [InjectOptional(Id = "NoodleExtensions")] EditorDeserializedData editorDeserializedData,
            AnimationHelper animationHelper,
            IReadonlyBeatmapState state,
            AudioDataModel audioDataModel)
        {
            _editorDeserializedData = editorDeserializedData;
            _animationHelper = animationHelper;
            _state = state;
            _audioDataModel = audioDataModel;
        }

        public void Init(BaseEditorData editorData, EditorBasicBeatmapObjectSpawnMovementData movementData, Func<IObjectVisuals> getVisualRoot)
        {
            EditorNoodleBaseNoteData? noodleData = null;
            _editorDeserializedData?.Resolve(editorData, out noodleData);

            _editorBeatmapObjectSpawnMovementData = movementData;
            var noteEditorData = editorData as NoteEditorData;
            var spawnDataAssociation = EditorSpawnDataRepository.GetSpawnData(editorData);
            var noteSpawnData = _editorBeatmapObjectSpawnMovementData.GetJumpingNoteSpawnData(noteEditorData);

            float beatTime = _audioDataModel.bpmData.BeatToSeconds(editorData.beat);
            float worldRotation = 0f;
            Vector3 moveStartPos = noteSpawnData.moveStartPos;
            Vector3 moveEndPos = noteSpawnData.moveEndPos;
            Vector3 jumpEndPos = noteSpawnData.jumpEndPos;
            float moveDuration = noteSpawnData.moveDuration;
            float jumpDuration = noteSpawnData.jumpDuration;
            float jumpGravity = noteSpawnData.jumpGravity;
            float flipYSide = noodleData?.InternalFlipYSide ?? spawnDataAssociation.flipYSide;
            float endRotation = noteEditorData.cutDirection.RotationAngle() + spawnDataAssociation.cutDirectionAngleOffset;

            moveStartPos.z += _zOffset;
            moveEndPos.z += _zOffset;
            jumpEndPos.z += _zOffset;

            _floorMovement.Init(editorData as NoteEditorData, worldRotation, moveStartPos, moveEndPos, moveDuration, beatTime - moveDuration - jumpDuration * 0.5f, getVisualRoot);
            _position = _floorMovement.SetToStart();
            _prevPosition = _position;
            _localPosition = (_prevLocalPosition = _floorMovement.localPosition);

            _jump.Init(editorData as NoteEditorData, beatTime, worldRotation, moveEndPos, jumpEndPos, jumpDuration, jumpGravity, flipYSide, endRotation, getVisualRoot);

            movementPhase = MovementPhase.MovingOnTheFloor;

            //NoteController.Init postfix for noodle(lol)

            if (noodleData != null)
            {
                float zOffset = _zOffset;
                moveStartPos.z += zOffset;
                moveEndPos.z += zOffset;
                jumpEndPos.z += zOffset;

                EditorNoteJump noteJump = _jump;
                EditorNoteFloorMovement floorMovement = _floorMovement;

                Quaternion? worldRotationQuaternion = noodleData.WorldRotationQuaternion;
                Quaternion? localRotationQuaternion = noodleData.LocalRotationQuaternion;

                Quaternion localRotation = Quaternion.identity;
                if (worldRotationQuaternion.HasValue || localRotationQuaternion.HasValue)
                {
                    if (localRotationQuaternion.HasValue)
                    {
                        localRotation = localRotationQuaternion.Value;
                    }

                    if (worldRotationQuaternion.HasValue)
                    {
                        Quaternion quatVal = worldRotationQuaternion.Value;
                        Quaternion inverseWorldRotation = Quaternion.Inverse(quatVal);
                        noteJump._worldRotation = quatVal;
                        noteJump._inverseWorldRotation = inverseWorldRotation;
                        floorMovement._worldRotation = quatVal;
                        floorMovement._inverseWorldRotation = inverseWorldRotation;

                        quatVal *= localRotation;

                        transform.localRotation = quatVal;
                    }
                    else
                    {
                        transform.localRotation *= localRotation;
                    }
                }

                transform.localScale = Vector3.one; // This is a fix for animation due to notes being recycled

                noodleData.InternalEndRotation = endRotation;
                noodleData.InternalStartPos = moveStartPos;
                noodleData.InternalMidPos = moveEndPos;
                noodleData.InternalEndPos = jumpEndPos;
                noodleData.InternalWorldRotation = _floorMovement.worldRotation;
                noodleData.InternalLocalRotation = localRotation;

                float num2 = jumpDuration * 0.5f;
                float startVerticalVelocity = jumpGravity * num2;
                float yOffset = (startVerticalVelocity * num2) - (jumpGravity * num2 * num2 * 0.5f);
                noodleData.InternalNoteOffset = new Vector3(jumpEndPos.x, moveEndPos.y + yOffset, 0);
            }
        }

        public void Enable()
        {

        }

        public void Disable()
        {

        }

        protected void Awake()
        {
            _floorMovement = GetComponent<EditorNoteFloorMovement>();
            _jump = GetComponent<EditorNoteJump>();

            movementPhase = MovementPhase.None;
            _floorMovement.floorMovementDidFinishEvent += HandleFloorMovementDidFinish;
            _jump.noteJumpDidFinishEvent += HandleNoteJumpDidFinish;
        }

        protected void OnDestroy()
        {
            if (_floorMovement)
            {
                _floorMovement.floorMovementDidFinishEvent -= HandleFloorMovementDidFinish;
            }
            if (_jump)
            {
                _jump.noteJumpDidFinishEvent -= HandleNoteJumpDidFinish;
            }
        }

        private void HandleFloorMovementDidFinish()
        {
            movementPhase = MovementPhase.Jumping;
            _position = _jump.ManualUpdate();
            _localPosition = _jump.localPosition;
        }

        private void HandleNoteJumpDidFinish()
        {
            movementPhase = MovementPhase.None;
        }

        public void Setup(BaseEditorData editorData)
        {
            EditorNoodleBaseNoteData? noodleData = null;
            if (!(_editorDeserializedData?.Resolve(editorData, out noodleData) ?? false))
            {
                return;
            }

            List<Track>? tracks = noodleData.Track;
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
                float jumpDuration = _jump.jumpDuration;
                float elapsedTime = _audioDataModel.bpmData.BeatToSeconds(_state.beat) - (_audioDataModel.bpmData.BeatToSeconds(editorData.beat) - (jumpDuration * 0.5f));
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
                out float? dissolve,
                out float? dissolveArrow,
                out float? cuttable);

            if (positionOffset.HasValue)
            {
                Vector3 moveStartPos = noodleData.InternalStartPos;
                Vector3 moveEndPos = noodleData.InternalMidPos;
                Vector3 jumpEndPos = noodleData.InternalEndPos;

                Vector3 offset = positionOffset.Value;
                _floorMovement._startPos = moveStartPos + offset;
                _floorMovement._endPos = moveEndPos + offset;
                _jump._startPos = moveEndPos + offset;
                _jump._endPos = jumpEndPos + offset;
            }

            if (rotationOffset.HasValue || localRotationOffset.HasValue)
            {
                Quaternion worldRotation = noodleData.InternalWorldRotation;
                Quaternion localRotation = noodleData.InternalLocalRotation;

                Quaternion worldRotationQuatnerion = worldRotation;
                if (rotationOffset.HasValue)
                {
                    worldRotationQuatnerion *= rotationOffset.Value;
                    Quaternion inverseWorldRotation = Quaternion.Inverse(worldRotationQuatnerion);
                    _jump._worldRotation = worldRotationQuatnerion;
                    _jump._inverseWorldRotation = inverseWorldRotation;
                    _floorMovement._worldRotation = worldRotationQuatnerion;
                    _floorMovement._inverseWorldRotation = inverseWorldRotation;
                }

                worldRotationQuatnerion *= localRotation;

                if (localRotationOffset.HasValue)
                {
                    worldRotationQuatnerion *= localRotationOffset.Value;
                }

                transform.localRotation = worldRotationQuatnerion;
            }

            if (scaleOffset.HasValue)
            {
                transform.localScale = scaleOffset.Value;
            }
        }

        public void ManualUpdate()
        {
            _prevPosition = _position;
            _prevLocalPosition = _localPosition;
            if (movementPhase == MovementPhase.MovingOnTheFloor)
            {
                _position = _floorMovement.ManualUpdate();
                _localPosition = _floorMovement.localPosition;
                return;
            }
            _position = _jump.ManualUpdate();
            _localPosition = _jump.localPosition;
        }

        public enum MovementPhase
        {
            None,
            MovingOnTheFloor,
            Jumping
        }
    }
}
