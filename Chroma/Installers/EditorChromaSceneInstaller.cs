using BetterEditor.Chroma.Colorizer;
using BetterEditor.Chroma.EnvironmentEnhancement;
using BetterEditor.Chroma.Events;
using BetterEditor.Chroma.Lighting;
using BetterEditor.Chroma.Patches;
using BetterEditor.Chroma.Patches.Events;
using Chroma;
using Chroma.EnvironmentEnhancement;
using Chroma.EnvironmentEnhancement.Component;
using Chroma.HarmonyPatches.Colorizer.Initialize;
using Chroma.HarmonyPatches.EnvironmentComponent;
using Chroma.Lighting;
using Heck;
using Zenject;

namespace BetterEditor.Chroma.Installers
{
    public class EditorChromaSceneInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<EditorAnimateComponent>().AsSingle();
            Container.BindInterfacesTo<EditorFogAnimatorV2>().AsSingle();

            Container.Bind<EditorLightColorizerManager>().AsSingle();
            Container.BindFactory<EditorChromaLightSwitchEventEffect, EditorLightColorizer, EditorLightColorizer.Factory>().AsSingle();
            Container.BindFactory<LightSwitchEventEffect, EditorChromaLightSwitchEventEffect, EditorChromaLightSwitchEventEffect.Factory>()
                .FromFactory<DisposableClassFactory<LightSwitchEventEffect, EditorChromaLightSwitchEventEffect>>();

            Container.BindInterfacesAndSelfTo<LightIDTableManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EditorLightWithIdRegisterer>().AsSingle();
            Container.BindInterfacesTo<EditorLightColorizerInitialize>().AsSingle();

            Container.BindInterfacesAndSelfTo<EditorChromaGradientController>().AsSingle();

            Container.BindInterfacesTo<EditorLightPairRotationChromafier>().AsSingle();
            Container.BindInterfacesTo<EditorLightRotationChromafier>().AsSingle();
            Container.BindInterfacesTo<EditorRingRotationChromafier>().AsSingle();
            Container.BindInterfacesTo<EditorRingStepChromafier>().AsSingle();
            Container.Bind<ChromaRingsRotationEffect.Factory>().AsSingle();

            Container.Bind<EditorDuplicateInitializer>().AsSingle();
            Container.Bind<EditorEnvironmentEnhancementManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<EditorComponentCustomizer>().AsSingle();
            Container.Bind<EditorGeometryFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<EditorMaterialsManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<MaterialColorAnimator>().AsSingle();
            Container.Bind<EditorILightWithIdCustomizer>().AsSingle();

            Container.BindInterfacesAndSelfTo<ParametricBoxControllerTransformOverride>().AsSingle();
            Container.BindInterfacesAndSelfTo<TrackLaneRingOffset>().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatmapObjectsAvoidanceTransformOverride>().AsSingle();

            Container.BindInterfacesTo<ColorSchemeGetter>().AsSingle();
        }
    }
}
