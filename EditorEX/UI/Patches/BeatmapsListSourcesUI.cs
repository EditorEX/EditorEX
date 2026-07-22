using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Config;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.SegmentedControl;
using EditorEX.Util;
using Reactive;
using Reactive.Yoga;
using SiraUtil.Affinity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.UI.Patches
{
    /// <summary>
    /// Builds the segmented source-tab control and the "New Map Source" add-source form on the
    /// beatmaps list, and reloads the list's cells/tabs when the underlying sources change.
    /// </summary>
    internal class BeatmapsListSourcesUI : IAffinity, IBeatmapsListRefresh
    {
        private readonly IReactiveContainer _reactiveContainer;
        private readonly SourcesConfig _sourcesConfig;
        private readonly BeatmapsCollectionDataModel _beatmapsCollectionDataModel;

        private State<int> _tab = StateUtils.Remember(0);
        private EditorSegmentedControl? _segmentedControl;
        private EditorStringInput? _filterInput;

        private BeatmapsListSourcesUI(
            IReactiveContainer reactiveContainer,
            SourcesConfig sourcesConfig,
            BeatmapsCollectionDataModel beatmapsCollectionDataModel
        )
        {
            _reactiveContainer = reactiveContainer;
            _sourcesConfig = sourcesConfig;
            _beatmapsCollectionDataModel = beatmapsCollectionDataModel;
        }

        public string FilterText => _filterInput?.InputField.text ?? string.Empty;

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

                new LayoutChildren
                {
                    new EditorSegmentedControl
                    {
                        SelectedIndex = _tab,
                        TabbingType = TabbingType.Alpha,
                    }
                        .AsFlexItem(size: new YogaVector(float.NaN, 30f))
                        .On(
                            _tab,
                            val =>
                            {
                                if (val != _segmentedControl!.Values.Length - 1)
                                {
                                    _sourcesConfig.SelectedSource =
                                        _sourcesConfig.Sources.Keys.ToList()[val];
                                    _beatmapsCollectionDataModel.RefreshCollection();
                                }
                            }
                        )
                        .Bind(ref _segmentedControl),
                    new LayoutChildren // Song List/Filter/Recently Modified
                    {
                        new LayoutElementComponent(
                            __instance
                                .transform.Find("RecentlyModifiedBeatmaps")
                                .gameObject.AddComponent<LayoutElement>()
                        ).AsFlexItem(),
                        new EditorStringInput()
                            .Bind(ref _filterInput)
                            .WithListener(
                                x => x.Text,
                                _ => _beatmapsCollectionDataModel.RefreshCollection()
                            )
                            .InEditorNamedRail("Filter", 18f)
                            .AsFlexItem(
                                size: new YogaVector(300f, 30f),
                                margin: new YogaFrame(100f, 0f, 50f, 0f)
                            ),
                        new LayoutElementComponent(beatmapList).AsFlexItem(
                            size: new YogaVector(100.pct, 50.pct)
                        ),
                    }
                        .AsLayout()
                        .AsFlexGroup(FlexDirection.Column)
                        .AsFlexItem(size: new YogaVector(100.pct, 100.pct))
                        .DisabledWithState(
                            _tab,
                            () => (int)Mathf.Max((_segmentedControl?.Values.Length ?? 2f) - 1, 1)
                        ),
                    new LayoutChildren // New Source
                    {
                        new EditorHeaderLabel
                        {
                            Text = "New Map Source",
                            FontSize = 24f,
                            Alignment = TextAlignmentOptions.Center,
                        }.AsFlexItem(size: new YogaVector(100.pct, 30f)),
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
                    }
                        .AsLayout()
                        .AsFlexGroup(FlexDirection.Column, gap: 20f)
                        .AsFlexItem(size: new YogaVector(40.pct, "auto"))
                        .EnabledWithState(
                            _tab,
                            () => (int)Mathf.Max((_segmentedControl?.Values.Length ?? 2f) - 1, 1)
                        ),
                }
                    .AsLayout()
                    .AsFlexItem(size: new YogaVector(1500f, 1000))
                    .AsFlexGroup(
                        FlexDirection.Column,
                        alignItems: Align.Center,
                        constrainHorizontal: false,
                        gap: 20f
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
    }
}
