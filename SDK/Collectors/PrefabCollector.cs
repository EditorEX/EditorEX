using BeatmapEditor3D;
using BeatmapEditor3D.Views;
using HMUI;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.SDK.Collectors
{
    public class PrefabCollector : IInitializable
    {
        private DiContainer _container;

        private Button _buttonPrefab;
        private Button _iconButtonPrefab;
        private Toggle _togglePrefab;
        private TextSegmentedControl _segmentedControlPrefab;
        private TMP_InputField _inputFieldPrefab;
        private ScrollView _scrollViewPrefab;
        private DropdownEditorView _textDropdownPrefab;

        [Inject]
        private void Construct(
            DiContainer container)
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
            _textDropdownPrefab = beatmapEditorSettingsViewController._screenResolutionDropdown;


            var editBeatmapViewController = _container.Resolve<EditBeatmapViewController>();
            _segmentedControlPrefab = editBeatmapViewController.transform.Find("StatusBarView").Find("PaginationView").Find("Wrapper").Find("BasicEventsPagination").Find("BeatmapEditorTextSegmentedView").GetComponent<TextSegmentedControl>();


            var editBeatmapLevelViewController = _container.Resolve<EditBeatmapLevelViewController>();
            _inputFieldPrefab = editBeatmapLevelViewController._songNameInputValidator.GetComponent<TMP_InputField>();


            var beatmapsListViewController = _container.Resolve<BeatmapsListViewController>();
            _scrollViewPrefab = beatmapsListViewController._beatmapsListTableView._tableView.GetComponent<ScrollView>();
        }

        public Button GetButtonPrefab()
        {
            return _buttonPrefab;
        }

        public Button GetIconButtonPrefab()
        {
            return _iconButtonPrefab;
        }

        public Toggle GetTogglePrefab()
        {
            return _togglePrefab;
        }

        public TextSegmentedControl GetSegmentedControlPrefab()
        {
            return _segmentedControlPrefab;
        }

        public TMP_InputField GetInputFieldPrefab()
        {
            return _inputFieldPrefab;
        }

        public ScrollView GetScrollViewPrefab()
        {
            return _scrollViewPrefab;
        }

        public DropdownEditorView GetTextDropdownPrefab()
        {
            return _textDropdownPrefab;
        }
    }
}
