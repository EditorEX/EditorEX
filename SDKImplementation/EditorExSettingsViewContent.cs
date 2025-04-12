using EditorEX.SDK.Settings;
using EditorEX.SDK.ViewContent;
using UnityEngine;

namespace EditorEX.SDKImplementation
{
    internal class EditorEXSettingsViewContent : IViewContent<SettingsViewData>
    {
        public void Create(GameObject host)
        {

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
