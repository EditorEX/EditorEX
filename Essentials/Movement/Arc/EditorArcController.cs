using BeatmapEditor3D.DataModels;
using BetterEditor.Essentials.Movement.Data;
using BetterEditor.Essentials.ViewMode;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BetterEditor.Essentials.Movement.Arc
{
	internal class EditorArcController : MonoBehaviour, IDisposable
	{
		private IReadonlyBeatmapState _state;

		private IObjectMovement _arcMovement;
		private MovementTypeProvider _movementTypeProvider;
		private ActiveViewMode _activeViewMode;

		private ArcEditorData _data;

		private EditorBasicBeatmapObjectSpawnMovementData _movementData;

		[Inject]
		private void Construct(IReadonlyBeatmapState state, ActiveViewMode activeViewMode, MovementTypeProvider movementTypeProvider, EditorBasicBeatmapObjectSpawnMovementData movementData)
		{
			_state = state;
			_movementTypeProvider = movementTypeProvider;
			_movementData = movementData;

			_activeViewMode = activeViewMode;
			_activeViewMode.ModeChanged += RefreshArcMovementAndInit;
		}

		public void Dispose()
		{
			_activeViewMode.ModeChanged -= RefreshArcMovementAndInit;
		}

		private void RefreshArcMovementAndInit()
		{
			try
			{
				RefreshArcMovement();
				Init(_data);
			}
			catch
			{

			}
		}

		private void RefreshArcMovement()
		{
			var components = gameObject?.GetComponents<IObjectMovement>();
			if (components == null) return;
			var types = components?.Select(x => x.GetType())?.ToArray();
			var type = _movementTypeProvider.GetNoteMovement(types);

			if (_arcMovement != null && type == _arcMovement.GetType())
			{
				return; // No need to refresh if the type is the same
			}

			_arcMovement = GetComponent(type) as IObjectMovement;
		}

		public void Init(ArcEditorData editorData)
		{
			if (editorData == null) return;
			_data = editorData;

			RefreshArcMovement();

			_arcMovement.Init(editorData, _movementData);

			ManualUpdate();
		}

		float _prevBeat = 9999f;

		public void Update()
		{
			if (!_state.isPlaying && _prevBeat == _state.beat) return;
			if (_prevBeat > _state.beat)
			{
				Init(_data);
			}
			_prevBeat = _state.beat;

			ManualUpdate();
		}

		public void ManualUpdate()
		{
			if (_arcMovement == null)
			{
				RefreshArcMovementAndInit();
			}

			_arcMovement.Setup(_data);

			_arcMovement.ManualUpdate();
		}
	}
}
