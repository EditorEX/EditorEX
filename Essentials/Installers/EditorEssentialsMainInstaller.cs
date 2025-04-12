using EditorEX.Essentials.Features.HideUI;
using EditorEX.Essentials.Features.ViewMode;
using Zenject;

namespace EditorEX.Essentials.Installers
{
    public class EditorEssentialsMainInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<HideUIInputBinder>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ViewModeInputBinder>().AsSingle().NonLazy();
        }
    }
}
