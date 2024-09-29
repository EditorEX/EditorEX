using EditorEX.Essentials.SpawnProcessing;
using Zenject;

namespace EditorEX.Essentials.Installers
{
    public class EditorEssentialsModelsInstaller : Installer
    {
        public override void InstallBindings()
        {
            EditorSpawnDataRepository.ClearAll();
        }
    }
}
