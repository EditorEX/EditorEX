using Zenject;
using EditorEX.Essentials.Features.HideUI;
using EditorEX.Essentials.Features.ViewMode;

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
