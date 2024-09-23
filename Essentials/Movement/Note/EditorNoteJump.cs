using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BetterEditor.Heck.Deserializer;
using BetterEditor.NoodleExtensions.ObjectData;
using Heck;
using NoodleExtensions;
using NoodleExtensions.Animation;
using NoodleExtensions.HarmonyPatches.Objects;
using System;
using UnityEngine;
using Zenject;

namespace BetterEditor.Essentials.Movement.Note
{
	public class EditorNoteJump : MonoBehaviour
	{
		public event Action noteJumpDidFinishEvent;

		public event Action noteJumpDidPassMissedMarkerEvent;

		public event Action<EditorNoteJump> noteJumpDidPassThreeQuartersEvent;

		public event Action noteJumpDidPassHalfEvent;

		public event Action<float> noteJumpDidUpdateProgressEvent;

		public float distanceToPlayer => Mathf.Abs(_localPosition.z - (_inverseWorldRotation * _playerTransforms.headPseudoLocalPos).z);

		public Vector3 beatPos => (_endPos + _startPos) * 0.5f;

		public float jumpDuration => _jumpDuration;

		public Vector3 moveVec => _moveVec;

		public Vector3 localPosition => _localPosition;

		[Inject(Id = "NoodleExtensions")] private EditorDeserializedData _editorDeserializedData;

		public void Init(NoteEditorData editorData, float beatTime, float worldRotation, Vector3 startPos, Vector3 endPos, float jumpDuration, float gravity, float flipYSide, float endRotation)
		{
			_editorData = editorData;
			if (!_editorDeserializedData.Resolve(_editorData, out NoodleData))
			{
				NoodleData = null;
				return;
			}

			_rotatedObject = transform.Find("NoteCube") ?? transform;
			_worldRotation = Quaternion.Euler(0f, worldRotation, 0f);
			_inverseWorldRotation = Quaternion.Euler(0f, -worldRotation, 0f);
			_startPos = startPos;
			_endPos = endPos;
			_jumpDuration = jumpDuration;
			_moveVec = (_endPos - _startPos) / _jumpDuration;
			_beatTime = beatTime;
			_gravity = gravity;
			if (flipYSide > 0f)
			{
				_yAvoidance = flipYSide * _yAvoidanceUp;
			}
			else
			{
				_yAvoidance = flipYSide * _yAvoidanceDown;
			}
			_missedMarkReported = false;
			_threeQuartersMarkReported = false;
			_startVerticalVelocity = _gravity * _jumpDuration * 0.5f;
			_endRotation = Quaternion.Euler(0f, 0f, endRotation);
			_missedTime = beatTime + kMissedTimeOffset;
			Vector3 vector = _endRotation.eulerAngles;
			_middleRotation = default;
			_middleRotation.eulerAngles = vector;
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
				_animationHelper.GetDefinitePositionOffset(noodleData.AnimationObject, noodleData.Track, time, out Vector3? position);
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

			float songTime = _audioTimeSyncController.songTime;
			float num = hasNoodle ? 
							NoteJumpTimeAdjust(songTime - (_beatTime - _jumpDuration * 0.5f), _jumpDuration) : 
							songTime - (_beatTime - _jumpDuration * 0.5f);
			float num2 = num / _jumpDuration;
			if (_startPos.x == _endPos.x)
			{
				_localPosition.x = _startPos.x;
			}
			else if (num2 < 0.25f)
			{
				_localPosition.x = _startPos.x + (_endPos.x - _startPos.x) * Easing.InOutQuad(num2 * 4f);
			}
			else
			{
				_localPosition.x = _endPos.x;
			}
			_localPosition.z = _playerTransforms.MoveTowardsHead(_startPos.z, _endPos.z, _inverseWorldRotation, num2);
			_localPosition.y = _startPos.y + _startVerticalVelocity * num - _gravity * num * num * 0.5f;
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
				quaternion = Quaternion.Slerp(_startRotation, _middleRotation, Mathf.Sin(num2Clamped * 3.1415927f * 4f));
			}
			else
			{
				quaternion = Quaternion.Slerp(_middleRotation, _endRotation, Mathf.Sin((num2Clamped - 0.125f) * 3.1415927f * 2f));
			}

			_rotatedObject.localRotation = quaternion;

			if (num2 >= 0.5f && !_halfJumpMarkReported)
			{
				_halfJumpMarkReported = true;
				noteJumpDidPassHalfEvent?.Invoke();
			}
			if (num2 >= 0.75f && !_threeQuartersMarkReported)
			{
				_threeQuartersMarkReported = true;
				noteJumpDidPassThreeQuartersEvent?.Invoke(this);
			}
			if (songTime >= _missedTime && !_missedMarkReported)
			{
				_missedMarkReported = true;
				noteJumpDidPassMissedMarkerEvent?.Invoke();
			}
			if (_threeQuartersMarkReported && (!_definitePosition || !hasNoodle))
			{
				float num4 = (num2 - 0.75f) / 0.25f;
				num4 = num4 * num4 * num4;
				_localPosition.z -= Mathf.LerpUnclamped(0f, _endDistanceOffset, num4);
			}
			if (num2 >= 1f)
			{
				if (!_missedMarkReported)
				{
					_missedMarkReported = true;
					noteJumpDidPassMissedMarkerEvent?.Invoke();
				}
				noteJumpDidFinishEvent?.Invoke();
			}
			Vector3 vector3 = _worldRotation * _localPosition;
			transform.localPosition = vector3;
			noteJumpDidUpdateProgressEvent?.Invoke(num);
			return vector3;
		}

		private bool _definitePosition;

		private EditorNoodleBaseNoteData NoodleData;

		private NoteEditorData _editorData;

		private Transform _rotatedObject;

		private float _yAvoidanceUp = 0.45f;

		private float _yAvoidanceDown = 0.15f;

		private float _endDistanceOffset = 500f;

		[Inject]
		private readonly PlayerTransforms _playerTransforms;

		[Inject]
		private readonly PlayerSpaceConvertor _playerSpaceConvertor;

		[Inject]
		private readonly IAudioTimeSource _audioTimeSyncController;

		[Inject]
		private readonly AnimationHelper _animationHelper;

		internal Vector3 _startPos;

		internal Vector3 _endPos;

		private float _jumpDuration;

		private Vector3 _moveVec;

		private float _beatTime;

		private float _startVerticalVelocity;

		private Quaternion _startRotation;

		private Quaternion _middleRotation;

		private Quaternion _endRotation;

		private float _gravity;

		private float _yAvoidance;

		private float _missedTime;

		private bool _missedMarkReported;

		private bool _threeQuartersMarkReported;

		private bool _halfJumpMarkReported;

		private Vector3 _localPosition;

		private readonly Vector3[] _randomRotations = new Vector3[]
		{
			new Vector3(-0.9543871f, -0.1183784f, 0.2741019f),
			new Vector3(0.7680854f, -0.08805521f, 0.6342642f),
			new Vector3(-0.6780157f, 0.306681f, -0.6680131f),
			new Vector3(0.1255014f, 0.9398643f, 0.3176546f),
			new Vector3(0.365105f, -0.3664974f, -0.8557909f),
			new Vector3(-0.8790653f, -0.06244748f, -0.4725934f),
			new Vector3(0.01886305f, -0.8065798f, 0.5908241f),
			new Vector3(-0.1455435f, 0.8901445f, 0.4318099f),
			new Vector3(0.07651193f, 0.9474725f, -0.3105508f),
			new Vector3(0.1306983f, -0.2508438f, -0.9591639f)
		};

		public const float kMissedTimeOffset = 0.15f;

		internal Quaternion _worldRotation;

		internal Quaternion _inverseWorldRotation;

		private bool _rotateTowardsPlayer;
	}
}
