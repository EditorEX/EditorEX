using System;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using BeatmapEditor3D.Views;
using BeatmapEditor3D.Visuals;
using EditorEX.Essentials.Features.ViewMode;
using SiraUtil.Affinity;
using UnityEngine;

namespace EditorEX.Essentials.Patches.Preview
{
    internal class PreviewToggler : IAffinity, IDisposable
    {
        private ActiveViewMode _activeViewMode;
        private IReadonlyBeatmapState _beatmapState;

        private GameObject grid;
        private BeatmapObjectGridHoverView hover;
        private GameObject selection;
        private Transform currentLine;
        private BeatGridContainer container;
        private GameObject lanes;
        private GameObject hjdLine;

        public PreviewToggler(
            ActiveViewMode activeViewMode,
            EditorGameplayCoreSceneSetupData setupData
        )
        {
            _activeViewMode = activeViewMode;
            _activeViewMode.ModeChanged += TogglePreview;
            _beatmapState = setupData.beatmapState;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectsContainer), nameof(BeatmapObjectsContainer.OnEnable))]
        private void Patch(BeatmapObjectsContainer __instance)
        {
            grid = __instance.transform.Find("BeatmapObjectSelectionGridView").gameObject;
            hover = __instance
                .transform.Find("BeatmapObjectGridHoverView")
                .GetComponent<BeatmapObjectGridHoverView>();
            selection = __instance.transform.Find("BeatmapObjectSelectionView").gameObject;
            container = __instance.GetComponentInChildren<BeatGridContainer>();
            currentLine = container._currentBeatLineTransform;
            lanes = __instance.transform.Find("BeatGridContainer").Find("GridLanes").gameObject;
            hjdLine = __instance
                .transform.Find("BeatGridContainer")
                .Find("HalfJumpDurationBeatline")
                .gameObject;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatGridContainer), nameof(BeatGridContainer.Enable))]
        private bool Patch(BeatGridContainer __instance)
        {
            if (_beatmapState.editingMode != BeatmapEditingMode.Objects)
            {
                try
                {
                    __instance._mainBeatlineContainer.Enable();
                    __instance._beatNumberContainer.Enable();
                    __instance._normalBeatlineContainer.Enable();
                }
                catch
                {
                    // ignored
                }

                return false;
            }
            return _activeViewMode.Mode.ShowGridAndSelection;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatGridContainer), nameof(BeatGridContainer.ForceUpdate))]
        private bool Patch2(BeatGridContainer __instance)
        {
            if (_beatmapState.editingMode != BeatmapEditingMode.Objects)
            {
                try
                {
                    __instance._mainBeatlineContainer.Enable();
                    __instance._beatNumberContainer.Enable();
                    __instance._normalBeatlineContainer.Enable();
                }
                catch
                {
                    // ignored
                }

                return true;
            }
            return _activeViewMode.Mode.ShowGridAndSelection;
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
                //container._mainBeatlineContainer.enabled = true;
                //container._beatNumberContainer.enabled = true;
                //container._normalBeatlineContainer.enabled = true;
                container._mainBeatlineContainer.Enable();
                container._beatNumberContainer.Enable();
                container._normalBeatlineContainer.Enable();
            }
            else
            {
                container._mainBeatlineContainer.Disable();
                container._beatNumberContainer.Disable();
                container._normalBeatlineContainer.Disable();
                //container._mainBeatlineContainer.enabled = false;
                //container._beatNumberContainer.enabled = false;
                //container._normalBeatlineContainer.enabled = false;
            }
            currentLine.gameObject.SetActive(showGrid);
            lanes.SetActive(showGrid);
            hjdLine.SetActive(showGrid);
        }
    }
}
