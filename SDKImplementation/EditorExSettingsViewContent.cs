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
                    } 
                }
            }.AsFlexGroup(FlexDirection.Column);
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
