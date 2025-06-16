using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using EditorEX.CustomDataModels;
using EditorEX.MapData.Contexts;
using EditorEX.MapData.SaveDataLoaders;
using EditorEX.MapData.SaveDataSavers;
using SiraUtil.Affinity;
using SiraUtil.Logging;

namespace EditorEX.CustomJSONData.Patches.Saving
{
    internal class BeatmapProjectManagerSaverPatch : IAffinity
    {
        private readonly SiraLog _siraLog;
        private readonly List<ICustomSaveDataSaver> _saveDataSavers;
        private readonly List<ICustomLevelDataSaver> _levelDataSavers;
        private readonly LevelCustomDataModel _levelCustomDataModel;

        private BeatmapProjectManagerSaverPatch(
            SiraLog siraLog,
            List<ICustomSaveDataSaver> saveDataSavers,
            List<ICustomLevelDataSaver> levelDataSavers,
            LevelCustomDataModel levelCustomDataModel
        )
        {
            _siraLog = siraLog;
            _saveDataSavers = saveDataSavers;
            _levelDataSavers = levelDataSavers;
            _levelCustomDataModel = levelCustomDataModel;
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(BeatmapProjectManager),
            nameof(BeatmapProjectManager.SaveBeatmapProject)
        )]
        private bool UniversalSaveDataSavingPatch(BeatmapProjectManager __instance, bool clearDirty)
        {
            var version = LevelContext.Version;

            var saver = _saveDataSavers.FirstOrDefault(x => x.IsVersion(version));
            if (saver == null)
            {
                _siraLog.Error("Could not find a viable save data saver!");
            }

            saver.Save(__instance, clearDirty);

            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(BeatmapProjectManager),
            nameof(BeatmapProjectManager.SaveBeatmapLevel)
        )]
        private bool UniversalLevelDataSavingPatch(
            BeatmapProjectManager __instance,
            bool clearDirty
        )
        {
            var version = MapContext.Version;

            _siraLog.Info($"Saving level data with version {version}");

            var saver = _levelDataSavers.FirstOrDefault(x => x.IsVersion(version));
            if (saver == null)
            {
                _siraLog.Error("Could not find a viable level data saver!");
            }

            saver.Save(__instance, __instance._beatmapDataModel.difficultyBeatmapData, clearDirty);

            return false;
        }
    }
}
