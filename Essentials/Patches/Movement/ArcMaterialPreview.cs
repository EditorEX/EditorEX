using BeatmapEditor3D.Views;
using BeatmapEditor3D.Visuals;
using BetterEditor.Essentials.ViewMode;
using HarmonyLib;
using SiraUtil.Affinity;
using System.Collections.Generic;
using Zenject;

namespace BetterEditor.Essentials.Patches.Movement
{
    public class ArcMaterialPreview : IAffinity
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
            return _activeViewMode.Mode != "Preview";
        }
    }
}
