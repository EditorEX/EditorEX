using EditorEX.Chroma.Codecs;
using EditorEX.Chroma.EnvironmentEnhancement;
using Zenject;

namespace EditorEX.Chroma.Installers
{
    public class EditorChromaAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ChromaCustomDataCodec>().AsCached();

            Container
                .Bind<EditorEnvironmentMaterialsManager>()
                .FromNewComponentOnNewGameObject()
                .AsSingle();

            Container.Bind<EditorGeomtryShaderAddressables>().AsSingle();
        }
    }
}
