using EditorEX.Essentials.Features.HideUI;
using EditorEX.Essentials.Features.ViewMode;
using Zenject;

namespace EditorEX.Essentials.Installers
{
    public class EditorEssentialsFeatureInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<HideUIImplementation>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ViewModeImplementation>().AsSingle().NonLazy();
        }
    }
}
