using BeatmapEditor3D;
using BeatmapEditor3D.Views;
using HMUI;
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
        private TextSegmentedControl _segmentedControlPrefab;
        private TMP_InputField _inputFieldPrefab;

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


            var editBeatmapViewController = _container.Resolve<EditBeatmapViewController>();

            _segmentedControlPrefab = editBeatmapViewController._eventBoxesView._eventBoxButtonsTextSegmentedControl;


            var editBeatmapLevelViewController = _container.Resolve<EditBeatmapLevelViewController>();

            _inputFieldPrefab = editBeatmapLevelViewController._songNameInputValidator.GetComponent<TMP_InputField>();
        }

        public Button GetButtonPrefab()
        {
            return _buttonPrefab;
        }

        public Button GetIconButtonPrefab()
        {
            return _iconButtonPrefab;
        }

        public TextSegmentedControl GetSegmentedControlPrefab()
        {
            return _segmentedControlPrefab;
        }

        public TMP_InputField GetInputFieldPrefab()
        {
            return _inputFieldPrefab;
        }
    }
}
