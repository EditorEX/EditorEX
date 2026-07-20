using System;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using Zenject;

namespace EditorEX.Essentials.Features.ViewMode
{
    internal class ViewModeImplementation
    {
        private ActiveViewMode _activeViewMode;
        private BeatmapObjectsView _beatmapObjectsView;
        private IReadonlyBeatmapState _beatmapState;

        public ViewModeImplementation(
            SignalBus signalBus,
            ActiveViewMode activeViewMode,
            BeatmapObjectsView beatmapObjectsView,
            IReadonlyBeatmapState beatmapState
        )
        {
            _activeViewMode = activeViewMode;
            _activeViewMode.Mode =
                ViewModeRepository.ViewMode("normal")
                ?? throw new Exception("Normal view mode not found");
            _activeViewMode.LastMode = _activeViewMode.Mode;
            _beatmapObjectsView = beatmapObjectsView;
            _beatmapState = beatmapState;
            signalBus.Subscribe<ShiftNextViewingModeSignal>(_ =>
                SetMode(ViewModeRepository.GetNextViewMode(_activeViewMode.Mode))
            );
            signalBus.Subscribe<ShiftPreviousViewingModeSignal>(_ =>
                SetMode(ViewModeRepository.GetPreviousViewMode(_activeViewMode.Mode))
            );
        }

        private void SetMode(ViewMode mode)
        {
            if (
                _activeViewMode.Mode == mode
                || _beatmapState.editingMode != BeatmapEditingMode.Objects
            )
                return;
            _activeViewMode.LastMode = _activeViewMode.Mode;
            _activeViewMode.Mode = mode;
            _activeViewMode.ModeChanged?.Invoke();
            _beatmapObjectsView._notesBeatmapObjectsView.ClearObjects();
            _beatmapObjectsView._notesBeatmapObjectsView.ClearPool();
            _beatmapObjectsView.gameObject.SetActive(false);
            _beatmapObjectsView.gameObject.SetActive(true);
        }
    }
}
