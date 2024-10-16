using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapLevelSaveDataVersion4;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomDataModels;
using EditorEX.MapData.SerializedSaveData;
using HarmonyLib;
using IPA.Loader;
using ModestTree;
using Newtonsoft.Json;
using SiraUtil.Zenject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Zenject;
using static EditorEX.MapData.SerializedSaveData.SerializedCustomLevelInfoSaveData;

namespace EditorEX.MapData.SaveDataSavers
{
    internal class V2CustomSaveDataSaver : ICustomSaveDataSaver
    {
        private BeatmapLevelDataModel _beatmapLevelDataModel;
        private PluginMetadata _metadata;
        private LevelCustomDataModel _levelCustomDataModel;

        [Inject]
        private void Construct(
            BeatmapLevelDataModel beatmapLevelDataModel,
            UBinder<Plugin, PluginMetadata> metadata,
            LevelCustomDataModel levelCustomDataModel)
        {
            _beatmapLevelDataModel = beatmapLevelDataModel;
            _metadata = metadata.Value;
            _levelCustomDataModel = levelCustomDataModel;
        }

        public bool IsVersion(Version version)
        {
            return version.Major == 2;
        }

        public void Save(BeatmapProjectManager beatmapProjectManager, bool clearDirty)
        {
            BeatmapLevelColorSchemeSaveData[] beatmapLevelColorSchemes = new BeatmapLevelColorSchemeSaveData[_beatmapLevelDataModel.colorSchemes.Count];
            for (int i = 0; i < _beatmapLevelDataModel.colorSchemes.Count; i++)
            {
                BeatmapLevelColorSchemeEditorData beatmapLevelColorSchemeEditorData = _beatmapLevelDataModel.colorSchemes[i];
                beatmapLevelColorSchemes[i] = new BeatmapLevelColorSchemeSaveData
                {
                    useOverride = false,
                    colorScheme = new PlayerSaveData.ColorScheme(beatmapLevelColorSchemeEditorData.colorSchemeName, beatmapLevelColorSchemeEditorData.saberAColor, beatmapLevelColorSchemeEditorData.saberBColor, beatmapLevelColorSchemeEditorData.environmentColor0, beatmapLevelColorSchemeEditorData.environmentColor1, beatmapLevelColorSchemeEditorData.obstaclesColor, beatmapLevelColorSchemeEditorData.environmentColor0Boost, beatmapLevelColorSchemeEditorData.environmentColor1Boost)
                };
            }

            var v4beatmaps = _beatmapLevelDataModel.difficultyBeatmaps;

            string[] envNames = v4beatmaps.Select(x => x.Value.environmentName._environmentName).Distinct().ToArray();

            var newSets = new List<SerializedDifficultyBeatmapSet>();
            foreach (var beatmap in v4beatmaps)
            {
                var k = beatmap.Key;
                var v = beatmap.Value;
                var existing = newSets.FirstOrDefault(x => x._beatmapCharacteristicName == k.Item1.serializedName || x._beatmapCharacteristicName == k.Item1.name);
                if (existing == null)
                {
                    int count = v4beatmaps.Count(x => x.Key.Item1.serializedName == k.Item1.serializedName || x.Key.Item1.name == k.Item1.name);
                    existing = new SerializedDifficultyBeatmapSet(k.Item1.serializedName, new SerializedDifficultyBeatmap[0]);
                    newSets.Add(existing);
                }

                var list = existing._difficultyBeatmaps.Cast<SerializedDifficultyBeatmap>().ToList();
                list.Add(new SerializedDifficultyBeatmap(
                    k.Item2.SerializedName(),
                    k.Item2.DefaultRating(),
                    v.beatmapFilename,
                    v.noteJumpMovementSpeed,
                    v.noteJumpStartBeatOffset,
                    _beatmapLevelDataModel.colorSchemes.IndexOf(v.colorScheme),
                    envNames.IndexOf(v.environmentName.ToString()),
                    _levelCustomDataModel.BeatmapCustomDatasByFilename[v.beatmapFilename]));
                existing._difficultyBeatmaps = list.ToArray();
            }

            // Modify the editors custom data to include editorex

            var levelCustomData = _levelCustomDataModel.LevelCustomData;

            var editors = levelCustomData.Get<CustomData>("_editors");
            if (editors == null)
            {
                editors = new CustomData();
                levelCustomData["_editors"] = editors;
            }

            editors["_lastEditedBy"] = "EditorEX + Official Editor";

            var editorEx = editors.Get<CustomData>("EditorEX");
            if (editorEx == null)
            {
                editorEx = new CustomData();
                editors["EditorEX"] = editorEx;
            }

            editorEx["version"] = _metadata.HVersion.ToString();

            _levelCustomDataModel.UpdateWith(null, null, null, null, null, levelCustomData, null);

            var saveData = new SerializedCustomLevelInfoSaveData(
                _beatmapLevelDataModel.songName,
                _beatmapLevelDataModel.songSubName,
                _beatmapLevelDataModel.songAuthorName,
                _levelCustomDataModel.LevelAuthorName,
                _beatmapLevelDataModel.beatsPerMinute,
                _beatmapLevelDataModel.songTimeOffset,
                _levelCustomDataModel.Shuffle,
                _levelCustomDataModel.ShufflePeriod,
                _beatmapLevelDataModel.previewStartTime,
                _beatmapLevelDataModel.previewDuration,
                _beatmapLevelDataModel.songFilename,
                _beatmapLevelDataModel.coverImageFilename,
                _levelCustomDataModel.EnvironmentName,
                _levelCustomDataModel.AllDirectionsEnvironmentName,
                envNames,
                beatmapLevelColorSchemes,
                newSets.ToArray(),
                levelCustomData);


            if (!beatmapProjectManager._projectOpened)
            {
                beatmapProjectManager._logger.Log(LogType.Warning, "Project not loaded");
                return;
            }

            string text = Path.Combine(beatmapProjectManager._workingBeatmapProject, "Info.dat");

            try
            {
                string contents = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                File.WriteAllText(text, contents);
            }
            catch (Exception message)
            {
                Debug.LogWarning(message);
            }

            Directory.SetLastWriteTime(beatmapProjectManager._workingBeatmapProject, DateTime.Now);

            beatmapProjectManager._logger.Log("BeatmapProjectManager - SaveBeatmapProject (EditorEX)");
            if (clearDirty)
            {
                _beatmapLevelDataModel.ClearDirty();
                beatmapProjectManager.BackupProject();
                beatmapProjectManager.SaveTempProject();
            }
        }
    }
}
