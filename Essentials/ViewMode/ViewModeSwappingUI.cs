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
    internal class ViewModeSwappingUI : ITickable
    {
        private ActiveViewMode _activeViewMode;
        private BeatmapObjectsView _beatmapObjectsView;

        [Inject]
        private void Construct(ActiveViewMode activeViewMode, BeatmapObjectsView beatmapObjectsView)
        {
            _activeViewMode = activeViewMode;
            _beatmapObjectsView = beatmapObjectsView;
        }

        public void Tick()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    _activeViewMode.Mode = "Normal";
                    _activeViewMode.ModeChanged();
                    _beatmapObjectsView._notesBeatmapObjectsView.ClearObjects();
                    _beatmapObjectsView._notesBeatmapObjectsView.ClearPool();
                    _beatmapObjectsView.gameObject.SetActive(false);
                    _beatmapObjectsView.gameObject.SetActive(true);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    _activeViewMode.Mode = "Preview";
                    _activeViewMode.ModeChanged();
                    _beatmapObjectsView._notesBeatmapObjectsView.ClearObjects();
                    _beatmapObjectsView._notesBeatmapObjectsView.ClearPool();
                    _beatmapObjectsView.gameObject.SetActive(false);
                    _beatmapObjectsView.gameObject.SetActive(true);
                }
            }
        }
    }
}
