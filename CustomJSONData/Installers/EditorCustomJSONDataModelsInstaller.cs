using EditorEX.CustomJSONData;
using Zenject;

namespace EditorEX.Heck.Installers
{
    public class EditorCustomJSONDataModelsInstaller : Installer
    {
        public override void InstallBindings()
        {
            CustomDataRepository.ClearAll();
        }
    }
}
