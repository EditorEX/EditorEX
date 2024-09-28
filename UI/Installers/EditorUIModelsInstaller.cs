using EditorEX.Essentials.SpawnProcessing;
using EditorEX.UI.Collectors;
using EditorEX.UI.Factories;
using EditorEX.UI.Patches;
using Zenject;

namespace EditorEX.UI.Installers
{
    public class EditorUIModelsInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ColorCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<FontCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<PrefabCollector>().AsSingle();

            Container.BindInterfacesAndSelfTo<TextFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ButtonFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<IconButtonFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ToggleFactory>().AsSingle();

            Container.BindInterfacesAndSelfTo<MoreCoverFileTypes>().AsSingle();
            Container.BindInterfacesAndSelfTo<EditDifficultyBeatmapPatches>().AsSingle();
        }
    }
}
