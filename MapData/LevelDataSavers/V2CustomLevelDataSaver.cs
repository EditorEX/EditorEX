using System;
using BeatmapEditor3D.DataModels;
using EditorEX.MapData.SaveDataLoaders;

namespace EditorEX.MapData.LevelDataSavers
{
    public class V2CustomLevelDataSaver : ICustomLevelDataSaver
    {
        public bool IsVersion(Version version)
        {
            return version.Major == 2;
        }

        public void Save(BeatmapProjectManager projectManager, bool clearDirty)
        {

        }

    }
}
