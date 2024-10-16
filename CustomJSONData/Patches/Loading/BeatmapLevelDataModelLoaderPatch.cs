using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.CustomDataModels;
using EditorEX.MapData.Contexts;
using EditorEX.MapData.SaveDataLoaders;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace EditorEX.CustomJSONData.Patches.Loading
{
    internal class BeatmapLevelDataModelLoaderPatch : IAffinity
    {
        private readonly SiraLog _siraLog;
        private List<ICustomSaveDataLoader> _loaders;
        private LevelCustomDataModel _levelCustomDataModel;
        private SignalBus _signalBus;

        private BeatmapLevelDataModelLoaderPatch(
            SiraLog siraLog,
            List<ICustomSaveDataLoader> saveDataLoaders,
            LevelCustomDataModel levelCustomDataModel,
            SignalBus signalBus)
        {
            _siraLog = siraLog;
            _loaders = saveDataLoaders;
            _levelCustomDataModel = levelCustomDataModel;
            _signalBus = signalBus;
        }

        [AffinityPatch(typeof(BeatmapLevelDataModelLoader), nameof(BeatmapLevelDataModelLoader.Load))]
        [AffinityPrefix]
        public bool UniversalLoadingPatch(BeatmapLevelDataModelLoader __instance, string projectPath)
        {
            var version = BeatmapProjectFileHelper.GetVersionedJSONVersion(projectPath, "Info.dat");

            LevelContext.Reset();

            LevelContext.Version = version;

            var loader = _loaders.FirstOrDefault(x => x.IsVersion(version));
            if (loader == null)
            {
                _siraLog.Error("Could not find a viable save data loader ):");
                return false;
            }

            loader.Load(projectPath);
            _signalBus.Fire<BeatmapDataModelSignals.BeatmapUpdatedSignal>();

            return false;
        }
    }
}
