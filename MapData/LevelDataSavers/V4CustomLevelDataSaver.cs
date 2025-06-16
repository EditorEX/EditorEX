using System;
using BeatmapEditor3D.DataModels;
using EditorEX.MapData.SaveDataLoaders;

namespace EditorEX.MapData.LevelDataSavers
{
    public class V4CustomLevelDataSaver : ICustomLevelDataSaver
    {
        public bool IsVersion(Version version)
        {
            return version.Major == 4;
        }

        public void Save(
            BeatmapProjectManager projectManager,
            DifficultyBeatmapData difficultyBeatmapData,
            bool clearDirty
        )
        {
            if (
                !projectManager._beatmapDataModelsSaver.NeedsSaving()
                && !projectManager._bookmarkDataModelSaver.NeedsSaving()
            )
            {
                return;
            }
            if (projectManager._bookmarkDataModelSaver.NeedsSaving())
            {
                BeatmapProjectFileHelper.CreateBookmarkSubdirectoryIfNotExists(
                    projectManager._workingBeatmapProject
                );
                var list = projectManager._bookmarkDataModelSaver.Save();
                foreach (var valueTuple in list)
                {
                    string item = valueTuple.Item1;
                    var item2 = valueTuple.Item2;
                    if (
                        item != null
                        && BeatmapProjectFileHelper.BookmarkFilenameChanged(item, item2)
                    )
                    {
                        BeatmapProjectFileHelper.TryDeleteBookmarkSet(
                            projectManager._workingBeatmapProject,
                            item
                        );
                    }
                }
                foreach (var valueTuple2 in list)
                {
                    string item3 = valueTuple2.Item1;
                    var item4 = valueTuple2.Item2;
                    BeatmapProjectFileHelper.SaveBookmarkSet(
                        projectManager._workingBeatmapProject,
                        item4
                    );
                    string bookmarkSetFilename = BeatmapProjectFileHelper.GetBookmarkSetFilename(
                        item4
                    );
                    projectManager._bookmarksDataModel.UpdateBookmarkSetFileName(
                        item3,
                        bookmarkSetFilename
                    );
                }
                if (clearDirty)
                {
                    projectManager._bookmarksDataModel.ClearDirty();
                }
            }
            if (projectManager._beatmapDataModelsSaver.BeatmapNeedSaving())
            {
                var beatmapSaveData = projectManager._beatmapDataModelsSaver.SaveBeatmapObjects();
                BeatmapProjectFileHelper.SaveBeatmap(
                    projectManager._workingBeatmapProject,
                    difficultyBeatmapData.beatmapFilename,
                    beatmapSaveData,
                    true
                );
                if (clearDirty)
                {
                    projectManager._beatmapObjectsDataModel.ClearDirty();
                }
            }
            if (projectManager._beatmapDataModelsSaver.LightshowNeedsSaving())
            {
                var lightshowSaveData = projectManager._beatmapDataModelsSaver.SaveLightshow();
                BeatmapProjectFileHelper.SaveLightshow(
                    projectManager._workingBeatmapProject,
                    difficultyBeatmapData.lightshowFilename,
                    lightshowSaveData,
                    false
                );
                if (clearDirty)
                {
                    projectManager._beatmapBasicEventsDataModel.ClearDirty();
                    projectManager._beatmapEventBoxGroupsDataModel.ClearDirty();
                }
            }
            if (clearDirty)
            {
                projectManager.BackupProject();
                projectManager.SaveTempProject();
            }
        }
    }
}
