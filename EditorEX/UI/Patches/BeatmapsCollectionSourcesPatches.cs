using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatmapEditor3D.DataModels;
using EditorEX.Config;
using EditorEX.Util;
using SiraUtil.Affinity;
using SiraUtil.Logging;

namespace EditorEX.UI.Patches
{
    /// <summary>
    /// Scans the configured (possibly multiple) map sources into
    /// <see cref="BeatmapsCollectionDataModel"/>, creates new maps in
    /// <c>SourcesConfig.SaveSource</c>, imports vanilla <c>customLevelsFolder</c>
    /// when it is not already a source, and resolves relative project paths against
    /// whichever configured source contains them.
    /// </summary>
    internal class BeatmapsCollectionSourcesPatches : IAffinity
    {
        private readonly SiraLog _siraLog;
        private readonly SourcesConfig _sourcesConfig;
        private string? _savedCustomLevelsFolder;

        private BeatmapsCollectionSourcesPatches(SiraLog siraLog, SourcesConfig sourcesConfig)
        {
            _siraLog = siraLog;
            _sourcesConfig = sourcesConfig;
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(BeatmapsCollectionDataModel),
            nameof(BeatmapsCollectionDataModel.AddNewBeatmap)
        )]
        private void UseSelectedSourceForNewMap(BeatmapsCollectionDataModel __instance)
        {
            var settings = __instance._beatmapEditorSettingsDataModel;
            _savedCustomLevelsFolder = settings._customLevelsFolder;
            string saveSource = BeatmapSourcePaths.ResolveSaveSource(
                _sourcesConfig.Sources,
                _sourcesConfig.SaveSource
            );
            _sourcesConfig.SaveSource = saveSource;
            _sourcesConfig.SelectedSource = saveSource;
            settings._customLevelsFolder = BeatmapSourcePaths.ResolveNewMapRoot(
                _sourcesConfig.Sources,
                saveSource,
                settings.customLevelsFolder
            );
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(BeatmapsCollectionDataModel),
            nameof(BeatmapsCollectionDataModel.AddNewBeatmap)
        )]
        private void RestoreCustomLevelsFolder(BeatmapsCollectionDataModel __instance)
        {
            __instance._beatmapEditorSettingsDataModel._customLevelsFolder =
                _savedCustomLevelsFolder;
            _savedCustomLevelsFolder = null;
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(BeatmapsCollectionDataModel),
            nameof(BeatmapsCollectionDataModel.GenerateRelativePath)
        )]
        private bool GenerateRelativePathWithCustomSource(
            BeatmapsCollectionDataModel __instance,
            string projectDirectoryPath,
            ref string __result
        )
        {
            __result = BeatmapSourcePaths.GenerateRelativePath(
                projectDirectoryPath,
                _sourcesConfig.Sources.Values,
                __instance._beatmapEditorSettingsDataModel.customLevelsFolder
            );
            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(BeatmapsCollectionDataModel),
            nameof(BeatmapsCollectionDataModel.RefreshCollection)
        )]
        private bool UseCustomLevelSources(BeatmapsCollectionDataModel __instance)
        {
            if (_sourcesConfig.Sources == null)
            {
                _sourcesConfig.Sources = new Dictionary<string, string>();
            }

            BeatmapSourcePaths.EnsureDefaultSources(_sourcesConfig.Sources);

            BeatmapSourcePaths.TryAddMissingFolder(
                _sourcesConfig.Sources,
                __instance._beatmapEditorSettingsDataModel.customLevelsFolder
            );

            string pathToLoad = string.Empty;
            if (!_sourcesConfig.Sources.TryGetValue(_sourcesConfig.SelectedSource, out pathToLoad))
            {
                var defaultSource = _sourcesConfig.Sources.First();
                _siraLog.Error(
                    $"Failed to get paths from source: {_sourcesConfig.SelectedSource}, defaulting to {defaultSource.Key}"
                );
                pathToLoad = defaultSource.Value;
            }

            __instance._beatmapInfos = new();

            if (Directory.Exists(pathToLoad))
            {
                var projectDirectories = DirectorySearchUtil.GetDirectoriesWithInfoDat(pathToLoad);
                __instance._beatmapInfos.AddRange(
                    projectDirectories
                        .Select(
                            new Func<string, BeatmapsCollectionDataModel.BeatmapInfoData>(
                                __instance.CreateBeatmapLevelInfoData
                            )
                        )
                        .ToList()
                );
            }
            else
            {
                _siraLog.Error($"Path {pathToLoad} does not exist, skipping");
            }

            __instance.SortBeatmaps();
            __instance._signalBus.Fire<BeatmapsCollectionSignals.UpdatedSignal>();

            return false;
        }
    }
}
