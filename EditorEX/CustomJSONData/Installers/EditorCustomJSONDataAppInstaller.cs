using EditorEX.CustomJSONData.Patches;
using Zenject;

namespace EditorEX.CustomJSONData.Installers
{
    public class EditorCustomJSONDataAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            // App-scoped so BeatmapEditor3D and GameCore (siblings) share one instance.
            // Scene-bound Chroma/Vivify/Heck consumers cannot see BeatmapEditor3D-only binds.
            Container.BindInterfacesAndSelfTo<CustomDataRepository>().AsSingle();

            Container.BindInterfacesAndSelfTo<PreviewCustomBeatmap>().AsSingle().NonLazy();
        }
    }
}
