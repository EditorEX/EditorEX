using EditorEX.Essentials.Patches;
using EditorEX.Essentials.SpawnProcessing;
using Zenject;

namespace EditorEX.Essentials.Installers
{
    public class EditorEssentialsModelsInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<FixSongTimeTweening>().AsSingle().NonLazy();
            EditorSpawnDataRepository.ClearAll();
        }
    }
}
