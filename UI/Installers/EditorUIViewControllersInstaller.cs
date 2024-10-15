using EditorEX.SDK.Collectors;
using EditorEX.SDK.Components;
using EditorEX.SDK.ContextMenu;
using EditorEX.SDK.Factories;
using EditorEX.SDK.Settings;
using EditorEX.SDK.ViewContent;
using EditorEX.SDKImplementation;
using EditorEX.UI.ContextMenu;
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
            Container.BindInterfacesAndSelfTo<TransitionCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<FontCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<PrefabCollector>().AsSingle();

            Container.BindInterfacesAndSelfTo<TextFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClickableTextFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ImageFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ButtonFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<IconButtonFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ToggleFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<StringInputFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ModalFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<TextSegmentedControlFactory>().AsSingle();

            Container.Bind<IViewContent<SettingsViewData>>().FromInstance(new EditorExSettingsViewContent());

            Container.BindInterfacesAndSelfTo<DisableContextMenuPatches>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddSourceListContextMenu>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddBeatmapListContextMenu>().AsSingle();
            Container.BindInterfacesAndSelfTo<MoreCoverFileTypes>().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatmapsListViewControllerPatches>().AsSingle();
            Container.BindInterfacesAndSelfTo<EditDifficultyBeatmapPatches>().AsSingle();
            Container.BindInterfacesAndSelfTo<EditBeatmapLevelPatches>().AsSingle();
            Container.BindInterfacesAndSelfTo<MapFilteringPatches>().AsSingle();
            Container.BindInterfacesAndSelfTo<AddSettingsPatches>().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatmapsCollectionDataModelPatches>().AsSingle();
            Container.BindInterfacesAndSelfTo<SaveLevelAuthorNamePatch>().AsSingle();

            Container.Bind<IContextMenuProvider>().To<DefaultEditorBeatmapListContextMenuProvider>().AsSingle();
            Container.Bind<IContextMenuProvider>().To<DefaultEditorSourceListContextMenuProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<ContextMenuComponent>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<StringInputDialogModal>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

        }
    }
}
