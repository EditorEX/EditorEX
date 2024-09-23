using BetterEditor.CustomJSONData;
using Zenject;

namespace BetterEditor.Heck.Installers
{
	public class EditorCustomJSONDataModelsInstaller : Installer
	{
		public override void InstallBindings()
		{
			CustomDataRepository.ClearAll();
		}
	}
}
