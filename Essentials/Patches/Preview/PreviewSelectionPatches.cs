using BeatmapEditor3D;
using BeatmapEditor3D.Views;
using BeatmapEditor3D.Visuals;
using EditorEX.Essentials.ViewMode;
using HarmonyLib;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using Zenject;

namespace EditorEX.Essentials.Patches.Preview
{
    public class PreviewSelectionPatches : IAffinity
    {
        private ActiveViewMode _activeViewMode;

        [Inject]
        private void Construct(ActiveViewMode activeViewMode)
        {
            _activeViewMode = activeViewMode;
        }

        [AffinityPatch(typeof(BeatmapObjectViewSelection), nameof(BeatmapObjectViewSelection.UpdateHighlight))]
        [AffinityPrefix]
        private bool UpdateHighlight()
        {
            return _activeViewMode.Mode.ShowGridAndSelection;
        }

        [AffinityPatch(typeof(BeatmapObjectViewSelection), nameof(BeatmapObjectViewSelection.SetInitialState))]
        [AffinityPrefix]
        private void SetInitialState(ref bool onBeat, ref bool pastBeat, ref bool selected, ref bool highlighted, ref bool isDeleting)
        {
            onBeat &= _activeViewMode.Mode.ShowGridAndSelection;
            pastBeat &= _activeViewMode.Mode.ShowGridAndSelection;
            selected &= _activeViewMode.Mode.ShowGridAndSelection;
            highlighted &= _activeViewMode.Mode.ShowGridAndSelection;
            isDeleting &= _activeViewMode.Mode.ShowGridAndSelection;
        }

        [AffinityPatch(typeof(BeatmapObjectViewSelection), nameof(BeatmapObjectViewSelection.UpdateState), AffinityMethodType.Normal, null, new[] { typeof(bool), typeof(bool), typeof(bool) })]
        [AffinityPrefix]
        private bool UpdateState()
        {
            return _activeViewMode.Mode.ShowGridAndSelection;
        }

        [AffinityPatch(typeof(ObstacleViewSelection), nameof(ObstacleViewSelection.SetObstacleData))]
        [AffinityPrefix]
        private bool SetObstacleData(ObstacleViewSelection __instance)
        {
            if (!_activeViewMode.Mode.ShowGridAndSelection)
            {
                __instance._selection.SetActive(false);
                if (_activeViewMode.Mode.PreviewObjects)
                {
                    __instance._nonSelectedGameObject.SetActive(false);
                }
            }
            return _activeViewMode.Mode.ShowGridAndSelection;
        }

    }
}
