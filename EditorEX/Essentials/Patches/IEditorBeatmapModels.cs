using BeatmapEditor3D.DataModels;

namespace EditorEX.Essentials.Patches
{
    public interface IEditorBeatmapModels
    {
        AudioDataModel AudioDataModel { get; }

        BeatmapLevelDataModel BeatmapLevelDataModel { get; }

        BeatmapObjectsDataModel BeatmapObjectsDataModel { get; }

        BeatmapDataModel BeatmapDataModel { get; }
    }
}
