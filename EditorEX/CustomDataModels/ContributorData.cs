using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;

namespace EditorEX.CustomDataModels
{
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

        public static List<ContributorData> DeserializeV2(CustomData? customData)
        {
            var contributors = new List<ContributorData>();
            var jsonContributors = customData
                ?.Get<List<object>>("_contributors")
                ?.Select(x => x as CustomData)
                ?.ToList();
            if (jsonContributors == null)
            {
                return contributors;
            }
            for (int i = 0; i < jsonContributors.Count; i++)
            {
                var contrib = jsonContributors[i];
                string? name = contrib?.Get<string>("_name");
                string? icon = contrib?.Get<string>("_iconPath");
                string? role = contrib?.Get<string>("_role");
                if (name == null || icon == null || role == null)
                    continue;
                contributors.Add(new ContributorData(name, icon, role));
            }
            return contributors;
        }

        public static List<ContributorData> DeserializeV4(CustomData? customData)
        {
            var contributors = new List<ContributorData>();
            var jsonContributors = customData
                ?.Get<List<object>>("contributors")
                ?.Select(x => x as CustomData)
                ?.ToList();
            if (jsonContributors == null)
            {
                return contributors;
            }
            for (int i = 0; i < jsonContributors.Count; i++)
            {
                var contrib = jsonContributors[i];
                string? name = contrib?.Get<string>("name");
                string? icon = contrib?.Get<string>("iconPath");
                string? role = contrib?.Get<string>("role");
                if (name == null || icon == null || role == null)
                    continue;
                contributors.Add(new ContributorData(name, icon, role));
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
