using BeatmapEditor3D;
using BeatmapEditor3D.Controller;
using EditorEX.Essentials.ViewMode;
using HMUI;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System.Linq;
using UnityEngine;
using Zenject;

namespace EditorEX.Essentials.Patches
{
    internal class HideUI : ITickable
    {
        private readonly KeyboardBinder _keyboardBinder;
        private BeatmapEditorScreenSystem _screenSystem;

        private HideUI(SiraLog siraLog)
        {
            siraLog.Info("HI");
            _keyboardBinder = new KeyboardBinder();
            _keyboardBinder.AddBinding(KeyCode.L, KeyboardBinder.KeyBindingType.KeyDown, ToggleUI);
        }

        public void Tick()
        {
            _keyboardBinder.ManualUpdate();
        }

        private void ToggleUI(bool what)
        {
            if (_screenSystem == null)
            {
                _screenSystem = Resources.FindObjectsOfTypeAll<BeatmapEditorScreenSystem>().FirstOrDefault();
            }
            _screenSystem?.gameObject.SetActive((!_screenSystem?.gameObject?.activeSelf) ?? true);
        }
    }
}
