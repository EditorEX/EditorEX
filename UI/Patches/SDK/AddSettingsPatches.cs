using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.Settings;
using EditorEX.SDK.ViewContent;
using EditorEX.Util;
using Reactive;
using Reactive.Yoga;
using SiraUtil.Affinity;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.UI.Patches.SDK
{
    internal class AddSettingsPatches : IAffinity
    {
        private readonly List<IViewContent<SettingsViewData>> _viewContents;
        private readonly List<string> _viewNames;
        private readonly ReactiveContainer _reactiveContainer;

        private AddSettingsPatches(
            List<IViewContent<SettingsViewData>> viewContents,
            ReactiveContainer reactiveContainer
        )
        {
            _viewContents = viewContents;
            _reactiveContainer = reactiveContainer;
            _viewNames = _viewContents.Select(x => x.GetViewData().Name).ToList();
            _viewNames.Insert(0, "Official");
        }

        [AffinityPatch(
            typeof(BeatmapEditorSettingsViewController),
            nameof(BeatmapEditorSettingsViewController.DidActivate)
        )]
        [AffinityPostfix]
        private void AddUI(BeatmapEditorSettingsViewController __instance, bool firstActivation)
        {
            if (firstActivation)
            {
                var tab = ValueUtils.Remember(0);

                new LayoutChildren
                {
                    new EditorSegmentedControl()
                    {
                        Values = _viewNames.ToArray(),
                        SelectedIndex = tab,
                    }
                }
                .AsLayout()
                .AsFlexGroup(FlexDirection.Column, gap: 20f, padding: 30)
                .Export(out var layout)
                .WithReactiveContainer(_reactiveContainer)
                .Use(__instance.transform);

                var vanillaContainer = __instance
                    .transform.Find("Container")
                    .gameObject.AddComponent<LayoutElement>();
                vanillaContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);

                layout.Children.Add(
                    new LayoutElementComponent(vanillaContainer)
                        .AsFlexItem()
                        .EnabledWithObservable(tab, 0)
                );
                layout.Children.AddRange(
                    _viewContents.Select(
                        (x, index) => x.Create().EnabledWithObservable(tab, index + 1)
                    )
                );
            }
        }
    }
}
