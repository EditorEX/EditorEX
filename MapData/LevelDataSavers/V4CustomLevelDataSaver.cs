using EditorEX.MapData.SaveDataLoaders;
using System;
using BeatmapEditor3D.DataModels;

namespace EditorEX.MapData.LevelDataSavers
{
    public class V4CustomLevelDataSaver : ICustomLevelDataSaver
    {
        public bool IsVersion(Version version)
        {
            return version.Major == 4;
        }

        public void Save(BeatmapProjectManager projectManager, bool clearDirty)
        {

        }
    }
}
