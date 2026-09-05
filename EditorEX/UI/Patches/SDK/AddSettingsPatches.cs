using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.Settings;
using EditorEX.SDK.ViewContent;
using Reactive;
using Reactive.Yoga;
using SiraUtil.Affinity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.UI.Patches.SDK
{
    internal class AddSettingsPatches : IAffinity
    {
        private readonly List<IViewContent<SettingsViewData>> _viewContents;
        private readonly List<string> _viewNames;
        private readonly IReactiveContainer _reactiveContainer;

        private AddSettingsPatches(
            List<IViewContent<SettingsViewData>> viewContents,
            IReactiveContainer reactiveContainer
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
                HideCustomLevelsFolderAndHeader(__instance);

                var tab = StateUtils.Remember(0);

                new LayoutChildren
                {
                    new EditorSegmentedControl()
                    {
                        Values = _viewNames.ToArray(),
                        SelectedIndex = tab,
                    }.AsFlexItem(size: new YogaVector(float.NaN, 30f)),
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
                        .EnabledWithState(tab, 0)
                );
                layout.Children.AddRange(
                    _viewContents.Select((x, index) => x.Create().EnabledWithState(tab, index + 1))
                );
            }
        }

        private static void HideCustomLevelsFolderAndHeader(
            BeatmapEditorSettingsViewController settings
        )
        {
            var folder = settings._openFolderView.gameObject;
            var parent = folder.transform.parent;
            if (parent != null && parent != settings.transform && parent.name != "Container")
            {
                parent.gameObject.SetActive(false);
            }
            else
            {
                folder.SetActive(false);
            }

            foreach (var label in settings.GetComponentsInChildren<TMP_Text>(true))
            {
                if (
                    label.text != null
                    && label
                        .text.Trim()
                        .Equals("General Settings", StringComparison.OrdinalIgnoreCase)
                )
                {
                    label.gameObject.SetActive(false);
                }
            }
        }
    }
}
