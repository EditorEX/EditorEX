using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Patches;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using JetBrains.Annotations;
using NoodleExtensions;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace EditorEX.NoodleExtensions.Managers
{
    internal class EditorSpawnDataManager
    {
        private readonly SiraLog _siraLog;
        private readonly EditorDeserializedData _editorDeserializedData;
        private readonly EditorBasicBeatmapObjectSpawnMovementData _movementData;

        [UsedImplicitly]
        private EditorSpawnDataManager(
            SiraLog siraLog,
            EditorBasicBeatmapObjectSpawnMovementData editorBasicBeatmapObjectSpawnMovementData,
            [Inject(Id = NoodleController.ID)] EditorDeserializedData deserializedData)
        {
            _siraLog = siraLog;
            _movementData = editorBasicBeatmapObjectSpawnMovementData;
            _editorDeserializedData = deserializedData;
        }

        internal static Vector2 Get2DNoteOffset(float lineIndex, int noteLinesCount, float lineLayer)
        {
            float distance = -(noteLinesCount - 1f) * 0.5f;
            return new Vector2((distance + lineIndex) * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance, LineYPosForLineLayer(lineLayer));
        }

        internal bool GetObstacleSpawnData(ObstacleEditorData obstacleData, out BeatmapObjectSpawnMovementData.ObstacleSpawnData? result)
        {
            if (!_editorDeserializedData.Resolve(obstacleData, out EditorNoodleObstacleData? noodleData))
            {
                result = null;
                return false;
            }

            float? njs = noodleData.NJS;
            float? spawnoffset = noodleData.SpawnOffset;

            float lineIndex = noodleData.StartX + (_movementData.noteLinesCount / 2) ?? obstacleData.column;
            float lineLayer = noodleData.StartY ?? (float)obstacleData.row;

            Vector3 obstacleOffset = GetObstacleOffset(lineIndex, lineLayer);
            obstacleOffset.y += _movementData.jumpOffsetY;

            float? height = noodleData.Height;
            float obstacleHeight;
            if (height.HasValue)
            {
                obstacleHeight = height.Value * StaticBeatmapObjectSpawnMovementData.layerHeight;
            }
            else
            {
                // _topObstaclePosY =/= _obstacleTopPosY
                obstacleHeight = Mathf.Min(
                    obstacleData.height * StaticBeatmapObjectSpawnMovementData.layerHeight,
                    _movementData._obstacleTopPosY - obstacleOffset.y);
            }

            GetNoteJumpValues(
                njs,
                spawnoffset,
                out float jumpDuration,
                out _,
                out Vector3 moveStartPos,
                out Vector3 moveEndPos,
                out Vector3 jumpEndPos);

            result = new BeatmapObjectSpawnMovementData.ObstacleSpawnData(
                moveStartPos + obstacleOffset,
                moveEndPos + obstacleOffset,
                jumpEndPos + obstacleOffset,
                obstacleHeight,
                _movementData.moveDuration,
                jumpDuration,
                StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance);

            return true;
        }

        internal bool GetJumpingNoteSpawnData(NoteEditorData noteData, out BeatmapObjectSpawnMovementData.NoteSpawnData? result)
        {
            if (!_editorDeserializedData.Resolve(noteData, out EditorNoodleBaseNoteData? noodleData))
            {
                result = null;
                return false;
            }

            float? njs = noodleData.NJS;
            float? spawnoffset = noodleData.SpawnOffset;

            bool gravityOverride = noodleData.DisableGravity;

            float offset = _movementData.noteLinesCount / 2f;
            float? flipLineIndex = noodleData.InternalFlipLineIndex;
            float lineIndex = noodleData.StartX + offset ?? noteData.column;
            float lineLayer = noodleData.StartY ?? (float)noteData.row;
            float startlinelayer = noodleData.InternalStartNoteLineLayer;

            Vector3 noteOffset = GetNoteOffset(lineIndex, startlinelayer);
            GetNoteJumpValues(
                njs,
                spawnoffset,
                out float jumpDuration,
                out float jumpDistance,
                out Vector3 moveStartPos,
                out Vector3 moveEndPos,
                out Vector3 jumpEndPos);

            NoteJumpGravityForLineLayer(
                lineLayer,
                startlinelayer,
                jumpDistance,
                njs,
                out float jumpGravity,
                out float noGravity);

            Vector3 noteOffset2 = GetNoteOffset(
                flipLineIndex ?? lineIndex,
                gravityOverride ? lineLayer : startlinelayer);

            result = new BeatmapObjectSpawnMovementData.NoteSpawnData(
                moveStartPos + noteOffset2,
                moveEndPos + noteOffset2,
                jumpEndPos + noteOffset,
                gravityOverride ? noGravity : jumpGravity,
                _movementData.moveDuration,
                jumpDuration);

            return true;
        }

        internal bool GetSliderSpawnData(BaseSliderEditorData sliderData, out BeatmapObjectSpawnMovementData.SliderSpawnData? result)
        {
            if (!_editorDeserializedData.Resolve(sliderData, out EditorNoodleSliderData? noodleData))
            {
                result = null;
                return false;
            }

            float? njs = noodleData.NJS;
            float? spawnoffset = noodleData.SpawnOffset;

            bool gravityOverride = noodleData.DisableGravity;

            float offset = _movementData.noteLinesCount / 2f;
            float headLineIndex = noodleData.StartX + offset ?? sliderData.column;
            float headLineLayer = noodleData.StartY ?? (float)sliderData.row;
            float headStartlinelayer = noodleData.InternalStartNoteLineLayer;
            float tailLineIndex = noodleData.TailStartX + offset ?? sliderData.tailColumn;
            float tailLineLayer = noodleData.TailStartY ?? (float)sliderData.tailRow;
            float tailStartlinelayer = noodleData.InternalTailStartNoteLineLayer;

            Vector3 headOffset = GetNoteOffset(headLineIndex, gravityOverride ? headLineLayer : headStartlinelayer);
            Vector3 tailOffset = GetNoteOffset(tailLineIndex, gravityOverride ? tailLineLayer : tailStartlinelayer);
            GetNoteJumpValues(
                njs,
                spawnoffset,
                out float jumpDuration,
                out float jumpDistance,
                out Vector3 moveStartPos,
                out Vector3 moveEndPos,
                out Vector3 jumpEndPos);

            NoteJumpGravityForLineLayer(
                headLineLayer,
                headStartlinelayer,
                jumpDistance,
                njs,
                out float headJumpGravity,
                out float headNoGravity);

            NoteJumpGravityForLineLayer(
                tailLineLayer,
                tailStartlinelayer,
                jumpDistance,
                njs,
                out float tailJumpGravity,
                out float tailNoGravity);

            result = new BeatmapObjectSpawnMovementData.SliderSpawnData(
                moveStartPos + headOffset,
                moveEndPos + headOffset,
                jumpEndPos + headOffset,
                gravityOverride ? headNoGravity : headJumpGravity,
                moveStartPos + tailOffset,
                moveEndPos + tailOffset,
                jumpEndPos + tailOffset,
                gravityOverride ? tailNoGravity : tailJumpGravity,
                _movementData.moveDuration,
                jumpDuration);

            return true;
        }

        internal float GetSpawnAheadTime(float? inputNjs, float? inputOffset)
        {
            return _movementData.moveDuration + (GetJumpDuration(inputNjs, inputOffset) * 0.5f);
        }

        private static float LineYPosForLineLayer(float height)
        {
            return StaticBeatmapObjectSpawnMovementData.kBaseLinesYPos
                   + (height * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance); // offset by 0.25
        }

        private Vector3 GetNoteOffset(float lineIndex, float lineLayer)
        {
            Vector2 coords = Get2DNoteOffset(lineIndex, _movementData.noteLinesCount, lineLayer);
            return (_movementData._rightVec * coords.x)
                   + new Vector3(0, coords.y, 0);
        }

        private Vector3 GetObstacleOffset(float lineIndex, float lineLayer)
        {
            Vector3 result = GetNoteOffset(lineIndex, lineLayer);
            result.y += StaticBeatmapObjectSpawnMovementData.kObstacleVerticalOffset;
            return result;
        }

        private void NoteJumpGravityForLineLayer(
            float lineLayer,
            float startLineLayer,
            float jumpDistance,
            float? njs,
            out float gravity,
            out float noGravity)
        {
            float lineYPos = LineYPosForLineLayer(lineLayer);
            float startLayerLineYPos = LineYPosForLineLayer(startLineLayer);

            // HighestJumpPosYForLineLayer
            // Magic numbers below found with linear regression y=mx+b using existing HighestJumpPosYForLineLayer values
            float highestJump = (0.875f * lineYPos) + 0.639583f + _movementData.jumpOffsetY;

            // NoteJumpGravityForLineLayer
            float num = jumpDistance / (njs ?? _movementData.noteJumpMovementSpeed) * 0.5f;
            num = 2 / (num * num);
            gravity = GetJumpGravity(startLayerLineYPos);
            noGravity = GetJumpGravity(lineYPos);
            return;
            float GetJumpGravity(float gravityLineYPos) => (highestJump - gravityLineYPos) * num;
        }

        private float GetJumpDuration(
            float? inputNjs,
            float? inputOffset)
        {
            if (!inputNjs.HasValue && !inputOffset.HasValue && _movementData._noteJumpValueType == BeatmapObjectSpawnMovementData.NoteJumpValueType.JumpDuration)
            {
                return _movementData.jumpDuration;
            }

            float oneBeatDuration = 60f / PopulateBeatmap._beatmapLevelDataModel.beatsPerMinute;
            float halfJumpDurationInBeats = CoreMathUtils.CalculateHalfJumpDurationInBeats(
                _movementData._startHalfJumpDurationInBeats,
                _movementData._maxHalfJumpDistance,
                inputNjs ?? _movementData.noteJumpMovementSpeed,
                oneBeatDuration,
                inputOffset ?? _movementData._noteJumpStartBeatOffset);
            return oneBeatDuration * halfJumpDurationInBeats * 2f;
        }

        private void GetNoteJumpValues(
            float? inputNjs,
            float? inputOffset,
            out float jumpDuration,
            out float jumpDistance,
            out Vector3 moveStartPos,
            out Vector3 moveEndPos,
            out Vector3 jumpEndPos)
        {
            jumpDuration = GetJumpDuration(inputNjs, inputOffset);

            Vector3 centerPos = _movementData.centerPos;
            Vector3 forwardVec = _movementData._forwardVec;

            jumpDistance = (inputNjs ?? _movementData.noteJumpMovementSpeed) * jumpDuration;
            moveEndPos = centerPos + (forwardVec * (jumpDistance * 0.5f));
            jumpEndPos = centerPos - (forwardVec * (jumpDistance * 0.5f));
            moveStartPos = centerPos + (forwardVec * (_movementData._moveDistance + (jumpDistance * 0.5f)));
        }
    }
}
