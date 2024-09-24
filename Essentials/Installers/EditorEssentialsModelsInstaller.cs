using BetterEditor.Essentials.SpawnProcessing;
using Zenject;

namespace BetterEditor.Essentials.Installers
{
    public class EditorEssentialsModelsInstaller : Installer
    {
        public override void InstallBindings()
        {
            EditorSpawnDataRepository.ClearAll();


        }
    }
}
