﻿using EditorEX.SDK.Collectors;
using EditorEX.SDK.Components;
using EditorEX.SDK.Factories;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.UI.ContextMenu;
using Zenject;

namespace EditorEX.SDK.Installers
{
    public class EditorSDKViewControllersInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ColorCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<TransitionCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<FontCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<PrefabCollector>().AsSingle();
            Container.BindInterfacesAndSelfTo<ReactiveContainer>().AsSingle();

            Container.BindInterfacesAndSelfTo<TextFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClickableTextFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ImageFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ButtonFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<IconButtonFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<StringInputFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<ModalFactory>().AsSingle();

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
