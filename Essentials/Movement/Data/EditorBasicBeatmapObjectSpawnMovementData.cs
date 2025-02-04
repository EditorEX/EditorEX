using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Patches;
using EditorEX.Essentials.SpawnProcessing;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.Managers;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.Data
{
    public class EditorBasicBeatmapObjectSpawnMovementData : IEditorBeatmapObjectSpawnMovementData
    {
        public float spawnAheadTime => _spawnAheadTime;
        public float moveDuration => _moveDuration;
        public float jumpDuration => _jumpDuration;
        public float noteLinesDistance => 0.6f;
        public float verticalLayersDistance => 0.6f;
        public float jumpDistance => _jumpDistance;
        public float noteJumpMovementSpeed => _noteJumpMovementSpeed;
        public int noteLinesCount => _noteLinesCount;
        public Vector3 centerPos => _centerPos;
        public float jumpOffsetY => _jumpOffsetYProvider.jumpOffsetY;

        [Inject]
        private void Construct(
            EditorSpawnDataManager editorSpawnDataManager, 
            PlayerDataModel playerDataModel,
            PopulateBeatmap populateBeatmap)
        {
            _editorSpawnDataManager = editorSpawnDataManager;

            var difficultyBeatmap = populateBeatmap._beatmapLevelDataModel.difficultyBeatmaps[(populateBeatmap._beatmapDataModel.beatmapCharacteristic, populateBeatmap._beatmapDataModel.beatmapDifficulty)];

            var offsetProvider = new StaticJumpOffsetYProvider();
            offsetProvider.SetField("_initData", new StaticJumpOffsetYProvider.InitData(0));

            BeatmapObjectSpawnMovementData.NoteJumpValueType jumpValueType = BeatmapObjectSpawnMovementData.NoteJumpValueType.JumpDuration;
            float jumpValue = 0f;

            BeatmapObjectSpawnControllerHelpers.GetNoteJumpValues(playerDataModel.playerData.playerSpecificSettings, difficultyBeatmap.noteJumpStartBeatOffset, out jumpValueType, out jumpValue);

            Init(4, difficultyBeatmap.noteJumpMovementSpeed, populateBeatmap._beatmapLevelDataModel.beatsPerMinute, jumpValueType, jumpValue, offsetProvider, Vector3.right, Vector3.forward);
        }

        public void Init(int noteLinesCount, float startNoteJumpMovementSpeed, float startBpm, BeatmapObjectSpawnMovementData.NoteJumpValueType noteJumpValueType, float noteJumpValue, IJumpOffsetYProvider jumpOffsetYProvider, Vector3 rightVec, Vector3 forwardVec)
        {
            _noteJumpValueType = noteJumpValueType;
            _noteLinesCount = noteLinesCount;
            _noteJumpMovementSpeed = startNoteJumpMovementSpeed;
            if (noteJumpValueType != BeatmapObjectSpawnMovementData.NoteJumpValueType.BeatOffset)
            {
                if (noteJumpValueType == BeatmapObjectSpawnMovementData.NoteJumpValueType.JumpDuration)
                {
                    _jumpDuration = noteJumpValue * 2f;
                }
            }
            else
            {
                _noteJumpStartBeatOffset = noteJumpValue;
                float num = 60f / startBpm;
                float num2 = CoreMathUtils.CalculateHalfJumpDurationInBeats(_startHalfJumpDurationInBeats, _maxHalfJumpDistance, noteJumpMovementSpeed, num, _noteJumpStartBeatOffset) * 2f;
                _jumpDuration = num * num2;
            }
            _rightVec = rightVec;
            _forwardVec = forwardVec;
            _jumpOffsetYProvider = jumpOffsetYProvider;
            _moveDistance = _moveSpeed * _moveDuration;
            _jumpDistance = _noteJumpMovementSpeed * _jumpDuration;
            _moveEndPos = _centerPos + _forwardVec * (_jumpDistance * 0.5f);
            _jumpEndPos = _centerPos - _forwardVec * (_jumpDistance * 0.5f);
            _moveStartPos = _centerPos + _forwardVec * (_moveDistance + _jumpDistance * 0.5f);
            _spawnAheadTime = _moveDuration + _jumpDuration * 0.5f;
        }

        private float GetGravityBase(NoteLineLayer noteLineLayer, NoteLineLayer beforeJumpLineLayer)
        {
            return HighestJumpPosYForLineLayer(noteLineLayer) - StaticBeatmapObjectSpawnMovementData.LineYPosForLineLayer(beforeJumpLineLayer);
        }

        public ObstacleSpawnData GetObstacleSpawnData(ObstacleEditorData? obstacleData)
        {
            if (_editorSpawnDataManager.GetObstacleSpawnData(obstacleData, out var spawnData))
            {
                if (spawnData.HasValue)
                    return spawnData.Value;
            }

            Vector3 obstacleOffset = GetObstacleOffset(obstacleData.column, (NoteLineLayer)obstacleData.row);
            obstacleOffset.y += _jumpOffsetYProvider.jumpOffsetY;
            obstacleOffset.y = Mathf.Max(obstacleOffset.y, _verticalObstaclePosY);
            float height = Mathf.Min((float)obstacleData.height * StaticBeatmapObjectSpawnMovementData.layerHeight, _obstacleTopPosY - obstacleOffset.y);
            float width = (float)obstacleData.width * 0.6f;
            obstacleOffset.x += (width - 0.6f) * 0.5f;
            return new ObstacleSpawnData(obstacleOffset, width, height);
        }

        public NoteSpawnData GetJumpingNoteSpawnData(NoteEditorData? noteData)
        {
            if (_editorSpawnDataManager.GetJumpingNoteSpawnData(noteData, out var resultSpawnData))
            {
                if (resultSpawnData.HasValue)
                    return resultSpawnData.Value;
            }

            var spawnData = EditorSpawnDataRepository.GetSpawnData(noteData);
            Vector3 noteOffset = GetNoteOffset(noteData.column, spawnData.beforeJumpNoteLineLayer);
            Vector3 flipOffset = ((noteData.type != ColorType.None) ? GetNoteOffset(spawnData.flipLineIndex, spawnData.beforeJumpNoteLineLayer) : noteOffset);
            return new NoteSpawnData(flipOffset, flipOffset, noteOffset, GetGravityBase((NoteLineLayer)noteData.row, spawnData.beforeJumpNoteLineLayer));
        }

        public SliderSpawnData GetSliderSpawnData(BaseSliderEditorData? sliderData)
        {
            if (_editorSpawnDataManager.GetSliderSpawnData(sliderData, out var resultSpawnData))
            {
                if (resultSpawnData.HasValue)
                    return resultSpawnData.Value;
            }

            var spawnData = EditorSpawnDataRepository.GetSpawnData(sliderData);
            Vector3 noteOffset = GetNoteOffset(sliderData.column, spawnData.headBeforeJumpLineLayer);
            float num = NoteJumpGravityForLineLayer((NoteLineLayer)sliderData.row, spawnData.headBeforeJumpLineLayer);
            Vector3 noteOffset2 = GetNoteOffset(sliderData.tailColumn, spawnData.tailBeforeJumpLineLayer);
            return new SliderSpawnData(noteOffset, GetGravityBase((NoteLineLayer)sliderData.row, spawnData.headBeforeJumpLineLayer), noteOffset2, GetGravityBase((NoteLineLayer)sliderData.tailRow, spawnData.tailBeforeJumpLineLayer));
        }

        public Vector3 GetNoteOffset(int noteLineIndex, NoteLineLayer noteLineLayer)
        {
            float num = (float)(-(float)(_noteLinesCount - 1)) * 0.5f;
            num = (num + (float)noteLineIndex) * 0.8f;
            return _rightVec * num + new Vector3(0f, StaticBeatmapObjectSpawnMovementData.LineYPosForLineLayer(noteLineLayer), 0f);
        }

        public Vector3 GetObstacleOffset(int noteLineIndex, NoteLineLayer noteLineLayer)
        {
            float num = (float)(-(float)(_noteLinesCount - 1)) * 0.5f;
            num = (num + (float)noteLineIndex) * 0.6f;
            return _rightVec * num + new Vector3(0f, StaticBeatmapObjectSpawnMovementData.LineYPosForLineLayer(noteLineLayer) + -0.15f, 0f);
        }

        public float JumpPosYForLineLayerAtDistanceFromPlayerWithoutJumpOffset(NoteLineLayer lineLayer, float distanceFromPlayer)
        {
            float num = (_jumpDistance * 0.5f - distanceFromPlayer) / _noteJumpMovementSpeed;
            float num2 = NoteJumpGravityForLineLayerWithoutJumpOffset(lineLayer, NoteLineLayer.Base);
            float num3 = num2 * _jumpDuration * 0.5f;
            return StaticBeatmapObjectSpawnMovementData.LineYPosForLineLayer(NoteLineLayer.Base) + num3 * num - num2 * num * num * 0.5f;
        }

        public float HighestJumpPosYForLineLayer(NoteLineLayer lineLayer)
        {
            if (lineLayer == NoteLineLayer.Base)
            {
                return _baseLinesHighestJumpPosY + _jumpOffsetYProvider.jumpOffsetY;
            }
            if (lineLayer == NoteLineLayer.Upper)
            {
                return _upperLinesHighestJumpPosY + _jumpOffsetYProvider.jumpOffsetY;
            }
            return _topLinesHighestJumpPosY + _jumpOffsetYProvider.jumpOffsetY;
        }

        public float HighestJumpPosYForLineLayerWithoutJumpOffset(NoteLineLayer lineLayer)
        {
            if (lineLayer == NoteLineLayer.Base)
            {
                return _baseLinesHighestJumpPosY;
            }
            if (lineLayer == NoteLineLayer.Upper)
            {
                return _upperLinesHighestJumpPosY;
            }
            return _topLinesHighestJumpPosY;
        }

        public float NoteJumpGravityForLineLayer(NoteLineLayer lineLayer, NoteLineLayer beforeJumpLineLayer)
        {
            float num = _jumpDistance / _noteJumpMovementSpeed * 0.5f;
            return 2f * (HighestJumpPosYForLineLayer(lineLayer) - StaticBeatmapObjectSpawnMovementData.LineYPosForLineLayer(beforeJumpLineLayer)) / (num * num);
        }

        public float NoteJumpGravityForLineLayerWithoutJumpOffset(NoteLineLayer lineLayer, NoteLineLayer beforeJumpLineLayer)
        {
            float num = _jumpDistance / _noteJumpMovementSpeed * 0.5f;
            return 2f * (HighestJumpPosYForLineLayerWithoutJumpOffset(lineLayer) - StaticBeatmapObjectSpawnMovementData.LineYPosForLineLayer(beforeJumpLineLayer)) / (num * num);
        }

        private EditorSpawnDataManager _editorSpawnDataManager;

        private Vector3 _centerPos = new Vector3(0f, 0f, 0.65f);
        internal float _maxHalfJumpDistance = kDefaultMaxHalfJumpDistance;
        internal float _startHalfJumpDurationInBeats = kDefaultStartHalfJumpDurationInBeats;
        private float _baseLinesHighestJumpPosY = 0.85f;
        private float _upperLinesHighestJumpPosY = 1.4f;
        private float _topLinesHighestJumpPosY = 1.9f;
        private float _moveSpeed = 200f;
        private float _moveDuration = 0.5f;
        private float _verticalObstaclePosY = 0.1f;
        internal float _obstacleTopPosY = 3.1f;
        private float _spawnAheadTime;
        internal float _jumpDuration;
        internal float _noteJumpStartBeatOffset;
        private float _noteJumpMovementSpeed;
        private float _jumpDistance;
        internal float _moveDistance;
        private Vector3 _moveStartPos;
        private Vector3 _moveEndPos;
        private Vector3 _jumpEndPos;
        private int _noteLinesCount = 4;
        internal IJumpOffsetYProvider _jumpOffsetYProvider;
        internal Vector3 _rightVec;
        internal Vector3 _forwardVec;
        internal BeatmapObjectSpawnMovementData.NoteJumpValueType _noteJumpValueType;

        public const float kDefaultMaxHalfJumpDistance = 18f;
        public const float kDefaultStartHalfJumpDurationInBeats = 4f;
    }
}
