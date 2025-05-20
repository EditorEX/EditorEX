using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Dropdown;
using EditorEX.SDK.Settings;
using EditorEX.SDK.ViewContent;
using EditorEX.Util;
using Reactive;
using Reactive.Yoga;
using UnityEngine;

namespace EditorEX.SDKImplementation
{
    internal class EditorEXSettingsViewContent : IViewContent<SettingsViewData>
    {
        public ReactiveComponent Create()
        {
            return new Layout()
            {
                Children = {
                    new EditorHeaderLabel() {
                        Text = "Camera Settings",
                        FontStyle = TMPro.FontStyles.Bold,
                    }.AsFlexItem(margin: new YogaFrame(0f, 0f, 0f, 10f)),
                    new EditorToggle() {
                        OnClick = x => { Debug.Log($"Toggle: {x}"); }
                    }.InEditorNamedRail("Toggle", 18f),
                    new EditorTextDropdown<string>() {
                        Items = {
                            {"Option 1", "Option 1"},
                            {"Option 2", "Option 2"},
                            {"Option 3", "Option 3"},
                            {"Option 4", "Option 4"},
                            {"Option 5", "Option 5"},
                            {"Option 6", "Option 6"},
                            {"Option 7", "Option 7"},
                            {"Option 8", "Option 8"},
                            {"Option 9", "Option 9"},
                            {"Option 10", "Option 10"},
                        }
                    }.AsFlexItem(size: new YogaVector(350f, 40f)).InEditorNamedRail("Dropdown", 18f),
                    new EditorStringInput() {
                    }.AsFlexItem(size: new YogaVector(350f, 40f)).InEditorNamedRail("Input", 18f),
                }
            }.AsFlexGroup(FlexDirection.Column, Justify.Center, gap: 10f)
            .AsFlexItem(size: new YogaVector(900f, "auto"));
        }

        public SettingsViewData GetViewData()
        {
            return new SettingsViewData("EditorEX");
        }

        public void OnEnable()
        {
        }

        public void OnHide()
        {
        }
    }
}
