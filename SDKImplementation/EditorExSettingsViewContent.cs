using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.Settings;
using EditorEX.SDK.ViewContent;
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
                    new EditorNamedRail() {
                        Label = { Text = "Silly", FontSize = 18f },
                        Component = new EditorLabelButton() {
                            Text = "Click me!",
                            OnClick = () => { Debug.Log("Clicked!"); },
                        }
                    }.AsFlexItem(),
                }
            }.AsFlexGroup(FlexDirection.Column).AsFlexItem(size: new YogaVector(1000f, "auto"));
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
