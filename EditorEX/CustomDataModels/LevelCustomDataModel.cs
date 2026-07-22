using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using EditorEX.MapData.Contexts;

namespace EditorEX.CustomDataModels
{
    /// <summary>
    /// Holds data exclusive to V2 and V3 levels and also custom data for the level
    /// </summary>
    public class LevelCustomDataModel : ILevelCustomDataModel
    {
        public string? LevelAuthorName { get; set; }
        public string? AllDirectionsEnvironmentName { get; set; }
        public string? EnvironmentName { get; set; }
        public float Shuffle { get; set; }
        public float ShufflePeriod { get; set; }
        public CustomData? LevelCustomData { get; set; }
        public Dictionary<string, CustomData>? BeatmapCustomDatasByFilename { get; set; }
        public List<ContributorData>? Contributors { get; set; }
        public CustomPlatformsListModel.CustomPlatformInfo? CustomPlatformInfo { get; set; }
        public Dictionary<
            string,
            CharacteristicDetailsData
        > CharacteristicDetailsByName { get; set; } =
            new Dictionary<string, CharacteristicDetailsData>();

        public Dictionary<string, CustomData> CharacteristicCustomDataByName { get; set; } =
            new Dictionary<string, CustomData>();

        public void UpdateWith(
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
        )
        {
            LevelAuthorName = levelAuthorName ?? LevelAuthorName;
            AllDirectionsEnvironmentName =
                allDirectionsEnvironmentName ?? AllDirectionsEnvironmentName;
            EnvironmentName = environmentName ?? EnvironmentName;
            Shuffle = shuffle ?? Shuffle;
            ShufflePeriod = shufflePeriod ?? ShufflePeriod;
            LevelCustomData = levelCustomData ?? LevelCustomData;
            BeatmapCustomDatasByFilename =
                beatmapCustomDatasByFilename ?? BeatmapCustomDatasByFilename;
            if (contributors != null)
            {
                LevelCustomData ??= new CustomData();
                Contributors = contributors;
                if (LevelContext.Version.Major == 2)
                {
                    ContributorData.SerializeV2(LevelCustomData, contributors);
                }
                else
                {
                    ContributorData.SerializeV4(LevelCustomData, contributors);
                }
            }
            else
            {
                Contributors =
                    LevelContext.Version.Major == 2
                        ? ContributorData.DeserializeV2(LevelCustomData)
                        : ContributorData.DeserializeV4(LevelCustomData);
            }
            if (customPlatformInfo != null)
            {
                LevelCustomData ??= new CustomData();
                CustomPlatformInfo = customPlatformInfo;
                if (LevelContext.Version.Major == 2)
                {
                    CustomPlatformsListModel.CustomPlatformInfo.SerializeV2(
                        LevelCustomData,
                        customPlatformInfo
                    );
                }
                else
                {
                    CustomPlatformsListModel.CustomPlatformInfo.SerializeV4(
                        LevelCustomData,
                        customPlatformInfo
                    );
                }
            }
            else
            {
                CustomPlatformInfo =
                    LevelContext.Version.Major == 2
                        ? CustomPlatformsListModel.CustomPlatformInfo.DeserializeV2(LevelCustomData)
                        : CustomPlatformsListModel.CustomPlatformInfo.DeserializeV4(
                            LevelCustomData
                        );
            }
            if (characteristicDetailsByName != null)
            {
                CharacteristicDetailsByName = characteristicDetailsByName;
            }
        }
    }
}
