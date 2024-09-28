using EditorEX.Essentials.SpawnProcessing;
using EditorEX.UI.Patches;
using Zenject;

namespace EditorEX.UI.Installers
{
    public class EditorUIModelsInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MoreCoverFileTypes>().AsSingle();
        }
    }
}
