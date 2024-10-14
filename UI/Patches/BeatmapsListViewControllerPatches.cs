using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Config;
using EditorEX.SDK.Collectors;
using EditorEX.SDK.Components;
using EditorEX.SDK.Factories;
using HMUI;
using SiraUtil.Affinity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace EditorEX.UI.Patches
{
    internal class BeatmapsListViewControllerPatches : IAffinity
    {
        private readonly TextSegmentedControlFactory _textSegmentedControlFactory;
        private readonly ButtonFactory _buttonFactory;
        private readonly SourcesConfig _sourcesConfig;
        private readonly BeatmapsCollectionDataModel _beatmapsCollectionDataModel;

        private TextSegmentedControl _segmentedControl;
        private TabbingSegmentedControlController _tabbingSegmentedControlController;

        private List<GameObject> gameObjects = new List<GameObject>();

        private BeatmapsListViewControllerPatches(
            TextSegmentedControlFactory textSegmentedControlFactory,
            ButtonFactory buttonFactory,
            SourcesConfig sourcesConfig,
            BeatmapsCollectionDataModel beatmapsCollectionDataModel)
        {
            _textSegmentedControlFactory = textSegmentedControlFactory;
            _buttonFactory = buttonFactory;
            _sourcesConfig = sourcesConfig;
            _beatmapsCollectionDataModel = beatmapsCollectionDataModel;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapsListViewController), nameof(BeatmapsListViewController.DidActivate))]
        private void ModifyUI(BeatmapsListViewController __instance, bool firstActivation)
        {
            if (firstActivation)
            {
                var sources = _sourcesConfig.Sources.Keys.ToList();
                sources.Add("+");

                __instance.gameObject.SetActive(false);

                _segmentedControl = _textSegmentedControlFactory.Create(__instance.transform, sources, Selected);
                (_segmentedControl.transform as RectTransform).anchoredPosition = new Vector2(0f, 500f);
                _tabbingSegmentedControlController = _segmentedControl.gameObject.AddComponent<TabbingSegmentedControlController>();
                _tabbingSegmentedControlController.Setup(_segmentedControl);

                var button = _buttonFactory.Create(__instance.transform, "Add Path", AddPath);
                button.transform.localPosition = new Vector2(800f, 500f);

                (_segmentedControl.cells.Last().transform as RectTransform).sizeDelta = new Vector2(80f, 30f);

                __instance.gameObject.SetActive(true);
            }
            else
            {
                _tabbingSegmentedControlController.AddBindings();
            }

            _tabbingSegmentedControlController.ClickCell(0);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapsListViewController), nameof(BeatmapsListViewController.DidDeactivate))]
        private void DidDeactivate()
        {
            _tabbingSegmentedControlController.ClearBindings();
        }

        private void Selected(SegmentedControl segmentedControl, int idx)
        {
            if (idx < segmentedControl.cells.Count - 1)
            {
                _sourcesConfig.SelectedSource = _sourcesConfig.Sources.Keys.ToList()[idx];
                _beatmapsCollectionDataModel.RefreshCollection();
            }
        }

        private void AddPath()
        {
            var newSource = NativeFileDialogs.OpenDirectoryDialog("New Custom levels Directoy", _sourcesConfig.SelectedSource);

            if (newSource != null && Directory.Exists(newSource))
                _sourcesConfig.Sources[_sourcesConfig.SelectedSource].Add(newSource.Replace("\\", "/"));
        }
    }
}
