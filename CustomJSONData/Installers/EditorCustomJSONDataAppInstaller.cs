using EditorEX.CustomJSONData.Patches;
using Zenject;

namespace EditorEX.CustomJSONData.Installers
{
    public class EditorCustomJSONDataAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PreviewCustomBeatmap>().AsSingle().NonLazy();
        }
    }
}
