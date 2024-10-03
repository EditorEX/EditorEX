using EditorEX.SDK.Collectors;
using EditorEX.SDK.Factories;
using EditorEX.SDK.Settings;
using EditorEX.SDK.ViewContent;
using EditorEX.SDKImplementation;
using EditorEX.UI.Patches;
using EditorEX.UI.Patches.SDK;
using Zenject;

namespace EditorEX.UI.Installers
{
    public class EditorUIViewControllersInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ColorCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<FontCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<PrefabCollector>().AsSingle();

            Container.BindInterfacesAndSelfTo<TextFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ImageFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ButtonFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<IconButtonFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ToggleFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<StringInputFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<TextSegmentedControlFactory>().AsSingle();

            Container.Bind<IViewContent<SettingsViewData>>().FromInstance(new EditorExSettingsViewContent());

            Container.BindInterfacesAndSelfTo<MoreCoverFileTypes>().AsSingle();
            Container.BindInterfacesAndSelfTo<EditDifficultyBeatmapPatches>().AsSingle();
            Container.BindInterfacesAndSelfTo<EditBeatmapLevelPatches>().AsSingle();
            Container.BindInterfacesAndSelfTo<MapFilteringPatches>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddSettingsPatches>().AsSingle();
        }
    }
}
