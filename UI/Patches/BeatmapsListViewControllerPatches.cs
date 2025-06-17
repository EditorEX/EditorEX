using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Config;
using EditorEX.SDK.Components;
using EditorEX.SDK.Factories;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.SegmentedControl;
using EditorEX.Util;
using HMUI;
using Reactive;
using Reactive.Yoga;
using SiraUtil.Affinity;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.UI.Patches
{
    internal class BeatmapsListViewControllerPatches : IAffinity
    {
        private readonly ReactiveContainer _reactiveContainer;
        private readonly SourcesConfig _sourcesConfig;
        private readonly BeatmapsCollectionDataModel _beatmapsCollectionDataModel;

        private ObservableValue<int> _tab = ValueUtils.Remember(0);
        private EditorSegmentedControl? _segmentedControl;
        private EditorStringInput? _filterInput;

        private BeatmapsListViewControllerPatches(
            ReactiveContainer reactiveContainer,
            SourcesConfig sourcesConfig,
            BeatmapsCollectionDataModel beatmapsCollectionDataModel
        )
        {
            _reactiveContainer = reactiveContainer;
            _sourcesConfig = sourcesConfig;
            _beatmapsCollectionDataModel = beatmapsCollectionDataModel;
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(BeatmapsListViewController),
            nameof(BeatmapsListViewController.DidActivate)
        )]
        private void ModifyUI(BeatmapsListViewController __instance, bool firstActivation)
        {
            if (firstActivation)
            {
                var beatmapList = __instance
                    .transform.Find("BeatmapsList")
                    .gameObject.AddComponent<LayoutElement>();
                beatmapList.GetComponent<RectTransform>().sizeDelta = new Vector2(-60f, 72f);
                beatmapList.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f);

                new Layout
                {
                    Children =
                    {
                        new EditorSegmentedControl
                        {
                            SelectedIndex = _tab,
                            TabbingType = TabbingType.Alpha,
                        }
                            .AsFlexItem(size: new YogaVector(float.NaN, 30f))
                            .Animate(
                                _tab,
                                () =>
                                {
                                    if (_tab.Value != _segmentedControl!.Values.Length - 1)
                                    {
                                        _sourcesConfig.SelectedSource =
                                            _sourcesConfig.Sources.Keys.ToList()[_tab.Value];
                                        _beatmapsCollectionDataModel.RefreshCollection();
                                    }
                                }
                            )
                            .Bind(ref _segmentedControl),
                        new Layout
                        { // Song List/Filter/Recently Modified
                            Children =
                            {
                                new LayoutElementComponent(
                                    __instance
                                        .transform.Find("RecentlyModifiedBeatmaps")
                                        .gameObject.AddComponent<LayoutElement>()
                                ).AsFlexItem(),
                                new EditorStringInput()
                                    .Bind(ref _filterInput)
                                    .InEditorNamedRail("Filter", 18f)
                                    .AsFlexItem(
                                        size: new YogaVector(300f, 30f),
                                        margin: new YogaFrame(100f, 0f, 50f, 0f)
                                    ),
                                new LayoutElementComponent(beatmapList).AsFlexItem(
                                    size: new YogaVector(100.pct(), 50.pct())
                                ),
                            },
                        }
                            .AsFlexGroup(FlexDirection.Column)
                            .AsFlexItem(size: new YogaVector(100.pct(), 100.pct()))
                            .DisabledWithObservable(
                                _tab,
                                () =>
                                    (int)Mathf.Max((_segmentedControl?.Values.Length ?? 2f) - 1, 1)
                            ),
                        new Layout
                        { // New Source
                            Children =
                            {
                                new EditorHeaderLabel
                                {
                                    Text = "New Map Source",
                                    FontSize = 24f,
                                    Alignment = TextAlignmentOptions.Center,
                                }.AsFlexItem(size: new YogaVector(100.pct(), 30f)),
                                new EditorStringInput()
                                    .Export(out var _newSourceName)
                                    .InEditorNamedRail("Source Name", 18f)
                                    .AsFlexItem(),
                                new EditorStringInput() //TODO: File Path Input Validator
                                    .Export(out var _newSourcePath)
                                    .InEditorNamedRail("Source Path", 18f)
                                    .AsFlexItem(),
                                new EditorLabelButton
                                {
                                    Text = "Add Source",
                                    OnClick = () =>
                                    {
                                        if (_newSourceName!.InputField.text != "")
                                        {
                                            _sourcesConfig.Sources.Add(
                                                _newSourceName!.InputField.text,
                                                _newSourcePath!.InputField.text
                                            );
                                            _newSourceName!.InputField.text = "";
                                            _newSourcePath!.InputField.text = "";

                                            _segmentedControl!.Values = SetupSources().ToArray();

                                            _tab.Value = _segmentedControl!.Values.Length - 1;
                                        }
                                    },
                                }.AsFlexItem(),
                            },
                        }
                            .AsFlexGroup(FlexDirection.Column)
                            .AsFlexItem()
                            .EnabledWithObservable(
                                _tab,
                                () =>
                                    (int)Mathf.Max((_segmentedControl?.Values.Length ?? 2f) - 1, 1)
                            ),
                    },
                }
                    .AsFlexItem(size: new YogaVector(1500f, 1000))
                    .AsFlexGroup(
                        FlexDirection.Column,
                        alignItems: Align.Center,
                        constrainHorizontal: false
                    )
                    .WithReactiveContainer(_reactiveContainer)
                    .Use(__instance.transform);
            }

            ReloadCells();

            __instance._beatmapsListTableView._tableView.RefreshCellsContent();
        }

        private IEnumerable<string> SetupSources()
        {
            var sources = _sourcesConfig.Sources.Keys.ToList();
            sources.Add("<size=20>+</size><voffset=0.2em>");

            return sources;
        }

        public void ReloadCells()
        {
            _segmentedControl!.Values = SetupSources().ToArray();

            _beatmapsCollectionDataModel.RefreshCollection();
        }

        private void ApplyFilter(BeatmapsListViewController instance)
        {
            var filteredMaps = BeatmapFilterUtil.Filter(
                instance._beatmapsCollectionDataModel._beatmapInfos,
                _filterInput!.InputField.text
            );
            instance._beatmapsListTableView.SetData(filteredMaps);
        }

        [AffinityPatch(
            typeof(BeatmapsListViewController),
            nameof(BeatmapsListViewController.DidActivate)
        )]
        [AffinityPostfix]
        private void Filter(BeatmapsListViewController __instance, bool firstActivation)
        {
            if (!firstActivation)
            {
                ApplyFilter(__instance);
            }
        }

        [AffinityPatch(
            typeof(BeatmapsListViewController),
            nameof(BeatmapsListViewController.HandleBeatmapsCollectionDataModelUpdated)
        )]
        [AffinityPrefix]
        private bool KeepFilter(BeatmapsListViewController __instance)
        {
            ApplyFilter(__instance);
            return false;
        }

        // Maintains the proper index.
        [AffinityPatch(
            typeof(BeatmapsListViewController),
            nameof(BeatmapsListViewController.HandleBeatmapListTableViewOpenBeatmap)
        )]
        [AffinityPrefix]
        private void FixIndex(BeatmapsListViewController __instance, ref int idx)
        {
            var filteredMaps = __instance._beatmapsListTableView._beatmapInfos;
            idx = __instance
                ._beatmapsCollectionDataModel.beatmapInfos.ToList()
                .IndexOf(filteredMaps[idx]);
        }
    }
}
