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

namespace EditorEX.UI.Patches
{
    internal class BeatmapsListViewControllerPatches : IAffinity
    {
        private readonly ReactiveContainer _reactiveContainer;
        private readonly SourcesConfig _sourcesConfig;
        private readonly BeatmapsCollectionDataModel _beatmapsCollectionDataModel;

        private ObservableValue<int> _tab = ValueUtils.Remember(0);
        private EditorSegmentedControl? _segmentedControl;

        private BeatmapsListViewControllerPatches(
            ReactiveContainer reactiveContainer,
            SourcesConfig sourcesConfig,
            BeatmapsCollectionDataModel beatmapsCollectionDataModel)
        {
            _reactiveContainer = reactiveContainer;
            _sourcesConfig = sourcesConfig;
            _beatmapsCollectionDataModel = beatmapsCollectionDataModel;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapsListViewController), nameof(BeatmapsListViewController.DidActivate))]
        private void ModifyUI(BeatmapsListViewController __instance, bool firstActivation)
        {
            if (firstActivation)
            {
                new Layout
                {
                    Children = {
                        new EditorSegmentedControl {
                            SelectedIndex = _tab,
                            TabbingType = TabbingType.Alpha
                        }.AsFlexItem(size: new YogaVector(500f, 30f))
                        .Bind(ref _segmentedControl),

                        new Layout { // Song List/Filter/Recently Modified
                            Children = {
                                new LayoutElementComponent(__instance.transform.Find("RecentlyModifiedBeatmaps").gameObject.AddComponent<LayoutElement>())
                                .AsFlexItem(),

                                new EditorStringInput {

                                }
                                .InEditorNamedRail("Filter", 18f)
                                .AsFlexItem(size: new YogaVector(300f, 30f), margin: new YogaFrame(50f, 0f, 30f, 0f)),

                                new LayoutElementComponent(__instance.transform.Find("BeatmapsList").gameObject.AddComponent<LayoutElement>())
                                .AsFlexItem(flexGrow: 1f)
                            }
                        }.AsFlexGroup(FlexDirection.Column)
                        .AsFlexItem()
                        .DisabledWithObservable(_tab, () => (int)Mathf.Min((_segmentedControl?.Values.Length ?? 2f) - 1 , 1)),

                        new Layout { // New Source
                            Children = {

                            }
                        }.AsFlexGroup(FlexDirection.Column)
                        .AsFlexItem()
                        .EnabledWithObservable(_tab, () => (int)Mathf.Min((_segmentedControl?.Values.Length ?? 2f) - 1 , 1)),
                    }
                }.AsFlexGroup(FlexDirection.Column, alignItems: Align.Center)
                .WithReactiveContainer(_reactiveContainer)
                .Use(__instance.transform);
            }

            ReloadCells();
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
