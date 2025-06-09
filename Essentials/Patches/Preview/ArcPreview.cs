using BeatmapEditor3D.Visuals;
using EditorEX.Essentials.Features.ViewMode;
using SiraUtil.Affinity;
using Zenject;

namespace EditorEX.Essentials.Patches.Preview
{
    public class ArcPreview : IAffinity
    {
        private ActiveViewMode _activeViewMode = null!;

        [Inject]
        private void Construct(ActiveViewMode activeViewMode)
        {
            _activeViewMode = activeViewMode;
        }

        [AffinityPatch(typeof(ArcView), nameof(ArcView.SetMaterialPropertyBlock))]
        [AffinityPrefix]
        private bool SetMaterialPropertyBlock()
        {
            if (_activeViewMode?.Mode == null)
                return true;
            return _activeViewMode.Mode.ShowGridAndSelection;
        }

        [AffinityPatch(typeof(ArcView), nameof(ArcView.UpdateHighlight))]
        [AffinityPrefix]
        private bool DisableSelectionViews()
        {
            if (_activeViewMode?.Mode == null)
                return true;
            return _activeViewMode.Mode.ShowGridAndSelection;
        }

        [AffinityPatch(typeof(ArcView), nameof(ArcView.UpdateState))]
        [AffinityPrefix]
        private bool DisableSelectionViews2()
        {
            if (_activeViewMode?.Mode == null)
                return true;
            return _activeViewMode.Mode.ShowGridAndSelection;
        }

        [AffinityPatch(typeof(ArcHandleView), nameof(ArcView.Init))]
        [AffinityPrefix]
        private void DisableHandles(ref bool showHandle, ref bool showMoveHandle)
        {
            if (_activeViewMode?.Mode == null)
                return;
            showHandle &= _activeViewMode.Mode.ShowGridAndSelection;
            showMoveHandle &= _activeViewMode.Mode.ShowGridAndSelection;
        }
    }
}
