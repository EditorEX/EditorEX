using EditorEX.SDK.Collectors;
using EditorEX.SDK.Components;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.UI.Patches;
using Zenject;

namespace EditorEX.SDK.Installers
{
    public class EditorSDKViewControllersInstaller : Installer
    {
        public override void InstallBindings()
        {
            //Container.BindInterfacesAndSelfTo<DisableContextMenuPatches>().AsSingle();

            Container.BindInterfacesAndSelfTo<ColorCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<TransitionCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<FontCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<PrefabCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<ReactiveContainer>().AsSingle();

            Container
                .BindInterfacesAndSelfTo<ContextMenuComponent>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<StringInputDialogModal>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();
        }
    }
}
