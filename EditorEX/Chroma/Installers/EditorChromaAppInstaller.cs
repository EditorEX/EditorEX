using EditorEX.Chroma.Codecs;
using Zenject;

namespace EditorEX.Chroma.Installers
{
    public class EditorChromaAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ChromaCustomDataCodec>().AsSingle();
        }
    }
}
