using System;
using BeatmapEditor3D.DataModels;

namespace EditorEX.MapData.SaveDataSavers
{
    public interface ICustomSaveDataSaver
    {
        public bool IsVersion(Version version);
        public void Save(BeatmapProjectManager beatmapProjectManager, bool clearDirty);
    }
}
