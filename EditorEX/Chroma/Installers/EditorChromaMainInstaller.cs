using Chroma.EnvironmentEnhancement;
using EditorEX.Chroma.Patches;
using Zenject;

namespace EditorEX.Chroma.Installers
{
    public class EditorChromaMainInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<EnvironmentMaterialsManager.EnvironmentMaterialsManagerInitializer>()
                .AsSingle();

            Container.BindInterfacesTo<InjectCustomDataIntoLivePreview>().AsSingle().NonLazy();
        }
    }
}
