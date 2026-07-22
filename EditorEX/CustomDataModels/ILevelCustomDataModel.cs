using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;

namespace EditorEX.CustomDataModels
{
    /// <summary>
    /// Holds data exclusive to V2 and V3 levels and also custom data for the level.
    /// </summary>
    public interface ILevelCustomDataModel
    {
        string? LevelAuthorName { get; }
        string? AllDirectionsEnvironmentName { get; }
        string? EnvironmentName { get; }
        float Shuffle { get; }
        float ShufflePeriod { get; }
        CustomData? LevelCustomData { get; }
        Dictionary<string, CustomData>? BeatmapCustomDatasByFilename { get; }
        CustomPlatformsListModel.CustomPlatformInfo? CustomPlatformInfo { get; }
        Dictionary<string, CharacteristicDetailsData> CharacteristicDetailsByName { get; }
        Dictionary<string, CustomData> CharacteristicCustomDataByName { get; set; }

        void UpdateWith(
            string? levelAuthorName = null,
            string? allDirectionsEnvironmentName = null,
            string? environmentName = null,
            float? shuffle = null,
            float? shufflePeriod = null,
            CustomData? levelCustomData = null,
            Dictionary<string, CustomData>? beatmapCustomDatasByFilename = null,
            List<ContributorData>? contributors = null,
            CustomPlatformsListModel.CustomPlatformInfo? customPlatformInfo = null,
            Dictionary<string, CharacteristicDetailsData>? characteristicDetailsByName = null
        );
    }
}
