using BetterEditor.Heck.Deserializer;
using BetterEditor.Heck.Patches;
using Heck;
using Zenject;

namespace BetterEditor.Heck.Installers
{
	public class EditorHeckModelsInstaller : Installer
	{
		public override void InstallBindings()
		{
			Container.Bind<bool>().WithId(HeckController.LEFT_HANDED_ID).FromInstance(false);

			Container.Bind<EditorDeserializerManager>().AsSingle();
			Container.BindInterfacesAndSelfTo<DeserializationPatch>().AsSingle().NonLazy();
		}
	}
}
