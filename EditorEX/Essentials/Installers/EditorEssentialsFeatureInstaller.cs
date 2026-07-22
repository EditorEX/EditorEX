using EditorEX.Essentials.Features.HideUI;
using EditorEX.Essentials.Features.ViewMode;
using EditorEX.SDK.Collectors;
using EditorEX.SDK.ReactiveComponents;
using Zenject;

namespace EditorEX.Essentials.Installers
{
    public class EditorEssentialsFeatureInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<HideUIImplementation>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ViewModeImplementation>().AsSingle().NonLazy();

            // Toast needs ReactiveContainer; collectors live in the view-controllers
            // context. Bind locally only if this container cannot already resolve them.
            if (!Container.HasBinding<IColorCollector>())
            {
                Container.BindInterfacesAndSelfTo<ColorCollector>().AsSingle();
            }

            if (!Container.HasBinding<IFontCollector>())
            {
                Container.BindInterfacesAndSelfTo<FontCollector>().AsSingle();
            }

            if (!Container.HasBinding<ITransitionCollector>())
            {
                Container.BindInterfacesAndSelfTo<TransitionCollector>().AsSingle();
            }

            if (!Container.HasBinding<IReactiveContainer>())
            {
                Container.BindInterfacesAndSelfTo<ReactiveContainer>().AsSingle();
            }

            Container
                .BindInterfacesAndSelfTo<ViewModeToastHost>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();
        }
    }
}
