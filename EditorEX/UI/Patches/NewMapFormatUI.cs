using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Config;
using EditorEX.MapData.Contexts;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Dropdown;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using SiraUtil.Affinity;
using UnityEngine;

namespace EditorEX.UI.Patches
{
    /// <summary>
    /// Adds a v2 / v3 / v4 format picker and a save-source dropdown to
    /// <see cref="NewBeatmapViewController"/>, then applies the chosen Info +
    /// difficulty versions when the map (and later an empty difficulty) is created.
    /// </summary>
    internal class NewMapFormatUI : IAffinity
    {
        private static readonly string[] FormatLabels = ["v2", "v3", "v4"];
        private const float RowHeight = 40f;
        private const float RowStep = 50f;
        private const float ExtraRowsHeight = RowStep * 2f;

        private readonly IReactiveContainer _reactiveContainer;
        private readonly SourcesConfig _sourcesConfig;
        private readonly State<int> _presetIndex = StateUtils.Remember((int)NewMapFormatPreset.V4);
        private EditorDropdown<string>? _saveSourceDropdown;

        private NewMapFormatUI(IReactiveContainer reactiveContainer, SourcesConfig sourcesConfig)
        {
            _reactiveContainer = reactiveContainer;
            _sourcesConfig = sourcesConfig;
        }

        private NewMapFormat SelectedFormat =>
            NewMapFormat.FromPreset((NewMapFormatPreset)_presetIndex.Value);

        [AffinityPostfix]
        [AffinityPatch(
            typeof(NewBeatmapViewController),
            nameof(NewBeatmapViewController.DidActivate)
        )]
        private void AddFormatPicker(NewBeatmapViewController __instance, bool firstActivation)
        {
            _presetIndex.Value = (int)NewMapFormatPreset.V4;

            if (!firstActivation)
            {
                RefreshSaveSourceDropdown();
                return;
            }

            var songRow = (RectTransform)__instance._openSongView.transform;
            var inputs = (RectTransform)songRow.parent;

            PlaceRow(inputs, songRow, "EditorEXNewMapFormat", RowStep);
            PlaceRow(inputs, songRow, "EditorEXNewMapSaveSource", RowStep * 2f);

            ShiftRowsBelow(inputs, songRow.anchoredPosition.y, -ExtraRowsHeight);
            inputs.sizeDelta = new Vector2(
                inputs.sizeDelta.x,
                inputs.sizeDelta.y + ExtraRowsHeight
            );

            var formatHost = inputs.Find("EditorEXNewMapFormat");
            new EditorSegmentedControl { Values = FormatLabels, SelectedIndex = _presetIndex }
                .InEditorNamedRail("Format", 18f, 60f)
                .AsFlexItem(size: new YogaVector(100.pct, RowHeight))
                .WithReactiveContainer(_reactiveContainer)
                .Use(formatHost);

            var saveHost = inputs.Find("EditorEXNewMapSaveSource");
            new EditorDropdown<string>
            {
                Items = SaveSourceItems(),
                Key = ResolvedSaveSource(),
                OnKeyChanged = key => _sourcesConfig.SaveSource = key,
            }
                .Bind(ref _saveSourceDropdown)
                .AsFlexItem(size: new YogaVector(100.pct, RowHeight))
                .InEditorNamedRail("Save to", 18f, 60f)
                .WithReactiveContainer(_reactiveContainer)
                .Use(saveHost);
        }

        [AffinityPostfix]
        [AffinityPatch(
            typeof(BeatmapsCollectionDataModel),
            nameof(BeatmapsCollectionDataModel.AddNewBeatmap)
        )]
        private void ApplyFormatToNewMap(BeatmapsCollectionDataModel __instance, string songPath)
        {
            if (!File.Exists(songPath) || __instance._beatmapInfos.Count == 0)
            {
                return;
            }

            // SortBeatmaps is newest-first; the map just created is first, not last.
            string? folder = __instance._beatmapInfos[0].beatmapFolderPath;
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            string infoPath = Path.Combine(folder, "Info.dat");
            if (!File.Exists(infoPath))
            {
                return;
            }

            NewMapInfoDat.ApplyToProject(folder, SelectedFormat);
        }

        private static void ShiftRowsBelow(RectTransform inputs, float belowY, float deltaY)
        {
            for (int i = 0; i < inputs.childCount; i++)
            {
                var child = (RectTransform)inputs.GetChild(i);
                if (
                    child.name == "EditorEXNewMapFormat"
                    || child.name == "EditorEXNewMapSaveSource"
                )
                {
                    continue;
                }

                if (child.anchoredPosition.y < belowY)
                {
                    child.anchoredPosition += new Vector2(0f, deltaY);
                }
            }
        }

        [AffinityPrefix]
        [AffinityPatch(
            typeof(BeatmapProjectManager),
            nameof(BeatmapProjectManager.SaveEmptyBeatmapLevel)
        )]
        private bool WriteEmptyDifficulty(
            BeatmapProjectManager __instance,
            BeatmapCharacteristicSO beatmapCharacteristic,
            BeatmapDifficulty beatmapDifficulty
        )
        {
            if (
                !__instance._projectOpened
                || !__instance._beatmapLevelDataModel.difficultyBeatmaps.TryGetValue(
                    (beatmapCharacteristic, beatmapDifficulty),
                    out DifficultyBeatmapData? data
                )
            )
            {
                return true;
            }

            string infoPath = Path.Combine(__instance._workingBeatmapProject, "Info.dat");
            Version? stamped = File.Exists(infoPath)
                ? NewMapInfoDat.TryReadStoredBeatmapVersion(File.ReadAllText(infoPath))
                : null;
            Version? beatmapVersion = NewMapFormat.ResolveBeatmapVersion(
                stamped,
                MapContext.Version,
                LevelContext.Version
            );

            if (beatmapVersion == null || beatmapVersion.Major >= 4)
            {
                return true;
            }

            File.WriteAllText(
                Path.Combine(__instance._workingBeatmapProject, data.beatmapFilename),
                NewMapEmptyBeatmap.Write(beatmapVersion)
            );
            MapContext.Version = beatmapVersion;
            return false;
        }

        private void RefreshSaveSourceDropdown()
        {
            if (_saveSourceDropdown == null)
            {
                return;
            }

            string key = ResolvedSaveSource();
            _saveSourceDropdown.Items = SaveSourceItems();
            _saveSourceDropdown.Key = key;
            _sourcesConfig.SaveSource = key;
        }

        private string ResolvedSaveSource()
        {
            return BeatmapSourcePaths.ResolveSaveSource(
                _sourcesConfig.Sources,
                _sourcesConfig.SaveSource
            );
        }

        private Dictionary<string, BsDropdownItem> SaveSourceItems()
        {
            if (_sourcesConfig.Sources == null)
            {
                _sourcesConfig.Sources = new Dictionary<string, string>();
            }

            BeatmapSourcePaths.EnsureDefaultSources(_sourcesConfig.Sources);
            return _sourcesConfig.Sources.Keys.ToDictionary(
                name => name,
                name => new BsDropdownItem(name, null)
            );
        }

        private static void PlaceRow(
            RectTransform inputs,
            RectTransform songRow,
            string name,
            float yOffset
        )
        {
            var host = new GameObject(name, typeof(RectTransform));
            var hostRt = (RectTransform)host.transform;
            hostRt.SetParent(inputs, false);
            hostRt.anchorMin = new Vector2(0f, 1f);
            hostRt.anchorMax = new Vector2(0f, 1f);
            hostRt.pivot = new Vector2(0.5f, 0.5f);
            hostRt.sizeDelta = new Vector2(songRow.sizeDelta.x, RowHeight);
            hostRt.anchoredPosition = new Vector2(
                songRow.anchoredPosition.x,
                songRow.anchoredPosition.y - yOffset
            );
        }
    }
}
