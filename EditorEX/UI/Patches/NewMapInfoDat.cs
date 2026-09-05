using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace EditorEX.UI.Patches
{
    public static class NewMapInfoDat
    {
        public const string BeatmapVersionKey = "beatmapVersion";

        public static string RewriteAsV2(string v4InfoJson, Version beatmapVersion)
        {
            JObject v4 = JObject.Parse(v4InfoJson);
            JObject song = (JObject?)v4["song"] ?? new JObject();
            JObject audio = (JObject?)v4["audio"] ?? new JObject();
            JArray environments = (JArray?)v4["environmentNames"] ?? new JArray();

            string environmentName =
                environments.Count > 0 ? environments[0]!.ToString() : "DefaultEnvironment";
            string allDirections =
                environments.Count > 1 ? environments[1]!.ToString() : "GlassDesertEnvironment";

            var info = new JObject
            {
                ["_version"] = "2.1.0",
                ["_songName"] = song["title"]?.ToString() ?? string.Empty,
                ["_songSubName"] = song["subTitle"]?.ToString() ?? string.Empty,
                ["_songAuthorName"] = song["author"]?.ToString() ?? string.Empty,
                ["_levelAuthorName"] = string.Empty,
                ["_beatsPerMinute"] = audio["bpm"] ?? 0,
                ["_songTimeOffset"] = 0,
                ["_shuffle"] = 0,
                ["_shufflePeriod"] = 0.5,
                ["_previewStartTime"] = audio["previewStartTime"] ?? 0,
                ["_previewDuration"] = audio["previewDuration"] ?? 10,
                ["_songFilename"] = audio["songFilename"]?.ToString() ?? string.Empty,
                ["_coverImageFilename"] = v4["coverImageFilename"]?.ToString() ?? string.Empty,
                ["_environmentName"] = environmentName,
                ["_allDirectionsEnvironmentName"] = allDirections,
                ["_environmentNames"] = environments,
                ["_colorSchemes"] = new JArray(),
                ["_difficultyBeatmapSets"] = new JArray(),
                ["_customData"] = new JObject(),
            };

            StampEditors(info, beatmapVersion, v2Keys: true);
            return info.ToString();
        }

        public static string StampBeatmapVersion(string infoJson, Version beatmapVersion)
        {
            JObject info = JObject.Parse(infoJson);
            bool v2Keys = info["_version"] != null;
            StampEditors(info, beatmapVersion, v2Keys);
            return info.ToString();
        }

        public static Version? TryReadStoredBeatmapVersion(string infoJson)
        {
            JObject info = JObject.Parse(infoJson);
            JObject? customData = (JObject?)info["customData"] ?? (JObject?)info["_customData"];
            if (customData == null)
            {
                return null;
            }

            JObject? editors = (JObject?)customData["editors"] ?? (JObject?)customData["_editors"];
            JObject? editorEx = (JObject?)editors?["EditorEX"];
            string? version = (string?)editorEx?[BeatmapVersionKey];
            return Version.TryParse(version, out Version? parsed) ? parsed : null;
        }

        public static void ApplyToProject(string projectPath, NewMapFormat format)
        {
            string infoPath = Path.Combine(projectPath, "Info.dat");
            string info = File.ReadAllText(infoPath);
            string next =
                format.InfoVersion.Major >= 4
                    ? StampBeatmapVersion(info, format.BeatmapVersion)
                    : RewriteAsV2(info, format.BeatmapVersion);
            File.WriteAllText(infoPath, next);
        }

        private static void StampEditors(JObject info, Version beatmapVersion, bool v2Keys)
        {
            string customDataKey = v2Keys ? "_customData" : "customData";
            string editorsKey = v2Keys ? "_editors" : "editors";
            string lastEditedKey = v2Keys ? "_lastEditedBy" : "lastEditedBy";

            JObject customData = (JObject?)info[customDataKey] ?? new JObject();
            JObject editors = (JObject?)customData[editorsKey] ?? new JObject();
            JObject editorEx = (JObject?)editors["EditorEX"] ?? new JObject();

            editors[lastEditedKey] = "EditorEX + Official Editor";
            editorEx[BeatmapVersionKey] = beatmapVersion.ToString();
            editors["EditorEX"] = editorEx;
            customData[editorsKey] = editors;
            info[customDataKey] = customData;
        }
    }
}
