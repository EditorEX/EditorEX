using EditorEX.Heck.Deserialize;
using Heck;
using Zenject;

namespace EditorEX.Heck.Installers
{
    public class EditorHeckModelsInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<bool>().WithId(HeckController.LEFT_HANDED_ID).FromInstance(false);

            Container.Bind<EditorDeserializerManager>().AsSingle();
        }
    }
}
