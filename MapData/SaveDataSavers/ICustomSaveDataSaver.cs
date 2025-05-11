using BeatmapEditor3D.DataModels;
using System;

namespace EditorEX.MapData.SaveDataSavers
{
    public interface ICustomSaveDataSaver
    {
        public bool IsVersion(Version version);
        public void Save(BeatmapProjectManager beatmapProjectManager, bool clearDirty);
    }
}
