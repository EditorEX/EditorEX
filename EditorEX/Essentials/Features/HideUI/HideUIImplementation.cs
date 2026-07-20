using System;
using System.Linq;
using BeatmapEditor3D;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Features.HideUI
{
    internal class HideUIImplementation
    {
        private BeatmapEditorScreenSystem? _screenSystem;

        public bool IsUiHidden => !(_screenSystem?.gameObject.activeSelf ?? true);
        public event Action? OnUiHiddenChanged;

        private HideUIImplementation(SignalBus signalBus)
        {
            signalBus.Subscribe<HideUIFeatureToggledSignal>(ToggleUi);
        }

        private void ToggleUi()
        {
            if (_screenSystem == null)
            {
                _screenSystem = Resources
                    .FindObjectsOfTypeAll<BeatmapEditorScreenSystem>()
                    .FirstOrDefault();
            }

            _screenSystem?.gameObject.SetActive((!_screenSystem?.gameObject.activeSelf) ?? true);
            OnUiHiddenChanged?.Invoke();
        }
    }
}
