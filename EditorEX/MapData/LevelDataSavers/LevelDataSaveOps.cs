using BeatmapEditor3D.DataModels;

namespace EditorEX.MapData.LevelDataSavers
{
    public static class LevelDataSaveOps
    {
        public static bool ShouldSkipSave(BeatmapProjectManager projectManager)
        {
            return !projectManager._beatmapDataModelsSaver.NeedsSaving()
                && !projectManager._bookmarkDataModelSaver.NeedsSaving();
        }

        public static bool BeatmapOrLightshowOrBookmarksNeedSaving(
            BeatmapProjectManager projectManager
        )
        {
            return projectManager._beatmapDataModelsSaver.BeatmapNeedSaving()
                || projectManager._beatmapDataModelsSaver.LightshowNeedsSaving()
                || projectManager._bookmarkDataModelSaver.NeedsSaving();
        }

        public static void ClearDifficultyDirty(BeatmapProjectManager projectManager)
        {
            projectManager._beatmapObjectsDataModel.ClearDirty();
            projectManager._beatmapBasicEventsDataModel.ClearDirty();
            projectManager._beatmapEventBoxGroupsDataModel.ClearDirty();
            projectManager._bookmarksDataModel.ClearDirty();
        }

        public static void BackupAndSaveTemp(BeatmapProjectManager projectManager, bool clearDirty)
        {
            if (!clearDirty)
            {
                return;
            }

            projectManager.BackupProject();
            projectManager.SaveTempProject();
        }
    }
}
