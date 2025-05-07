using System;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Features.ViewMode;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.VariableMovement;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.ChainHead
{
    public class EditorChainHeadController : MonoBehaviour, IDisposable
    {
        private IReadonlyBeatmapState _state;

        private IObjectMovement? _arcMovement;
        private IVariableMovementDataProvider _variableMovementDataProvider;
        private MovementTypeProvider _movementTypeProvider;
        private VariableMovementTypeProvider _variableMovementTypeProvider;
        private ActiveViewMode _activeViewMode;

        private ArcEditorData? _data;

        private EditorBasicBeatmapObjectSpawnMovementData _movementData;

        [Inject]
        private void Construct(
            IReadonlyBeatmapState state,
            ActiveViewMode activeViewMode,
            MovementTypeProvider movementTypeProvider,
            VariableMovementTypeProvider variableMovementTypeProvider,
            EditorBasicBeatmapObjectSpawnMovementData movementData)
        {
            _state = state;
            _movementTypeProvider = movementTypeProvider;
            _variableMovementTypeProvider = variableMovementTypeProvider;
            _movementData = movementData;

            _activeViewMode = activeViewMode;
            _activeViewMode.ModeChanged += RefreshHeadMovementAndInit;
        }

        public void Dispose()
        {
            _activeViewMode.ModeChanged -= RefreshHeadMovementAndInit;
        }

        private void RefreshHeadMovementAndInit()
        {
            try
            {
                RefreshHeadMovement();
                Init(_data);
            }
            catch
            {

            }
        }

        private void RefreshHeadMovement()
        {
            if (TypeProviderUtils.GetProvidedComponent(gameObject, _movementTypeProvider, _arcMovement, out var newNoteMovement))
            {
                _arcMovement = newNoteMovement;
                _arcMovement?.Enable();
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

        public void Init(ArcEditorData? editorData)
        {
            if (editorData == null) return;
            _data = editorData;

            RefreshHeadMovement();

            _arcMovement.Init(editorData, _variableMovementDataProvider, _movementData, null);

            ManualUpdate();
        }

        float _prevBeat = 9999f;

        public void Update()
        {
            if (!_state.isPlaying && Mathf.Approximately(_prevBeat, _state.beat)) return;
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
                RefreshHeadMovementAndInit();
            }

            _arcMovement?.Setup(_data);

            _arcMovement?.ManualUpdate();
        }
    }
}