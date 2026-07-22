using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;

namespace EditorEX.CustomDataModels
{
    public class CharacteristicDetailsData
    {
        public string? Label { get; set; }
        public string? IconFilename { get; set; }

        // v2/v3: read from a difficulty beatmap set's _customData.
        public static CharacteristicDetailsData? DeserializeV2(CustomData? setCustomData)
        {
            if (setCustomData == null)
                return null;
            string? label = setCustomData.Get<string>("_characteristicLabel");
            string? icon = setCustomData.Get<string>("_characteristicIconImageFilename");
            if (label == null && icon == null)
                return null;
            return new CharacteristicDetailsData { Label = label, IconFilename = icon };
        }

        // v2/v3: write into a difficulty beatmap set's _customData (mutates in place).
        public static void SerializeV2(CustomData setCustomData, CharacteristicDetailsData details)
        {
            if (!string.IsNullOrEmpty(details.Label))
                setCustomData["_characteristicLabel"] = details.Label;
            else
                setCustomData.TryRemove("_characteristicLabel", out _);

            if (!string.IsNullOrEmpty(details.IconFilename))
                setCustomData["_characteristicIconImageFilename"] = details.IconFilename;
            else
                setCustomData.TryRemove("_characteristicIconImageFilename", out _);
        }

        // v4: read the top-level customData.characteristicData[] array.
        public static Dictionary<string, CharacteristicDetailsData> DeserializeV4(
            CustomData? levelCustomData
        )
        {
            var result = new Dictionary<string, CharacteristicDetailsData>();
            var entries = levelCustomData
                ?.Get<List<object>>("characteristicData")
                ?.Select(x => x as CustomData)
                ?.ToList();
            if (entries == null)
                return result;
            foreach (var entry in entries)
            {
                string? name = entry?.Get<string>("characteristic");
                if (string.IsNullOrEmpty(name))
                    continue;
                result[name!] = new CharacteristicDetailsData
                {
                    Label = entry!.Get<string>("label"),
                    IconFilename = entry.Get<string>("iconPath"),
                };
            }
            return result;
        }

        // v4: write the top-level customData.characteristicData[] array (mutates in place).
        public static void SerializeV4(
            CustomData levelCustomData,
            Dictionary<string, CharacteristicDetailsData> detailsByName
        )
        {
            var list = new List<object>();
            foreach (var kvp in detailsByName)
            {
                var details = kvp.Value;
                if (
                    string.IsNullOrEmpty(details.Label)
                    && string.IsNullOrEmpty(details.IconFilename)
                )
                    continue;
                var entry = new CustomData();
                entry["characteristic"] = kvp.Key;
                if (!string.IsNullOrEmpty(details.Label))
                    entry["label"] = details.Label;
                if (!string.IsNullOrEmpty(details.IconFilename))
                    entry["iconPath"] = details.IconFilename;
                list.Add(entry);
            }
            if (list.Count > 0)
                levelCustomData["characteristicData"] = list;
            else
                levelCustomData.TryRemove("characteristicData", out _);
        }
    }
}
