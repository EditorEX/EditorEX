using EditorEX.SDK.Integration.Patches;
using Zenject;

namespace EditorEX.SDK.Installers
{
    public class EditorSDKAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InjectCustomKeybindings>().AsSingle().NonLazy();
            Container
                .BindInterfacesAndSelfTo<CustomKeybindingPersistencePatches>()
                .AsSingle()
                .NonLazy();
        }
    }
}
