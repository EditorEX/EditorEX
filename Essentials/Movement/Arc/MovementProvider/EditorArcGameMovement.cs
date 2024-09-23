using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BetterEditor.Essentials.Movement.Data;
using BetterEditor.Essentials.SpawnProcessing;
using BetterEditor.Heck.Deserializer;
using BetterEditor.NoodleExtensions.ObjectData;
using NoodleExtensions.Animation;
using System;
using UnityEngine;
using Zenject;

namespace BetterEditor.Essentials.Movement.Arc.MovementProvider
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

		// Movement related fields
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
		private void Construct([Inject(Id = "NoodleExtensions")] EditorDeserializedData editorDeserializedData,
			AnimationHelper animationHelper,
			IReadonlyBeatmapState state,
			ColorManager colorManager,
			IAudioTimeSource audioTimeSyncController)
		{
			_editorDeserializedData = editorDeserializedData;
			_animationHelper = animationHelper;
			_state = state;
			_colorManager = colorManager;
			_audioTimeSyncController = audioTimeSyncController;
		}

		public void Init(BaseEditorData editorData, EditorBasicBeatmapObjectSpawnMovementData movementData)
		{
			_stretchableObstacle = GetComponent<StretchableObstacle>();
			_selection = GetComponent<ObstacleViewSelection>();

			_editorBeatmapObjectSpawnMovementData = movementData;
			var obstacleData = editorData as ObstacleEditorData;
			var obstacleSpawnData = _editorBeatmapObjectSpawnMovementData.GetObstacleSpawnData(obstacleData);

			float worldRotation = 0f;

			_worldRotation = Quaternion.Euler(0f, worldRotation, 0f);
			_inverseWorldRotation = Quaternion.Euler(0f, -worldRotation, 0f);
			_obstacleDuration = obstacleData.duration;
			_height = obstacleSpawnData.obstacleHeight;
			_color = _colorManager.obstaclesColor;
			_width = (float)obstacleData.width * obstacleSpawnData.noteLinesDistance;
			Vector3 vector = new Vector3((_width - obstacleSpawnData.noteLinesDistance) * 0.5f, 0f, 0f);
			_startPos = obstacleSpawnData.moveStartPos + vector;
			_midPos = obstacleSpawnData.moveEndPos + vector;
			_endPos = obstacleSpawnData.jumpEndPos + vector;
			_move1Duration = obstacleSpawnData.moveDuration;
			_move2Duration = obstacleSpawnData.jumpDuration;
			_startTimeOffset = obstacleData.beat - _move1Duration - _move2Duration * 0.5f;
			float num = (_endPos - _midPos).magnitude / _move2Duration;
			_length = num * obstacleData.duration;
			_stretchableObstacle.SetSizeAndColor(_width * 0.98f, _height, _length, _color);
			_selection.SetObstacleData(_width, _height, _length);
			_selection.UpdateState();
			_bounds = _stretchableObstacle.bounds;
			_passedThreeQuartersOfMove2Reported = false;
			_passedAvoidedMarkReported = false;
			_passedAvoidedMarkTime = _move1Duration + _move2Duration * 0.5f + _obstacleDuration + 0.15f;
			_finishMovementTime = _move1Duration + _move2Duration + _obstacleDuration;
			transform.localPosition = obstacleSpawnData.moveStartPos;
			transform.localRotation = _worldRotation;
		}

		protected void Awake()
		{
		}

		protected void OnDestroy()
		{
		}

		public void Setup(BaseEditorData editorData)
		{
			if (!_editorDeserializedData.Resolve(editorData, out EditorNoodleObstacleData? noodleData))
			{
				return;
			}
		}

		private Vector3 GetPosForTime(float time)
		{
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


		public void ManualUpdate()
		{
			float num = this._audioTimeSyncController.songTime - _startTimeOffset;
			Vector3 posForTime = GetPosForTime(num);
			transform.localPosition = _worldRotation * posForTime;
			if (!this._passedThreeQuartersOfMove2Reported && num > this._move1Duration + this._move2Duration * 0.75f)
			{
				this._passedThreeQuartersOfMove2Reported = true;
			}
			if (!this._passedAvoidedMarkReported && num > this._passedAvoidedMarkTime)
			{
				this._passedAvoidedMarkReported = true;
			}
		}
	}
}
