using BeatmapEditor3D;
using BeatmapEditor3D.Views;
using BeatmapEditor3D.Visuals;
using EditorEX.Essentials.Features.ViewMode;
using SiraUtil.Affinity;
using System;
using UnityEngine;

namespace EditorEX.Essentials.Patches.Preview
{
    internal class PreviewToggler : IAffinity, IDisposable
    {
        private ActiveViewMode _activeViewMode;

        private GameObject grid;
        private BeatmapObjectGridHoverView hover;
        private GameObject selection;
        private Transform currentLine;
        private BeatGridContainer container;
        private GameObject lanes;

        public PreviewToggler(ActiveViewMode activeViewMode)
        {
            _activeViewMode = activeViewMode;
            _activeViewMode.ModeChanged += TogglePreview;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectsContainer), nameof(BeatmapObjectsContainer.OnEnable))]
        private void Patch(BeatmapObjectsContainer __instance)
        {
            grid = __instance.transform.Find("BeatmapObjectSelectionGridView").gameObject;
            hover = __instance.transform.Find("BeatmapObjectGridHoverView").GetComponent<BeatmapObjectGridHoverView>();
            selection = __instance.transform.Find("BeatmapObjectSelectionView").gameObject;
            container = __instance.GetComponentInChildren<BeatGridContainer>();
            currentLine = container._currentBeatLineTransform;
            lanes = __instance.transform.Find("BeatGridContainer").Find("GridLanes").gameObject;
        }

        public void Dispose()
        {
            _activeViewMode.ModeChanged -= TogglePreview;
        }

        private void TogglePreview()
        {
            bool showGrid = _activeViewMode.Mode.ShowGridAndSelection;
            grid.SetActive(showGrid);
            hover.gameObject.SetActive(showGrid);
            selection.gameObject.SetActive(showGrid);
            if (showGrid)
            {
                container.Enable();
            }
            else
            {
                container.Disable();
            }
            currentLine.gameObject.SetActive(showGrid);
            lanes.SetActive(showGrid);
        }
    }
}
