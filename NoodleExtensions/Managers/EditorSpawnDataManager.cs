using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using NoodleExtensions;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

// Based from https://github.com/Aeroluna/Heck
namespace EditorEX.NoodleExtensions.Managers
{
    internal class EditorSpawnDataManager
    {
        private readonly SiraLog _siraLog;
        private readonly EditorDeserializedData _editorDeserializedData;
        private readonly EditorBasicBeatmapObjectSpawnMovementData _movementData;

        private EditorSpawnDataManager(
            SiraLog siraLog,
            EditorBasicBeatmapObjectSpawnMovementData editorBasicBeatmapObjectSpawnMovementData,
            [InjectOptional(Id = NoodleController.ID)] EditorDeserializedData deserializedData)
        {
            _siraLog = siraLog;
            _editorDeserializedData = deserializedData;
            _movementData = editorBasicBeatmapObjectSpawnMovementData;
        }

        private static Vector2 Get2DNoteOffset(float lineIndex, int noteLinesCount, float lineLayer)
        {
            float distance = -(noteLinesCount - 1f) * 0.5f;
            return new Vector2((distance + lineIndex) * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance, LineYPosForLineLayer(lineLayer));
        }

        private float GetGravityBase(float noteLineLayer, float beforeJumpLineLayer)
        {
            return HighestJumpPosYForLineLayer(noteLineLayer) - LineYPosForLineLayer(beforeJumpLineLayer);
        }

        private float HighestJumpPosYForLineLayer(float lineLayer)
        {
            // Magic numbers below found with linear regression y=mx+b using existing HighestJumpPosYForLineLayer values
            return (0.525f * lineLayer) + 0.858333f + _movementData._jumpOffsetYProvider.jumpOffsetY;
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

        internal bool GetObstacleSpawnData(ObstacleEditorData? obstacleData, out ObstacleSpawnData? result)
        {
            if (obstacleData == null || !(_editorDeserializedData?.Resolve(obstacleData, out EditorNoodleObstacleData? noodleData) ?? false) || noodleData == null)
            {
                result = null;
                return false;
            }

            float lineIndex = noodleData.StartX + (_movementData.noteLinesCount / 2) ?? obstacleData.column;
            float lineLayer = noodleData.StartY ?? obstacleData.row;

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

            float width = noodleData.Width ?? obstacleData.width;
            width *= StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance;
            obstacleOffset.x += (width - StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance) * 0.5f;
            result = new ObstacleSpawnData(
                obstacleOffset,
                width,
                obstacleHeight);

            return true;
        }

        internal bool GetJumpingNoteSpawnData(NoteEditorData? noteData, out NoteSpawnData? result)
        {
            if (noteData == null || !(_editorDeserializedData?.Resolve(noteData, out EditorNoodleBaseNoteData? noodleData) ?? false) || noodleData == null)
            {
                result = null;
                return false;
            }

            bool gravityOverride = noodleData.DisableGravity;

            float offset = _movementData.noteLinesCount / 2f;
            float? flipLineIndex = noodleData.InternalFlipLineIndex;
            float lineIndex = noodleData.StartX + offset ?? noteData.column;
            float lineLayer = noodleData.StartY ?? noteData.row;
            float startLineLayer = noodleData.InternalStartNoteLineLayer;

            Vector3 noteOffset = GetNoteOffset(lineIndex, startLineLayer);

            Vector3 noteOffset2 = (noteData.type != ColorType.None)
                ? GetNoteOffset(
                    flipLineIndex ?? lineIndex,
                    gravityOverride ? lineLayer : startLineLayer)
                : noteOffset;

            float gravity = GetGravityBase(lineLayer, gravityOverride ? lineLayer : startLineLayer);

            result = new NoteSpawnData(
                noteOffset2,
                noteOffset2,
                noteOffset,
                gravity);

            return true;
        }

        internal bool GetSliderSpawnData(BaseSliderEditorData? sliderData, out SliderSpawnData? result)
        {
            if (sliderData == null || !(_editorDeserializedData?.Resolve(sliderData, out EditorNoodleSliderData? noodleData) ?? false) || noodleData == null)
            {
                result = null;
                return false;
            }

            bool gravityOverride = noodleData.DisableGravity;

            float offset = _movementData.noteLinesCount / 2f;
            float headLineIndex = noodleData.StartX + offset ?? sliderData.column;
            float headLineLayer = noodleData.StartY ?? sliderData.row;
            float headStartLineLayer = noodleData.InternalStartNoteLineLayer;
            float tailLineIndex = noodleData.TailStartX + offset ?? sliderData.tailColumn;
            float tailLineLayer = noodleData.TailStartY ?? sliderData.tailRow;
            float tailStartLineLayer = noodleData.InternalTailStartNoteLineLayer;

            Vector3 headOffset = GetNoteOffset(headLineIndex, gravityOverride ? headLineLayer : headStartLineLayer);
            Vector3 tailOffset = GetNoteOffset(tailLineIndex, gravityOverride ? tailLineLayer : tailStartLineLayer);
            float headGravity = GetGravityBase(headLineLayer, gravityOverride ? headLineLayer : headStartLineLayer);
            float tailGravity = GetGravityBase(tailLineLayer, gravityOverride ? tailLineLayer : tailStartLineLayer);

            result = new SliderSpawnData(
                headOffset,
                headGravity,
                tailOffset,
                tailGravity);

            return true;
        }
    }
}
