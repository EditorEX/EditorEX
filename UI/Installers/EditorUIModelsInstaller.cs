using EditorEX.SDK.AddressableHelpers;
using EditorEX.SDK.ContextMenu;
using EditorEX.SDK.Settings;
using EditorEX.SDK.ViewContent;
using EditorEX.SDKImplementation;
using EditorEX.UI.Cursor;
using EditorEX.UI.Patches;
using EditorEX.UI.Patches.SDK;
using UnityEngine;
using Zenject;

namespace EditorEX.UI.Installers
{
    public class EditorUIModelsInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<IAddressableCollectorItem>()
                .To<DefaultAddressableCollectorItem<Material>>()
                .FromInstance(new("rounded-corners", "Visuals/Materials/UI/UINoGlowRoundEdge.mat"));
        }
    }
}
