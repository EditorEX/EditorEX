using BeatmapEditor3D;
using EditorEX.SDK.Factories;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.Settings;
using EditorEX.SDK.ViewContent;
using EditorEX.Util;
using HMUI;
using Reactive;
using Reactive.Yoga;
using SiraUtil.Affinity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.UI.Patches.SDK
{
    internal class AddSettingsPatches : IAffinity
    {
        private readonly List<IViewContent<SettingsViewData>> _viewContents;
        private readonly List<string> _viewNames;
        private readonly ReactiveContainer _reactiveContainer;

        private int currentSelected = 0;

        private AddSettingsPatches(
            List<IViewContent<SettingsViewData>> viewContents,
            ReactiveContainer reactiveContainer)
        {
            _viewContents = viewContents;
            _reactiveContainer = reactiveContainer;
            _viewNames = _viewContents.Select(x => x.GetViewData().Name).ToList();
            _viewNames.Insert(0, "Official");
        }

        [AffinityPatch(typeof(BeatmapEditorSettingsViewController), nameof(BeatmapEditorSettingsViewController.DidActivate))]
        [AffinityPostfix]
        private void AddUI(BeatmapEditorSettingsViewController __instance, bool firstActivation)
        {
            if (firstActivation)
            {
                var tab = ValueUtils.Remember(0);
                
                new Layout() {
                    Children = {
                        new EditorSegmentedControl() {
                            Values = _viewNames.ToArray(),
                            SelectedIndex = tab
                        }.AsFlexItem(size: new YogaVector("auto", 30f))
                    }
                }.AsFlexGroup(FlexDirection.Column, gap: 10f, padding: 10)
                .Export(out var layout)
                .WithReactiveContainer(_reactiveContainer)
                .Use(__instance.transform);

                var vanillaContainer = __instance.transform.Find("Container").gameObject.AddComponent<LayoutElement>();

                layout.Children.Add(new LayoutElementComponent(vanillaContainer).AsFlexItem().EnabledWithObservable(tab, 0));
                layout.Children.AddRange(_viewContents.Select((x, index) => x.Create().EnabledWithObservable(tab, index + 1)));
            }
        }
    }
}
