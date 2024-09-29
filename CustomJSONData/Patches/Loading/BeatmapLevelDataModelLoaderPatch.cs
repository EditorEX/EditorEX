using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.MapData.Contexts;
using EditorEX.MapData.SaveDataLoaders;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace EditorEX.CustomJSONData.Patches.Loading
{
    public class BeatmapLevelDataModelLoaderPatch : IAffinity
    {
        private readonly SiraLog _siraLog;
        private List<ICustomSaveDataLoader> _loaders;
        private SignalBus _signalBus;

        [Inject]
        public BeatmapLevelDataModelLoaderPatch(
            SiraLog siraLog,
            List<ICustomSaveDataLoader> saveDataLoaders,
            SignalBus signalBus)
        {
            _siraLog = siraLog;
            _loaders = saveDataLoaders;
            _signalBus = signalBus;
        }

        [AffinityPatch(typeof(BeatmapLevelDataModelLoader), nameof(BeatmapLevelDataModelLoader.Load))]
        [AffinityPrefix]
        public bool UniversalLoadingPatch(BeatmapLevelDataModelLoader __instance, string projectPath)
        {
            var version = BeatmapProjectFileHelper.GetVersionedJSONVersion(projectPath, "Info.dat");

            LevelContext.Reset();

            LevelContext.Version = version;

            if (version >= BeatmapProjectFileHelper.version400)
            {
                return true;
            }

            var loader = _loaders.FirstOrDefault(x => x.IsVersion(version));
            if (loader == null)
            {
                _siraLog.Error("Could not find a viable save data loader ):");
            }

            loader.Load(projectPath);
            _signalBus.Fire<BeatmapDataModelSignals.BeatmapUpdatedSignal>();

            return false;
        }
    }
}
