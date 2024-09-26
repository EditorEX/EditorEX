using EditorEX.CustomJSONData.Patches;
using EditorEX.CustomJSONData.Patches.Loading;
using EditorEX.MapData.LevelDataLoaders;
using EditorEX.MapData.SaveDataLoaders;
using Zenject;

namespace EditorEX.CustomJSONData.Installers
{
    public class EditorCustomJSONDataModelsInstaller : Installer
    {
        public override void InstallBindings()
        {
            Plugin.Log.Info("fff");
            Container.Bind<ICustomSaveDataLoader>().To<V2CustomSaveDataLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatmapLevelDataModelLoaderPatch>().AsSingle().NonLazy();

            Container.Bind<V2BeatmapBpmDataVersionedLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<AudioDataLoaderPatch>().AsSingle().NonLazy();

            Container.Bind<LevelDataLoaderV2>().AsSingle();
            Container.Bind<LevelDataLoaderV3>().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatmapDataModelsLoaderPatch>().AsSingle().NonLazy();


            Container.BindInterfacesAndSelfTo<DisableConversion>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<InjectCustomEvents>().AsSingle().NonLazy();
        }
    }
}
