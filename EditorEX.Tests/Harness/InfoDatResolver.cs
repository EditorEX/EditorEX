using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace EditorEX.Tests.Harness
{
    public static class InfoDatResolver
    {
        public static string FindInfoDat(string projectPath)
        {
            string info = Path.Combine(projectPath, "Info.dat");
            if (File.Exists(info))
            {
                return info;
            }

            string lower = Path.Combine(projectPath, "info.dat");
            if (File.Exists(lower))
            {
                return lower;
            }

            throw new FileNotFoundException("Info.dat not found in " + projectPath);
        }

        public static string ResolveBeatmapFilename(
            string projectPath,
            string characteristic,
            string difficulty
        )
        {
            JObject info = JObject.Parse(File.ReadAllText(FindInfoDat(projectPath)));
            string? filename =
                ResolveV2(info, characteristic, difficulty)
                ?? ResolveV4(info, characteristic, difficulty);
            if (string.IsNullOrEmpty(filename))
            {
                throw new InvalidOperationException(
                    $"No {characteristic} {difficulty} difficulty in {projectPath}"
                );
            }

            return filename!;
        }

        public static string? ResolveLightshowFilename(
            string projectPath,
            string characteristic,
            string difficulty
        )
        {
            JObject info = JObject.Parse(File.ReadAllText(FindInfoDat(projectPath)));
            if (info["difficultyBeatmaps"] is not JArray diffs)
            {
                return null;
            }

            foreach (JToken diff in diffs)
            {
                bool characteristicMatch = string.Equals(
                    diff["characteristic"]?.ToString(),
                    characteristic,
                    StringComparison.OrdinalIgnoreCase
                );
                bool difficultyMatch = string.Equals(
                    diff["difficulty"]?.ToString(),
                    difficulty,
                    StringComparison.OrdinalIgnoreCase
                );
                if (characteristicMatch && difficultyMatch)
                {
                    string? filename = diff["lightshowDataFilename"]?.ToString();
                    return string.IsNullOrEmpty(filename) ? null : filename;
                }
            }

            return null;
        }

        public static Version ReadDifficultyVersion(string projectPath, string beatmapFilename)
        {
            JObject difficulty = JObject.Parse(
                File.ReadAllText(Path.Combine(projectPath, beatmapFilename))
            );
            string? version = difficulty["version"]?.ToString() ?? difficulty["_version"]?.ToString();
            if (string.IsNullOrEmpty(version))
            {
                throw new InvalidOperationException(
                    "Difficulty file has no version: " + beatmapFilename
                );
            }

            return new Version(version);
        }

        private static string? ResolveV2(JObject info, string characteristic, string difficulty)
        {
            JToken? sets = info["_difficultyBeatmapSets"];
            if (sets is not JArray setArray)
            {
                return null;
            }

            foreach (JToken set in setArray)
            {
                if (
                    !string.Equals(
                        set["_beatmapCharacteristicName"]?.ToString(),
                        characteristic,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }

                if (set["_difficultyBeatmaps"] is not JArray diffs)
                {
                    continue;
                }

                foreach (JToken diff in diffs)
                {
                    if (
                        string.Equals(
                            diff["_difficulty"]?.ToString(),
                            difficulty,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        return diff["_beatmapFilename"]?.ToString();
                    }
                }
            }

            return null;
        }

        private static string? ResolveV4(JObject info, string characteristic, string difficulty)
        {
            if (info["difficultyBeatmaps"] is not JArray diffs)
            {
                return null;
            }

            foreach (JToken diff in diffs)
            {
                bool characteristicMatch = string.Equals(
                    diff["characteristic"]?.ToString(),
                    characteristic,
                    StringComparison.OrdinalIgnoreCase
                );
                bool difficultyMatch = string.Equals(
                    diff["difficulty"]?.ToString(),
                    difficulty,
                    StringComparison.OrdinalIgnoreCase
                );
                if (characteristicMatch && difficultyMatch)
                {
                    return diff["beatmapDataFilename"]?.ToString()
                        ?? diff["beatmapFilename"]?.ToString();
                }
            }

            return null;
        }
    }
}
