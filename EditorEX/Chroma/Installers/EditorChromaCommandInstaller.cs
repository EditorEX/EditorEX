using EditorEX.Chroma.EnvironmentEnhancement;
using Zenject;

namespace EditorEX.Chroma.Installers
{
    public class EditorChromaCommandInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<EditorGeomtryShaderAddressablesBinder>().AsSingle().NonLazy();
        }
    }
}
