using EditorEX.CustomDataModels;
using EditorEX.CustomJSONData.Patches;
using EditorEX.CustomJSONData.Patches.Loading;
using EditorEX.CustomJSONData.Patches.Saving;
using EditorEX.MapData.LevelDataLoaders;
using EditorEX.MapData.LevelDataSavers;
using EditorEX.MapData.SaveDataLoaders;
using EditorEX.MapData.SaveDataSavers;
using Zenject;

namespace EditorEX.CustomJSONData.Installers
{
    public class EditorCustomJSONDataModelsInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<ICustomSaveDataLoader>().To<V2CustomSaveDataLoader>().AsSingle();
            Container.Bind<ICustomSaveDataLoader>().To<V4CustomSaveDataLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatmapLevelDataModelLoaderPatch>().AsSingle().NonLazy();

            Container.Bind<ICustomSaveDataSaver>().To<V4CustomSaveDataSaver>().AsSingle().NonLazy();
            Container.Bind<ICustomSaveDataSaver>().To<V2CustomSaveDataSaver>().AsSingle().NonLazy();

            Container.Bind<ICustomLevelDataSaver>().To<V4CustomLevelDataSaver>().AsSingle();
            Container.Bind<ICustomLevelDataSaver>().To<V3CustomLevelDataSaver>().AsSingle();
            Container.Bind<ICustomLevelDataSaver>().To<V2CustomLevelDataSaver>().AsSingle();

            Container.BindInterfacesAndSelfTo<BeatmapProjectManagerSaverPatch>().AsSingle().NonLazy();

            Container.Bind<V2BeatmapBpmDataVersionedLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<AudioDataLoaderPatch>().AsSingle().NonLazy();

            Container.Bind<LevelDataLoaderV2>().AsSingle();
            Container.Bind<LevelDataLoaderV3>().AsSingle();
            Container.BindInterfacesAndSelfTo<BeatmapDataModelsLoaderPatch>().AsSingle().NonLazy();

            Container.Bind<LevelCustomDataModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<CustomPlatformsListModel>().AsSingle();

            Container.BindInterfacesAndSelfTo<DisableConversion>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<InjectCustomEvents>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<BetterClearEvents>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<BeatmapFileUtilsPatch>().AsSingle().NonLazy();
        }
    }
}
