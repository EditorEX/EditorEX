using System.Collections.Generic;
using System.Linq;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Dropdown;
using EditorEX.SDK.Settings;
using EditorEX.SDK.ViewContent;
using EditorEX.Util;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;

namespace EditorEX.SDKImplementation
{
    internal class EditorEXSettingsViewContent : IViewContent<SettingsViewData>
    {
        public ReactiveComponent Create()
        {
            return new LayoutChildren
            {
                new EditorHeaderLabel()
                {
                    Text = "Camera Settings",
                    FontStyle = TMPro.FontStyles.Bold,
                }.AsFlexItem(margin: new YogaFrame(0f, 0f, 0f, 10f)),
                new EditorToggle()
                {
                    OnClick = x =>
                    {
                        Debug.Log($"Toggle: {x}");
                    },
                }.InEditorNamedRail("Toggle", 18f),
                new EditorDropdown<string>
                {
                    Items = new Dictionary<string, BsDropdownItem>
                    {
                        { "Option 1", new BsDropdownItem("Option 1", null) },
                        { "Option 2", new BsDropdownItem("Option 2", null) },
                        { "Option 3", new BsDropdownItem("Option 3", null) },
                        { "Option 4", new BsDropdownItem("Option 4", null) },
                        { "Option 5", new BsDropdownItem("Option 5", null) },
                        { "Option 6", new BsDropdownItem("Option 6", null) },
                        { "Option 7", new BsDropdownItem("Option 7", null) },
                        { "Option 8", new BsDropdownItem("Option 8", null) },
                        { "Option 9", new BsDropdownItem("Option 9", null) },
                        { "Option 10", new BsDropdownItem("Option 10", null) },
                    },
                    Key = "Option 1",
                    OnKeyChanged = key => Debug.Log($"Dropdown: {key}"),
                }
                    .AsFlexItem(size: new YogaVector(350f, 40f))
                    .InEditorNamedRail("Dropdown", 18f),
                new EditorStringInput() { }
                    .AsFlexItem(size: new YogaVector(350f, 40f))
                    .InEditorNamedRail("Input", 18f),
            }
                .AsLayout()
                .AsFlexGroup(FlexDirection.Column, Justify.Center, gap: 10f)
                .AsFlexItem(size: new YogaVector(900f, "auto"));
        }

        public SettingsViewData GetViewData()
        {
            return new SettingsViewData("EditorEX");
        }

        public void OnEnable() { }

        public void OnHide() { }
    }
}
