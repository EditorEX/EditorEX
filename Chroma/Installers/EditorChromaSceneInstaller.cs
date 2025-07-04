﻿using Chroma.EnvironmentEnhancement;
using Chroma.HarmonyPatches.EnvironmentComponent;
using Chroma.Lighting;
using EditorEX.Chroma.Colorizer;
using EditorEX.Chroma.EnvironmentEnhancement;
using EditorEX.Chroma.EnvironmentEnhancement.Component;
using EditorEX.Chroma.Events;
using EditorEX.Chroma.HarmonyPatches.Colorizer.Initialize;
using EditorEX.Chroma.Lighting;
using EditorEX.Chroma.Patches.Events;
using EditorEX.MapData.Contexts;
using Heck;
using Zenject;

namespace EditorEX.Chroma.Installers
{
    public class EditorChromaSceneInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (MapContext.Version.Major < 4)
            {
                Container.BindInterfacesTo<EditorAnimateComponent>().AsSingle();
                Container.BindInterfacesTo<EditorFogAnimatorV2>().AsSingle();

                Container.Bind<EditorLightColorizerManager>().AsSingle();
                Container
                    .BindFactory<
                        EditorChromaLightSwitchEventEffect,
                        EditorLightColorizer,
                        EditorLightColorizer.Factory
                    >()
                    .AsSingle();
                Container
                    .BindFactory<
                        LightSwitchEventEffect,
                        EditorChromaLightSwitchEventEffect,
                        EditorChromaLightSwitchEventEffect.Factory
                    >()
                    .FromFactory<
                        DisposableClassFactory<
                            LightSwitchEventEffect,
                            EditorChromaLightSwitchEventEffect
                        >
                    >();

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
                Container
                    .Bind<EditorEnvironmentEnhancementManager>()
                    .FromNewComponentOnNewGameObject()
                    .AsSingle()
                    .NonLazy();
                Container.Bind<EditorComponentCustomizer>().AsSingle();
                Container.Bind<EditorGeometryFactory>().AsSingle();
                Container.BindInterfacesAndSelfTo<EditorMaterialsManager>().AsSingle();
                Container.BindInterfacesAndSelfTo<MaterialColorAnimator>().AsSingle();
                Container.Bind<EditorILightWithIdCustomizer>().AsSingle();

                Container
                    .BindInterfacesAndSelfTo<ParametricBoxControllerTransformOverride>()
                    .AsSingle();
                Container.BindInterfacesAndSelfTo<TrackLaneRingOffset>().AsSingle();
            }
        }
    }
}
