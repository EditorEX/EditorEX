using Zenject;
using EditorEX.Essentials.Features.HideUI;

namespace EditorEX.Essentials.Installers
{
    public class EditorEssentialsFeatureInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<HideUIImplementation>().AsSingle().NonLazy();
        }
    }
}
