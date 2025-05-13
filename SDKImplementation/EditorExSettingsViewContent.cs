using EditorEX.SDK.ReactiveComponents;
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
            return new Layout() {
                Children = {
                    new EditorImage() {
                        Source = "https://picsum.photos/300"
                    }.AsFlexItem(aspectRatio: 1f, maxSize: 300),
                    new EditorToggle() {
                        OnClick = x => { Debug.Log($"Toggle: {x}"); }
                    }.InEditorNamedRail("Toggle", 18f),
                }
            }.AsFlexGroup(FlexDirection.Column, Justify.Center).AsFlexItem(size: new YogaVector(1000f, "auto"));
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
