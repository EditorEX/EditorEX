using EditorEX.Heck.Deserialize;
using EditorEX.Heck.Events;
using EditorEX.Heck.Patches;
using HarmonyLib;
using Heck;
using Heck.Animation;
using Heck.Animation.Transform;
using Heck.Event;
using System.Collections.Generic;
using Zenject;

namespace EditorEX.Heck.Installers
{
    public class EditorHeckSceneInstaller : Installer
    {
        public override void InstallBindings()
        {
            var beatmapTracks = EditorDeserializedDataContainer.Tracks;
            var deserializedDatas = EditorDeserializedDataContainer.DeserializeDatas;

            Container.Bind<Dictionary<string, Track>>().FromInstance(beatmapTracks).AsSingle();
            deserializedDatas.Do(x => Container.BindInstance(x.Value).WithId(x.Key));
            Container.BindInstance(deserializedDatas);

            Container.Bind<bool>().WithId(HeckController.LEFT_HANDED_ID).FromInstance(false);

            Container.Bind<CoroutineDummy>().FromNewComponentOnRoot().AsSingle();
            Container.BindInterfacesTo<CustomEventController>().AsSingle();
            Container.BindInterfacesTo<EditorCoroutineEvent>().AsSingle();
            Container.BindInterfacesAndSelfTo<TransformControllerFactory>().AsSingle();
            Container.BindInterfacesTo<TrackUpdateManager>().AsSingle();

            Container.BindInterfacesTo<EditorGameObjectTracker>().AsSingle().NonLazy();
        }
    }
}
