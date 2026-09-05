using EditorEX.NoodleExtensions.Codecs;
using Zenject;

namespace EditorEX.NoodleExtensions.Installers
{
    public class EditorNoodleAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<NoodleCustomDataCodec>().AsCached();
        }
    }
}
