using BeatmapEditor3D;
using System.Linq;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Features.HideUI
{
    internal class HideUIImplementation
    {
        private BeatmapEditorScreenSystem? _screenSystem;

        private HideUIImplementation(SignalBus signalBus)
        {
            signalBus.Subscribe<HideUIFeatureToggledSignal>(ToggleUI);
        }

        private void ToggleUI()
        {
            if (_screenSystem == null)
            {
                _screenSystem = Resources.FindObjectsOfTypeAll<BeatmapEditorScreenSystem>().FirstOrDefault();
            }
            _screenSystem?.gameObject.SetActive((!_screenSystem?.gameObject?.activeSelf) ?? true);
        }
    }
}
