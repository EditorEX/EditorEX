using System;
using BeatmapEditor3D.DataModels;

namespace EditorEX.MapData.LevelDataSavers
{
    public interface ICustomLevelDataSaver
    {
        public bool IsVersion(Version version);
        public void Save(
            BeatmapProjectManager projectManager,
            DifficultyBeatmapData difficultyBeatmapData,
            bool clearDirty
        );
    }
}
