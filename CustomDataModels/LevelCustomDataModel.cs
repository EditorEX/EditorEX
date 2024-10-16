using CustomJSONData.CustomBeatmap;
using EditorEX.MapData.Contexts;
using Heck.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EditorEX.CustomDataModels
{
    /// <summary>
    /// Holds data exclusive to V2 and V3 levels and also custom data for the level
    /// </summary>
    public class LevelCustomDataModel
    {
        public string LevelAuthorName { get; set; }
        public string AllDirectionsEnvironmentName { get; set; }
        public string EnvironmentName { get; set; }
        public float Shuffle { get; set; }
        public float ShufflePeriod { get; set; }
        public CustomData LevelCustomData { get; set; }
        public Dictionary<string, CustomData> BeatmapCustomDatasByFilename { get; set; }
        public List<ContributorData> Contributors { get; set; }

        public void UpdateWith(string levelAuthorName = null, string allDirectionsEnvironmentName = null, string environmentName = null, float? shuffle = null, float? shufflePeriod = null, CustomData levelCustomData = null, Dictionary<string, CustomData> beatmapCustomDatasByFilename = null, List<ContributorData> contributors = null)
        {
            LevelAuthorName = levelAuthorName ?? LevelAuthorName;
            AllDirectionsEnvironmentName = allDirectionsEnvironmentName ?? AllDirectionsEnvironmentName;
            EnvironmentName = environmentName ?? EnvironmentName;
            Shuffle = shuffle ?? Shuffle;
            ShufflePeriod = shufflePeriod ?? ShufflePeriod;
            LevelCustomData = levelCustomData ?? LevelCustomData;
            BeatmapCustomDatasByFilename = beatmapCustomDatasByFilename ?? BeatmapCustomDatasByFilename;
            if (contributors != null)
            {
                Contributors = contributors;
                if (LevelContext.Version.Major == 2)
                {
                    ContributorData.SerializeV2(LevelCustomData, contributors);
                }
                else
                {
                    ContributorData.SerializeV2(LevelCustomData, contributors);
                }
            }
            else
            {
                Contributors = LevelContext.Version.Major == 2 ? ContributorData.DeserializeV2(LevelCustomData ?? levelCustomData) : ContributorData.DeserializeV4(LevelCustomData ?? levelCustomData);
            }
        }
    }

    public class ContributorData
    {
        public ContributorData(string name, string iconPath, string role)
        {
            Name = name;
            IconPath = iconPath;
            Role = role;
        }

        public string Name { get; set; }
        public string IconPath { get; set; }
        public string Role { get; set; }

        public static List<ContributorData> DeserializeV2(CustomData customData)
        {
            var contributors = new List<ContributorData>();
            var jsonContributors = customData.Get<List<object>>("_contributors").Select(x => x as CustomData).ToList();
            if (jsonContributors == null)
            {
                return contributors;
            }
            for (int i = 0; i < jsonContributors.Count; i++)
            {
                var contrib = jsonContributors[i];
                contributors.Add(new ContributorData(contrib.Get<string>("_name"), contrib.Get<string>("_iconPath"), contrib.Get<string>("_role")));
            }
            return contributors;
        }

        public static List<ContributorData> DeserializeV4(CustomData customData)
        {
            var contributors = new List<ContributorData>();
            var jsonContributors = customData.Get<List<object>>("contributors").Select(x => x as CustomData).ToList();
            if (jsonContributors == null)
            {
                return contributors;
            }
            for (int i = 0; i < jsonContributors.Count; i++)
            {
                var contrib = jsonContributors[i];
                contributors.Add(new ContributorData(contrib.Get<string>("name"), contrib.Get<string>("iconPath"), contrib.Get<string>("role")));
            }
            return contributors;
        }

        public static void SerializeV2(CustomData customData, List<ContributorData> contributorData)
        {
            var jsonContributors = new List<object>();
            for (int i = 0; i < contributorData.Count; i++)
            {
                var contrib = contributorData[i];
                var jsonContributor = new CustomData();
                jsonContributor["_name"] = contrib.Name;
                jsonContributor["_iconPath"] = contrib.IconPath;
                jsonContributor["_role"] = contrib.Role;
                jsonContributors.Add(jsonContributor);
            }
            customData["_contributors"] = jsonContributors;
        }

        public static void SerializeV4(CustomData customData, List<ContributorData> contributorData)
        {
            var jsonContributors = new List<object>();
            for (int i = 0; i < contributorData.Count; i++)
            {
                var contrib = contributorData[i];
                var jsonContributor = new CustomData();
                jsonContributor["name"] = contrib.Name;
                jsonContributor["iconPath"] = contrib.IconPath;
                jsonContributor["role"] = contrib.Role;
                jsonContributors.Add(jsonContributor);
            }
            customData["contributors"] = jsonContributors;
        }
    }
}
