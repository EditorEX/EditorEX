using System.Linq;
using BeatmapEditor3D;
using Zenject;

namespace EditorEX.Essentials.Features.ViewMode
{
    internal class ViewModeImplementation
    {
        private ActiveViewMode _activeViewMode;
        private BeatmapObjectsView _beatmapObjectsView;

        private ViewModeImplementation(
            SignalBus signalBus,
            ActiveViewMode activeViewMode,
            BeatmapObjectsView beatmapObjectsView)
        {
            _activeViewMode = activeViewMode;
            _activeViewMode.Mode = ViewModeRepository.GetViewModes().FirstOrDefault(x => x.ID == "normal");
            _activeViewMode.LastMode = _activeViewMode.Mode;
            _beatmapObjectsView = beatmapObjectsView;
            signalBus.Subscribe<ViewModeSwitchedSignal>(x => SetMode(x.ViewMode));
        }

        private void SetMode(ViewMode mode)
        {
            if (_activeViewMode.Mode == mode) return;
            _activeViewMode.LastMode = _activeViewMode.Mode;
            _activeViewMode.Mode = mode;
            _activeViewMode?.ModeChanged();
            _beatmapObjectsView._notesBeatmapObjectsView.ClearObjects();
            _beatmapObjectsView._notesBeatmapObjectsView.ClearPool();
            _beatmapObjectsView.gameObject.SetActive(false);
            _beatmapObjectsView.gameObject.SetActive(true);
        }
    }
}
