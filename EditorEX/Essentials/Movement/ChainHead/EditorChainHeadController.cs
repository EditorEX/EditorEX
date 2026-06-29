using System;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.Features.ViewMode;
using EditorEX.Essentials.Movement.Data;
using EditorEX.Essentials.VariableMovement;
using EditorEX.Essentials.Visuals;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Movement.ChainHead
{
    public class EditorChainHeadController : MonoBehaviour, IDisposable
    {
        private IReadonlyBeatmapState _state = null!;
        private MovementTypeProvider _movementTypeProvider = null!;
        private VisualsTypeProvider _visualsTypeProvider = null!;
        private VariableMovementTypeProvider _variableMovementTypeProvider = null!;
        private ActiveViewMode _activeViewMode = null!;
        private EditorBasicBeatmapObjectSpawnMovementData _movementData = null!;

        private ChainEditorData? _data;
        private IObjectMovement? _chainMovement;
        private IObjectVisuals? _chainVisuals;
        private IVariableMovementDataProvider? _variableMovementDataProvider;

        [Inject]
        private void Construct(
            IReadonlyBeatmapState state,
            ActiveViewMode activeViewMode,
            MovementTypeProvider movementTypeProvider,
            VisualsTypeProvider visualsTypeProvider,
            VariableMovementTypeProvider variableMovementTypeProvider,
            EditorBasicBeatmapObjectSpawnMovementData movementData
        )
        {
            _state = state;
            _movementTypeProvider = movementTypeProvider;
            _visualsTypeProvider = visualsTypeProvider;
            _variableMovementTypeProvider = variableMovementTypeProvider;
            _movementData = movementData;

            _activeViewMode = activeViewMode;
            _activeViewMode.ModeChanged += RefreshHeadMovementVisualsAndInit;
        }

        public void Dispose()
        {
            _activeViewMode.ModeChanged -= RefreshHeadMovementVisualsAndInit;
        }

        private void RefreshHeadMovementVisualsAndInit()
        {
            try
            {
                RefreshHeadMovementVisuals();
                Init(_data);
            }
            catch { }
        }

        private void RefreshHeadMovementVisuals()
        {
            if (
                TypeProviderUtils.GetProvidedComponent(
                    gameObject,
                    _movementTypeProvider,
                    _chainMovement,
                    out var newChainMovement
                )
            )
            {
                _chainMovement = newChainMovement;
                _chainMovement?.Enable();
            }

            if (
                TypeProviderUtils.GetProvidedComponent(
                    gameObject,
                    _visualsTypeProvider,
                    _chainVisuals,
                    out var newChainVisuals
                )
            )
            {
                _chainVisuals = newChainVisuals;
                _chainVisuals?.Enable();
            }

            if (
                TypeProviderUtils.GetProvidedVariableMovementDataProvider(
                    gameObject,
                    _variableMovementTypeProvider,
                    _data,
                    _variableMovementDataProvider,
                    out var variableMovementDataProvider
                )
            )
            {
                _variableMovementDataProvider = variableMovementDataProvider;
                if (
                    _variableMovementDataProvider
                    is EditorNoodleMovementDataProvider noodleMovementDataProvider
                )
                {
                    noodleMovementDataProvider.InitObject(_data);
                }
            }
        }

        public void Init(ChainEditorData? editorData)
        {
            if (editorData == null)
                return;
            _data = editorData;

            RefreshHeadMovementVisuals();

            _chainMovement?.Init(editorData, _variableMovementDataProvider, _movementData, null);
            _chainVisuals?.Init(editorData);

            ManualUpdate();
        }

        float _prevBeat = 9999f;

        public void Update()
        {
            if (!_state.isPlaying && Mathf.Approximately(_prevBeat, _state.beat))
                return;
            if (_prevBeat > _state.beat)
            {
                Init(_data);
            }
            _prevBeat = _state.beat;

            ManualUpdate();
        }

        public void ManualUpdate()
        {
            if (_chainMovement == null || _chainVisuals == null)
            {
                RefreshHeadMovementVisualsAndInit();
            }

            _chainMovement?.Setup(_data);

            _chainMovement?.ManualUpdate();
            _chainVisuals?.ManualUpdate();
        }
    }
}
