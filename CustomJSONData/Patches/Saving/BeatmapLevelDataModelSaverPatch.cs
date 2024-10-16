using BeatmapEditor3D.DataModels;
using EditorEX.CustomDataModels;
using EditorEX.MapData.Contexts;
using EditorEX.MapData.SaveDataSavers;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System.Collections.Generic;
using System.Linq;

namespace EditorEX.CustomJSONData.Patches.Saving
{
    internal class BeatmapLevelDataModelSaverPatch : IAffinity
    {
        private readonly SiraLog _siraLog;
        private readonly List<ICustomSaveDataSaver> _savers;
        private readonly LevelCustomDataModel _levelCustomDataModel;

        private BeatmapLevelDataModelSaverPatch(
            SiraLog siraLog,
            List<ICustomSaveDataSaver> saveDataSavers,
            LevelCustomDataModel levelCustomDataModel)
        {
            _siraLog = siraLog;
            _siraLog.Info("fddfdf");
            _savers = saveDataSavers;
            _levelCustomDataModel = levelCustomDataModel;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapProjectManager), nameof(BeatmapProjectManager.SaveBeatmapProject))]
        private bool UniversalSavingPatch(BeatmapProjectManager __instance, bool clearDirty)
        {
            _siraLog.Info("oudishhosi");
            var version = LevelContext.Version;

            var saver = _savers.FirstOrDefault(x => x.IsVersion(version));
            if (saver == null)
            {
                _siraLog.Error("Could not find a viable save data saver ):");
            }

            saver.Save(__instance, clearDirty);

            return false;
        }
    }
}
