using BeatmapEditor3D.Visuals;
using EditorEX.Essentials.Features.ViewMode;
using SiraUtil.Affinity;
using Zenject;

namespace EditorEX.Essentials.Patches.Preview
{
    public class ArcPreview : IAffinity
    {
        private ActiveViewMode _activeViewMode;

        [Inject]
        private void Construct(ActiveViewMode activeViewMode)
        {
            _activeViewMode = activeViewMode;
        }

        [AffinityPatch(typeof(ArcView), nameof(ArcView.SetMaterialPropertyBlock))]
        [AffinityPrefix]
        private bool SetMaterialPropertyBlock()
        {
            return _activeViewMode.Mode.ShowGridAndSelection;
        }

        [AffinityPatch(typeof(ArcView), nameof(ArcView.UpdateHighlight))]
        [AffinityPrefix]
        private bool DisableSelectionViews()
        {
            return _activeViewMode.Mode.ShowGridAndSelection;
        }

        [AffinityPatch(typeof(ArcView), nameof(ArcView.UpdateState))]
        [AffinityPrefix]
        private bool DisableSelectionViews2()
        {
            return _activeViewMode.Mode.ShowGridAndSelection;
        }

        [AffinityPatch(typeof(ArcHandleView), nameof(ArcView.Init))]
        [AffinityPrefix]
        private void DisableHandles(ref bool showHandle, ref bool showMoveHandle)
        {
            showHandle &= _activeViewMode.Mode.ShowGridAndSelection;
            showMoveHandle &= _activeViewMode.Mode.ShowGridAndSelection;
        }
    }
}
