﻿using BeatmapEditor3D.DataModels;
using EditorEX.Config;
using EditorEX.SDK.Factories;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.UI.Patches
{
    internal class BeatmapsCollectionDataModelPatches : IAffinity
    {
        private readonly SiraLog _siraLog;
        private readonly SourcesConfig _sourcesConfig;

        private BeatmapsCollectionDataModelPatches(
            SiraLog siraLog,
            SourcesConfig sourcesConfig)
        {
            _siraLog = siraLog;
            _sourcesConfig = sourcesConfig;
        }

        private string EndsWithSeparator(string absolutePath)
        {
            return absolutePath?.TrimEnd('/', '\\') + "/";
        }

        private bool IsBaseOf(DirectoryInfo root, DirectoryInfo child)
        {
            var directoryPath = EndsWithSeparator(new Uri(child.FullName).AbsolutePath);
            var rootPath = EndsWithSeparator(new Uri(root.FullName).AbsolutePath);
            return directoryPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapsCollectionDataModel), nameof(BeatmapsCollectionDataModel.GenerateRelativePath))]
        private bool GenerateRelativePathWithCustomSource(string projectDirectoryPath, ref string __result)
        {
            var projectDirectory = new DirectoryInfo(projectDirectoryPath);

            var allPaths = _sourcesConfig.Sources.SelectMany(x => x.Value);
            string sourcePath = "";
            foreach (var path in allPaths)
            {
                if (IsBaseOf(new DirectoryInfo(path), projectDirectory))
                {
                    sourcePath = path;
                    break;
                }
            }

            if (sourcePath.Equals(projectDirectoryPath))
            {
                __result = string.Empty;
                return false;
            }
            int num = sourcePath.Length + 1;
            int length = Path.GetFileNameWithoutExtension(projectDirectoryPath).Length;
            int length2 = projectDirectoryPath.Length;
            __result =  projectDirectoryPath.Substring(num, length2 - length - num);

            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapsCollectionDataModel), nameof(BeatmapsCollectionDataModel.RefreshCollection))]
        private bool UseCustomLevelSources(BeatmapsCollectionDataModel __instance)
        {
            if (_sourcesConfig.Sources == null || _sourcesConfig.Sources.Count == 0)
            {
                _sourcesConfig.Sources = new Dictionary<string, List<string>>
                {
                    { "Custom Levels", GenerateDefaultSources(__instance, "CustomLevels") },
                    { "Custom WIP Levels", GenerateDefaultSources(__instance, "CustomWIPLevels") }
                };
            }

            List<string> pathsToLoad = new List<string>();
            if (!_sourcesConfig.Sources.TryGetValue(_sourcesConfig.SelectedSource, out pathsToLoad))
            {
                var defaultSource = _sourcesConfig.Sources.First();
                _siraLog.Error($"Failed to get paths from source: {_sourcesConfig.SelectedSource}, defaulting to {defaultSource.Key}");
                pathsToLoad = defaultSource.Value;
            }

            __instance._beatmapInfos = new();

            foreach (var path in pathsToLoad)
            {
                if (Directory.Exists(path))
                {
                    var projectDirectories = BeatmapProjectFileHelper.GetProjectDirectories(path);
                    __instance._beatmapInfos.AddRange(projectDirectories.Select(new Func<string, BeatmapsCollectionDataModel.BeatmapInfoData>(__instance.CreateBeatmapLevelInfoData)).ToList());
                }
                else
                {
                    _siraLog.Error($"Path {path} does not exist, skipping");
                }
            }

            __instance.SortBeatmaps();
            __instance._signalBus.Fire<BeatmapsCollectionSignals.UpdatedSignal>();

            return false;
        }

        private List<string> GenerateDefaultSources(BeatmapsCollectionDataModel __instance, string source)
        {
            var currentGamePath = Path.Combine(Environment.CurrentDirectory, "Beat Saber_Data");
            var configGamePath = Directory.GetParent(__instance._beatmapEditorSettingsDataModel.customLevelsFolder);

            _siraLog.Info($"current: {currentGamePath}, configured: {configGamePath}");

            bool samePath = new DirectoryInfo(currentGamePath).FullName == configGamePath.FullName;

            if (samePath)
            {
                return new List<string> { Path.Combine(currentGamePath, source).Replace("\\", "/") };
            }
            else
            {
                return new List<string> { Path.Combine(currentGamePath, source).Replace("\\", "/"), Path.Combine(configGamePath.FullName, source).Replace("\\", "/") };
            }
        }
    }
}