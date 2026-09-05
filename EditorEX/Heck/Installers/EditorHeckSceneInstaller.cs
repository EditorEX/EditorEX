using EditorEX.Heck.Events;
using EditorEX.Heck.Patches;
using Heck;
using Heck.Animation;
using Heck.Animation.Transform;
using Heck.BaseProvider;
using Heck.BaseProviders;
using Heck.Event;
using Heck.HarmonyPatches;
using Heck.ObjectInitialize;
using Zenject;

namespace EditorEX.Heck.Installers
{
    public class EditorHeckSceneInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<bool>().WithId(HeckController.LEFT_HANDED_ID).FromInstance(false);

            Container.Bind<CoroutineDummy>().FromNewComponentOnRoot().AsSingle();
            Container.BindInterfacesTo<CustomEventController>().AsSingle();
            Container.BindInterfacesTo<HeckTrackPreviewSource>().AsSingle();
            Container.BindInterfacesAndSelfTo<TransformControllerFactory>().AsSingle();
            Container.BindInterfacesTo<TrackUpdateManager>().AsSingle();

            Container.Bind<ObjectInitializerManager>().AsSingle();

            Container.BindInterfacesAndSelfTo<SiraUtilHeadFinder>().AsSingle();

            Container.BindInterfacesTo<EditorGameObjectTracker>().AsSingle().NonLazy();

            Container.BindInterfacesTo<GameBaseProviderDisposer>().AsSingle();

            Container.BindInterfacesTo<PlayerTransformGetter>().AsSingle();
            Container.BindInterfacesTo<ColorSchemeGetter>().AsSingle();
            //container.BindInterfacesTo<ScoreGetter>().AsSingle();
        }
    }
}
