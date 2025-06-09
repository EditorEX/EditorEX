using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.Visuals;
using System;
using EditorEX.Essentials.VariableMovement;
using UnityEngine;
using Zenject;
using EditorEX.Essentials.Features.ViewMode;
using SiraUtil.Logging;

namespace EditorEX.Essentials.Movement.Obstacle
{
    internal class EditorObstacleController : MonoBehaviour, IDisposable
    {
        private IObjectMovement? _obstacleMovement;
        private IObjectVisuals? _obstacleVisuals;
        private IVariableMovementDataProvider? _variableMovementDataProvider;

        private IReadonlyBeatmapState _state = null!;
        private MovementTypeProvider _movementTypeProvider = null!;
        private VisualsTypeProvider _visualsTypeProvider = null!;
        private VariableMovementTypeProvider _variableMovementTypeProvider = null!;
        private ActiveViewMode _activeViewMode = null!;
        private EditorBasicBeatmapObjectSpawnMovementData _movementData = null!;
        private SiraLog _siraLog = null!;

        private ObstacleEditorData? _data;

        [Inject]
        private void Construct(
            IReadonlyBeatmapState state,
            ActiveViewMode activeViewMode,
            MovementTypeProvider movementTypeProvider,
            VisualsTypeProvider visualsTypeProvider,
            VariableMovementTypeProvider variableMovementTypeProvider,
            EditorBasicBeatmapObjectSpawnMovementData movementData,
            SiraLog siraLog)
        {
            _state = state;
            _movementTypeProvider = movementTypeProvider;
            _visualsTypeProvider = visualsTypeProvider;
            _variableMovementTypeProvider = variableMovementTypeProvider;
            _movementData = movementData;
            _siraLog = siraLog;

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
            if (TypeProviderUtils.GetProvidedComponent(gameObject, _movementTypeProvider, _obstacleMovement, out IObjectMovement? newObstacleMovement))
            {
                if (newObstacleMovement == null)
                {
                    _siraLog.Error("EditorObstacleController: Failed to refresh, new movement provider is null!");
                }
                else
                {
                    _obstacleMovement = newObstacleMovement;
                    _obstacleMovement.Enable();   
                }
            }

            if (TypeProviderUtils.GetProvidedComponent(gameObject, _visualsTypeProvider, _obstacleVisuals, out IObjectVisuals? newObstacleVisuals))
            {
                if (newObstacleVisuals == null)
                {
                    _siraLog.Error("EditorObstacleController: Failed to refresh, new visuals provider is null!");
                }
                else
                {
                    _obstacleVisuals = newObstacleVisuals;
                    _obstacleVisuals.Enable();   
                }
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

            if (_obstacleMovement != null && _obstacleVisuals != null && _variableMovementDataProvider != null)
            {
                _obstacleMovement.Init(obstacleData, _variableMovementDataProvider, _movementData, null);
                _obstacleVisuals.Init(obstacleData);
            }

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

            if (_obstacleMovement != null && _obstacleVisuals != null)
            {
                _obstacleMovement.Setup(_data);

                _obstacleMovement.ManualUpdate();
                _obstacleVisuals.ManualUpdate();
            }
        }
    }
}
