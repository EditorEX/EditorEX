using BeatmapEditor3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.ViewMode
{
    public class ViewModeSwapper : ITickable
    {
        private ActiveViewMode _activeViewMode;
        private BeatmapObjectsView _beatmapObjectsView;

        [Inject]
        private void Construct(ActiveViewMode activeViewMode, BeatmapObjectsView beatmapObjectsView)
        {
            _activeViewMode = activeViewMode;
            _activeViewMode.Mode = ViewModeRepository.GetViewModes().FirstOrDefault(x => x.ID == "normal");
            _beatmapObjectsView = beatmapObjectsView;
        }

        private void SetMode(ViewMode mode)
        {
            _activeViewMode.Mode = mode;
            _activeViewMode.ModeChanged();
            _beatmapObjectsView._notesBeatmapObjectsView.ClearObjects();
            _beatmapObjectsView._notesBeatmapObjectsView.ClearPool();
            _beatmapObjectsView.gameObject.SetActive(false);
            _beatmapObjectsView.gameObject.SetActive(true);
        }

        public void Tick()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                var modes = ViewModeRepository.GetViewModes();
                for (int i = 1; i < modes.Count + 1; i++)
                {
                    var mode = modes[i - 1];
                    if (Input.GetKeyDown(KeyCode.Alpha1 + (i - 1)) && _activeViewMode.Mode != mode)
                    {
                        SetMode(mode);
                    }
                }
            }
        }
    }
}
