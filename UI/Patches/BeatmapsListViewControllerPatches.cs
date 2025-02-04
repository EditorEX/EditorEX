using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Config;
using EditorEX.SDK.AddressableHelpers;
using EditorEX.SDK.Components;
using EditorEX.SDK.Factories;
using EditorEX.Util;
using HMUI;
using SiraUtil.Affinity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;
using Zenject;
using static UnityEngine.SpookyHash;

namespace EditorEX.UI.Patches
{
    internal class BeatmapsListViewControllerPatches : IAffinity, IInitializable
    {
        private readonly TextSegmentedControlFactory _textSegmentedControlFactory;
        private readonly ButtonFactory _buttonFactory;
        private readonly TextFactory _textFactory;
        private readonly StringInputFactory _stringInputFactory;
        private readonly SourcesConfig _sourcesConfig;
        private readonly BeatmapsCollectionDataModel _beatmapsCollectionDataModel;

        private Material _roundedCornersMaterial;

        private TextSegmentedControl _segmentedControl;
        private TabbingSegmentedControlController _tabbingSegmentedControlController;

        private List<GameObject> gameObjects = new();

        // New Source
        private TMP_InputField _tempNewSourceName;

        private BeatmapsListViewControllerPatches(
            TextSegmentedControlFactory textSegmentedControlFactory,
            ButtonFactory buttonFactory,
            TextFactory textFactory,
            StringInputFactory stringInputFactory,
            SourcesConfig sourcesConfig,
            BeatmapsCollectionDataModel beatmapsCollectionDataModel)
        {
            _textSegmentedControlFactory = textSegmentedControlFactory;
            _buttonFactory = buttonFactory;
            _textFactory = textFactory;
            _stringInputFactory = stringInputFactory;
            _sourcesConfig = sourcesConfig;
            _beatmapsCollectionDataModel = beatmapsCollectionDataModel;
        }

        public void Initialize()
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<string> locationsList = new();

            foreach (var locator in Addressables.ResourceLocators)
            {
                foreach (var key in locator.Keys)
                {
                    IList<IResourceLocation> locations = new List<IResourceLocation>();
                    if (locator.Locate(key, typeof(object), out locations))
                    {
                        foreach (var location in locations)
                        {
                            locationsList.Add(location.PrimaryKey);
                        }
                    }
                }
            }

            foreach (var location in locationsList.Distinct())
            {
                stringBuilder.AppendLine(location.ToString());
            }

            File.WriteAllText("test.txt", stringBuilder.ToString());
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapsListViewController), nameof(BeatmapsListViewController.DidActivate))]
        private void ModifyUI(BeatmapsListViewController __instance, bool firstActivation)
        {
            if (firstActivation)
            {
                __instance.gameObject.SetActive(false);

                _segmentedControl = _textSegmentedControlFactory.Create(__instance.transform, SetupSources(), Selected);
                (_segmentedControl.transform as RectTransform).anchoredPosition = new Vector2(0f, 500f);
                _tabbingSegmentedControlController = _segmentedControl.gameObject.AddComponent<TabbingSegmentedControlController>();
                _tabbingSegmentedControlController.Setup(_segmentedControl, false);

                (_segmentedControl.cells.Last().transform as RectTransform).sizeDelta = new Vector2(80f, 30f);
                _segmentedControl.gameObject.name = "SourcesSegmentedControl";

                _segmentedControl.ReloadData();

                var ListParent = new GameObject("ListParent");
                ListParent.transform.SetParent(__instance.transform, false);

                var recentlyModified = __instance._recentBeatmapsView.transform.parent;
                var beatmapsList = __instance._beatmapsListTableView.transform;
                var search = __instance.transform.Find("ExStringInput");

                recentlyModified.SetParent(ListParent.transform, true);
                beatmapsList.SetParent(ListParent.transform, true);
                search.SetParent(ListParent.transform, true);

                gameObjects.Add(ListParent);

                var NewSourceParent = new GameObject("NewSourceParent");
                NewSourceParent.transform.SetParent(__instance.transform, false);
                NewSourceParent.transform.localPosition = new Vector3(0f, 200f, 0f);
                NewSourceParent.AddComponent<RectTransform>().sizeDelta = new Vector2(400f, 100f);

                var layout = NewSourceParent.AddComponent<VerticalLayoutGroup>();
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.spacing = 50f;

                var parentFitter = NewSourceParent.AddComponent<ContentSizeFitter>();
                parentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                parentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var header = _textFactory.Create(NewSourceParent.transform, "New Map Source", 25f, "Button/Text/Normal");
                var fitter = header.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                _tempNewSourceName = _stringInputFactory.Create(NewSourceParent.transform, "Source Name", 200f, null);
                (_tempNewSourceName.transform.parent as RectTransform).sizeDelta = new Vector2(400f, 40f);

                _buttonFactory.Create(NewSourceParent.transform, "Add Source", AddSource);

                gameObjects.Add(NewSourceParent);

                __instance.gameObject.SetActive(true);
            }
            else
            {
                _tabbingSegmentedControlController.AddBindings(false);
            }

            _tabbingSegmentedControlController.ClickCell(0);
            Selected(_segmentedControl, 0);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapsListViewController), nameof(BeatmapsListViewController.DidDeactivate))]
        private void DidDeactivate()
        {
            _tabbingSegmentedControlController.ClearBindings();
        }

        private IEnumerable<string> SetupSources()
        {
            var sources = _sourcesConfig.Sources.Keys.ToList();
            sources.Add("<size=20>+</size><voffset=0.2em>");

            for (int i = 0; i < sources.Count; i++)
            {
                sources[i] += $" | {i + 1}";
            }
            return sources;
        }

        private void Selected(SegmentedControl segmentedControl, int idx)
        {
            if (idx < segmentedControl.cells.Count - 1)
            {
                _sourcesConfig.SelectedSource = _sourcesConfig.Sources.Keys.ToList()[idx];
                _beatmapsCollectionDataModel.RefreshCollection();
            }

            idx = (idx == segmentedControl.cells.Count - 1) ? 1 : 0;

            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].SetActive(i == idx);
            }
        }

        private void AddSource()
        {
            if (_tempNewSourceName.text != "")
            {
                _sourcesConfig.Sources.Add(_tempNewSourceName.text, new List<string>());
                _tempNewSourceName.text = "";

                _segmentedControl.SetTexts(SetupSources().ToArray());
                _segmentedControl.ReloadData();

                _tabbingSegmentedControlController.ClickCell(_segmentedControl.cells.Count - 1);
            }
        }

        public void ReloadCells()
        {
            _segmentedControl.SetTexts(SetupSources().ToArray());
            _segmentedControl.ReloadData();

            _beatmapsCollectionDataModel.RefreshCollection();
        }
    }
}
