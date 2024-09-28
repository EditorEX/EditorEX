using BeatmapEditor3D;
using BeatmapEditor3D.Views;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.UI.Collectors
{
    public class PrefabCollector : IInitializable
    {
        private DiContainer _container;

        private Button _buttonPrefab;
        private Button _iconButtonPrefab;
        private Toggle _togglePrefab;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        public void Initialize()
        {
            var newBeatmapViewController = _container.Resolve<NewBeatmapViewController>();

            _buttonPrefab = newBeatmapViewController._saveAndOpenBeatmapButton;
            _iconButtonPrefab = newBeatmapViewController._openSongView._openFileButton;


            var beatmapEditorSettingsViewController = _container.Resolve<BeatmapEditorSettingsViewController>();

            _togglePrefab = beatmapEditorSettingsViewController._fullscreenToggle;
        }

        public Button GetButtonPrefab()
        {
            return _buttonPrefab;
        }

        public Button GetIconButtonPrefab()
        {
            return _iconButtonPrefab;
        }
    }
}
