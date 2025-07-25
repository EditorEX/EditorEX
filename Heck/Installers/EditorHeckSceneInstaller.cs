﻿using System.Collections.Generic;
using EditorEX.Heck.Deserialize;
using EditorEX.Heck.Events;
using EditorEX.Heck.Patches;
using HarmonyLib;
using Heck;
using Heck.Animation;
using Heck.Animation.Transform;
using Heck.BaseProvider;
using Heck.BaseProviders;
using Heck.Event;
using Heck.HarmonyPatches;
using Heck.ObjectInitialize;
using SiraUtil.Logging;
using Zenject;

namespace EditorEX.Heck.Installers
{
    public class EditorHeckSceneInstaller : Installer
    {
        [Inject]
        private SiraLog? _siraLog = null!;

        public override void InstallBindings()
        {
            var beatmapTracks =
                EditorDeserializedDataContainer.Tracks ?? new Dictionary<string, Track>();
            var deserializedDatas = EditorDeserializedDataContainer.DeserializeDatas;
            _siraLog.Info($"Deserialized {deserializedDatas.Count} custom data objects.");

            Container.Bind<Dictionary<string, Track>>().FromInstance(beatmapTracks).AsSingle();
            deserializedDatas.Do(x => Container.BindInstance(x.Value).WithId(x.Key));
            Container.BindInstance(deserializedDatas);

            Container.Bind<bool>().WithId(HeckController.LEFT_HANDED_ID).FromInstance(false);

            Container.Bind<CoroutineDummy>().FromNewComponentOnRoot().AsSingle();
            Container.BindInterfacesTo<CustomEventController>().AsSingle();
            Container.BindInterfacesTo<EditorCoroutineEvent>().AsSingle();
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
