using System;
using System.Collections.Generic;
using System.IO;

namespace EditorEX.UI.Patches
{
    /// <summary>
    /// Resolves where a newly created map should live and how list cells derive
    /// a relative path. Vanilla <c>AddNewBeatmap</c> always writes under
    /// <c>BeatmapEditorSettings.customLevelsFolder</c>, which can be a different
    /// install than the selected EditorEX source tab.
    /// </summary>
    public static class BeatmapSourcePaths
    {
        public const string ImportedOfficialSourceName = "Official Custom Levels";
        public const string DefaultCustomLevelsSourceName = "Custom Levels";
        public const string DefaultWipSourceName = "Custom WIP Levels";

        public static string ResolveSaveSource(
            IReadOnlyDictionary<string, string>? sources,
            string? saveSource
        )
        {
            if (sources == null || sources.Count == 0)
            {
                return string.IsNullOrEmpty(saveSource) ? DefaultWipSourceName : saveSource;
            }

            if (!string.IsNullOrEmpty(saveSource) && sources.ContainsKey(saveSource))
            {
                return saveSource;
            }

            if (sources.ContainsKey(DefaultWipSourceName))
            {
                return DefaultWipSourceName;
            }

            foreach (string key in sources.Keys)
            {
                if (key.IndexOf("WIP", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return key;
                }
            }

            foreach (string key in sources.Keys)
            {
                return key;
            }

            return DefaultWipSourceName;
        }

        public static void EnsureDefaultSources(IDictionary<string, string> sources)
        {
            if (sources.Count > 0)
            {
                return;
            }

            string data = Path.Combine(Environment.CurrentDirectory, "Beat Saber_Data");
            sources[DefaultCustomLevelsSourceName] = Path.Combine(data, "CustomLevels")
                .Replace('\\', '/');
            sources[DefaultWipSourceName] = Path.Combine(data, "CustomWIPLevels")
                .Replace('\\', '/');
        }

        public static bool TryAddMissingFolder(IDictionary<string, string> sources, string? folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                return false;
            }

            string canonical = Canonical(folder);
            foreach (string existing in sources.Values)
            {
                if (Canonical(existing).Equals(canonical, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            string name = ImportedOfficialSourceName;
            int suffix = 2;
            while (sources.ContainsKey(name))
            {
                name = $"{ImportedOfficialSourceName} ({suffix++})";
            }

            sources[name] = folder.Replace('\\', '/');
            return true;
        }

        public static string ResolveNewMapRoot(
            IReadOnlyDictionary<string, string>? sources,
            string? selectedSource,
            string vanillaCustomLevelsFolder
        )
        {
            if (
                sources != null
                && !string.IsNullOrEmpty(selectedSource)
                && sources.TryGetValue(selectedSource, out string? sourcePath)
                && !string.IsNullOrWhiteSpace(sourcePath)
            )
            {
                return Canonical(sourcePath);
            }

            return Canonical(vanillaCustomLevelsFolder);
        }

        public static string GenerateRelativePath(
            string projectDirectoryPath,
            IEnumerable<string> sourcePaths,
            string? fallbackRoot
        )
        {
            string project = Canonical(projectDirectoryPath);
            string? root = FindRoot(project, sourcePaths, fallbackRoot);
            if (root == null)
            {
                return string.Empty;
            }

            if (root.Equals(project, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            int num = root.Length + 1;
            int folderNameLength = Path.GetFileNameWithoutExtension(project).Length;
            int length = project.Length - folderNameLength - num;
            if (num < 0 || length < 0 || num + length > project.Length)
            {
                return string.Empty;
            }

            return project.Substring(num, length);
        }

        private static string? FindRoot(
            string project,
            IEnumerable<string> sourcePaths,
            string? fallbackRoot
        )
        {
            foreach (string path in sourcePaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                string candidate = Canonical(path);
                if (IsBaseOf(candidate, project))
                {
                    return candidate;
                }
            }

            if (!string.IsNullOrWhiteSpace(fallbackRoot))
            {
                string fallback = Canonical(fallbackRoot);
                if (IsBaseOf(fallback, project))
                {
                    return fallback;
                }
            }

            return null;
        }

        private static bool IsBaseOf(string root, string child)
        {
            string rootPrefix = root.TrimEnd('\\', '/') + "\\";
            string childPrefix = child.TrimEnd('\\', '/') + "\\";
            return childPrefix.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase);
        }

        private static string Canonical(string path)
        {
            return path.Replace('/', '\\').TrimEnd('\\', '/');
        }
    }
}
