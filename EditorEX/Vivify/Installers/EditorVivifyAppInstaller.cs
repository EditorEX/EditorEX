using EditorEX.Vivify.Codecs;
using Zenject;

namespace EditorEX.Vivify.Installers
{
    public class EditorVivifyAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<VivifyCustomDataCodec>().AsSingle();
        }
    }
}
