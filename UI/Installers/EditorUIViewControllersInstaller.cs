using EditorEX.SDK.AddressableHelpers;
using EditorEX.SDK.ContextMenu;
using EditorEX.SDK.Settings;
using EditorEX.SDK.ViewContent;
using EditorEX.SDKImplementation;
using EditorEX.UI.Cursor;
using EditorEX.UI.Patches;
using EditorEX.UI.Patches.SDK;
using UnityEngine;
using Zenject;

namespace EditorEX.UI.Installers
{
    public class EditorUIViewControllersInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<IViewContent<SettingsViewData>>().FromInstance(new EditorEXSettingsViewContent());

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
            Container.BindInterfacesAndSelfTo<SaveCustomDataPatch>().AsSingle();
            Container.BindInterfacesAndSelfTo<CursorUpdater>().AsSingle();
            Container.BindInterfacesAndSelfTo<BetterKeybindViewingPatches>().AsSingle();
            Container.BindInterfacesAndSelfTo<SelectableCellsCursorPatches>().AsSingle();

            Container.Bind<IContextMenuProvider>().To<DefaultEditorBeatmapListContextMenuProvider>().AsSingle();
            Container.Bind<IContextMenuProvider>().To<DefaultEditorSourceListContextMenuProvider>().AsSingle();
        }
    }
}
