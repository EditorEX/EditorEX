using System;
using BeatmapEditor3D.SerializedData;

namespace EditorEX.MapData.LevelDataLoaders
{
    public interface ICustomLevelDataLoader
    {
        bool IsVersion(Version version);

        DifficultyLoadResult Load(
            BeatmapDataModelsLoader loader,
            string projectPath,
            string beatmapFilename,
            string? lightshowFilename
        );
    }
}
