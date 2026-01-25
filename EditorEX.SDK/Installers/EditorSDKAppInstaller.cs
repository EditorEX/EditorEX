using EditorEX.SDKImplementation.Patches;
using Zenject;

namespace EditorEX.SDK.Installers
{
    public class EditorSDKAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InjectCustomKeybindings>().AsSingle().NonLazy();
        }
    }
}
