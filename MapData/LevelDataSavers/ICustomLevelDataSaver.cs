using System;
using BeatmapEditor3D.DataModels;

namespace EditorEX.MapData.SaveDataLoaders
{
    public interface ICustomLevelDataSaver
    {
        public bool IsVersion(Version version);
        public void Save(BeatmapProjectManager projectManager, bool clearDirty);
    }
}
