using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.ViewMode;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.Obstacle
{
    internal class EditorObstacleController : MonoBehaviour, IDisposable
    {
        private IReadonlyBeatmapState _state;

        private IObjectMovement _obstacleMovement;
        private MovementTypeProvider _movementTypeProvider;
        private ActiveViewMode _activeViewMode;

        private ObstacleEditorData _data;

        private EditorBasicBeatmapObjectSpawnMovementData _movementData;

        [Inject]
        private void Construct(IReadonlyBeatmapState state, ActiveViewMode activeViewMode, MovementTypeProvider movementTypeProvider, EditorBasicBeatmapObjectSpawnMovementData movementData)
        {
            _state = state;
            _movementTypeProvider = movementTypeProvider;
            _movementData = movementData;

            _activeViewMode = activeViewMode;
            _activeViewMode.ModeChanged += RefreshObstacleMovementAndInit;
        }

        public void Dispose()
        {
            _activeViewMode.ModeChanged -= RefreshObstacleMovementAndInit;
        }

        private void RefreshObstacleMovementAndInit()
        {
            try
            {
                RefreshObstacleMovement();
                Init(_data);
            }
            catch
            {

            }
        }

        private void RefreshObstacleMovement()
        {
            if (TypeProviderUtils.GetProvidedComponent(gameObject, _movementTypeProvider, _obstacleMovement, out IObjectMovement newNoteMovement))
            {
                _obstacleMovement = newNoteMovement;
                _obstacleMovement.Enable();
            }
        }

        public void Init(ObstacleEditorData obstacleData)
        {
            if (obstacleData == null) return;
            _data = obstacleData;

            RefreshObstacleMovement();

            _obstacleMovement.Init(obstacleData, _movementData, null);

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
            if (_obstacleMovement == null)
            {
                RefreshObstacleMovementAndInit();
            }

            _obstacleMovement.Setup(_data);

            _obstacleMovement.ManualUpdate();
        }
    }
}
