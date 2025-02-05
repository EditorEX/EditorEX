using BeatmapEditor3D.DataModels;
using BeatmapSaveDataVersion2_6_0AndEarlier;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.ViewMode;
using EditorEX.Essentials.Visuals;
using System;
using System.Linq;
using EditorEX.Essentials.VariableMovement;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.Obstacle
{
    internal class EditorObstacleController : MonoBehaviour, IDisposable
    {
        private IObjectMovement _obstacleMovement;
        private IObjectVisuals _obstacleVisuals;
        private IVariableMovementDataProvider _variableMovementDataProvider;

        private IReadonlyBeatmapState _state;
        private MovementTypeProvider _movementTypeProvider;
        private VisualsTypeProvider _visualsTypeProvider;
        private VariableMovementTypeProvider _variableMovementTypeProvider;
        private ActiveViewMode _activeViewMode;
        private EditorBasicBeatmapObjectSpawnMovementData _movementData;

        private ObstacleEditorData? _data;

        [Inject]
        private void Construct(
            IReadonlyBeatmapState state, 
            ActiveViewMode activeViewMode, 
            MovementTypeProvider movementTypeProvider,
            VisualsTypeProvider visualsTypeProvider,
            VariableMovementTypeProvider variableMovementTypeProvider,
            EditorBasicBeatmapObjectSpawnMovementData movementData)
        {
            _state = state;
            _movementTypeProvider = movementTypeProvider;
            _visualsTypeProvider = visualsTypeProvider;
            _variableMovementTypeProvider = variableMovementTypeProvider;
            _movementData = movementData;

            _activeViewMode = activeViewMode;
            _activeViewMode.ModeChanged += RefreshObstacleMovementVisualsAndInit;
        }

        public void Dispose()
        {
            _activeViewMode.ModeChanged -= RefreshObstacleMovementVisualsAndInit;
        }

        private void RefreshObstacleMovementVisualsAndInit()
        {
            try
            {
                RefreshObstacleMovementVisuals();
                Init(_data);
            }
            catch
            {

            }
        }

        private void RefreshObstacleMovementVisuals()
        {
            if (TypeProviderUtils.GetProvidedComponent(gameObject, _movementTypeProvider, _obstacleMovement, out IObjectMovement newObstacleMovement))
            {
                _obstacleMovement = newObstacleMovement;
                _obstacleMovement.Enable();
            }

            if (TypeProviderUtils.GetProvidedComponent(gameObject, _visualsTypeProvider, _obstacleVisuals, out IObjectVisuals newObstacleVisuals))
            {
                _obstacleVisuals = newObstacleVisuals;
                _obstacleVisuals.Enable();
            }

            if (TypeProviderUtils.GetProvidedVariableMovementDataProvider(gameObject, _variableMovementTypeProvider, _data, _variableMovementDataProvider, out var variableMovementDataProvider))
            {
                _variableMovementDataProvider = variableMovementDataProvider;
                if (_variableMovementDataProvider is EditorNoodleMovementDataProvider noodleMovementDataProvider)
                {
                    noodleMovementDataProvider.InitObject(_data);
                }
            }
        }

        public void Init(ObstacleEditorData? obstacleData)
        {
            if (obstacleData == null) return;
            _data = obstacleData;

            RefreshObstacleMovementVisuals();

            _obstacleMovement.Init(obstacleData, _variableMovementDataProvider, _movementData, null);
            _obstacleVisuals.Init(obstacleData);

            ManualUpdate();
        }

        // Use our own prevBeat field as _state.prevBeat only updates when playing or scrubbing which will cause constant updates after scrubbing while paused.
        float _prevBeat = 9999f;

        public void Update()
        {
            if (!_state.isPlaying && _prevBeat == _state.beat) return; //Don't update if not playing for performance, but force an update if scrubbing manually.

            // If we rewind we should reinit the note to stop issues
            if (_prevBeat > _state.beat)
            {
                Init(_data);
            }
            _prevBeat = _state.beat;

            ManualUpdate();
        }

        public void ManualUpdate()
        {
            if (_obstacleMovement == null || _obstacleVisuals == null)
            {
                RefreshObstacleMovementVisualsAndInit();
            }

            _obstacleMovement.Setup(_data);

            _obstacleMovement.ManualUpdate();
            _obstacleVisuals.ManualUpdate();
        }
    }
}
