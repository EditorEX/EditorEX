using BeatmapEditor3D;
using BeatmapEditor3D.Visuals;
using EditorEX.Essentials.ViewMode;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EditorEX.Essentials.Patches
{
    internal class PreviewToggler : IAffinity, IDisposable
    {
        private ActiveViewMode _activeViewMode;

        private GameObject grid;
        private BeatlineContainer[] beatlines;
        private BeatmapObjectBeatLine currentLine;
        private BeatNumberContainer beatNumbers;
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
            beatlines = __instance.gameObject.GetComponentsInChildren<BeatlineContainer>().ToArray();
            currentLine = __instance.gameObject.GetComponentInChildren<BeatmapObjectBeatLine>();
            beatNumbers = __instance.gameObject.GetComponentInChildren<BeatNumberContainer>();
            lanes = __instance.transform.Find("BeatGridContainer").Find("GridLanes").gameObject;

            TogglePreview();
        }

        public void Dispose()
        {
            _activeViewMode.ModeChanged -= TogglePreview;
        }

        private void TogglePreview()
        {
            bool showGrid = _activeViewMode.Mode != "Preview";
            grid.SetActive(showGrid);
            foreach (var beatline in beatlines)
            {
                if (showGrid)
                {
                    //beatline.Enable();
                }
                else
                {
                    //beatline.Disable();
                }
            }
            if (showGrid)
            {
                beatNumbers.Enable();
            }
            else
            {
                beatNumbers.Disable();
            }
            currentLine.gameObject.SetActive(showGrid);
            lanes.SetActive(showGrid);
        }
    }
}
