using BeatmapEditor3D.DataModels;
using BeatmapLevelSaveDataVersion4;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomDataModels;
using EditorEX.MapData.SerializedSaveData;
using IPA.Loader;
using JetBrains.Annotations;
using ModestTree;
using Newtonsoft.Json;
using SiraUtil.Zenject;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Zenject;

namespace EditorEX.MapData.SaveDataSavers
{
    internal class V4CustomSaveDataSaver : ICustomSaveDataSaver
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
            return version.Major == 4;
        }

        public void Save(BeatmapProjectManager beatmapProjectManager, bool clearDirty)
        {
            SerializedCustomBeatmapLevelSaveData.ColorScheme[] array = _beatmapLevelDataModel.colorSchemes.Select((BeatmapLevelColorSchemeEditorData colorScheme) => new SerializedCustomBeatmapLevelSaveData.ColorScheme
            {
                colorSchemeName = colorScheme.colorSchemeName,
                saberAColor = ColorUtility.ToHtmlStringRGBA(colorScheme.saberAColor),
                saberBColor = ColorUtility.ToHtmlStringRGBA(colorScheme.saberBColor),
                obstaclesColor = ColorUtility.ToHtmlStringRGBA(colorScheme.obstaclesColor),
                environmentColor0 = ColorUtility.ToHtmlStringRGBA(colorScheme.environmentColor0),
                environmentColor1 = ColorUtility.ToHtmlStringRGBA(colorScheme.environmentColor1),
                environmentColor0Boost = ColorUtility.ToHtmlStringRGBA(colorScheme.environmentColor0Boost),
                environmentColor1Boost = ColorUtility.ToHtmlStringRGBA(colorScheme.environmentColor1Boost)
            }).ToArray();

            string[] environmentNames = _beatmapLevelDataModel.difficultyBeatmaps.Values.Select((DifficultyBeatmapData d) => d.environmentName.ToString()).Distinct().ToArray();

            // Modify the editors custom data to include editorex

            var levelCustomData = _levelCustomDataModel.LevelCustomData;

            var editors = levelCustomData.Get<CustomData>("editors");
            if (editors == null)
            {
                editors = new CustomData();
                levelCustomData["editors"] = editors;
            }

            editors["lastEditedBy"] = "EditorEX + Official Editor";

            var editorEx = editors.Get<CustomData>("EditorEX");
            if (editorEx == null)
            {
                editorEx = new CustomData();
                editors["EditorEX"] = editorEx;
            }

            editorEx["version"] = _metadata.HVersion.ToString();

            _levelCustomDataModel.UpdateWith(null, null, null, null, null, levelCustomData);

            var saveData = new SerializedCustomBeatmapLevelSaveData
            {
                song = new SerializedCustomBeatmapLevelSaveData.SongData
                {
                    title = _beatmapLevelDataModel.songName,
                    subTitle = _beatmapLevelDataModel.songSubName,
                    author = _beatmapLevelDataModel.songAuthorName
                },
                audio = new SerializedCustomBeatmapLevelSaveData.AudioData
                {
                    songFilename = _beatmapLevelDataModel.songFilename,
                    audioDataFilename = "AudioData.dat",
                    bpm = _beatmapLevelDataModel.beatsPerMinute,
                    previewStartTime = _beatmapLevelDataModel.previewStartTime,
                    previewDuration = _beatmapLevelDataModel.previewDuration
                },
                songPreviewFilename = _beatmapLevelDataModel.songFilename,
                coverImageFilename = _beatmapLevelDataModel.coverImageFilename,
                environmentNames = environmentNames,
                colorSchemes = array,
                customData = levelCustomData,
                difficultyBeatmaps = _beatmapLevelDataModel.difficultyBeatmaps.Values.Select((DifficultyBeatmapData difficultyBeatmap) => new SerializedCustomBeatmapLevelSaveData.DifficultyBeatmap
                {
                    characteristic = difficultyBeatmap.beatmapCharacteristic.SerializedName(),
                    difficulty = difficultyBeatmap.beatmapDifficulty.SerializedName(),
                    beatmapAuthors = new BeatmapLevelSaveData.BeatmapAuthors
                    {
                        mappers = difficultyBeatmap.mappers.ToArray(),
                        lighters = difficultyBeatmap.lighters.ToArray()
                    },
                    beatmapColorSchemeIdx = _beatmapLevelDataModel.colorSchemes.IndexOf(difficultyBeatmap.colorScheme),
                    environmentNameIdx = environmentNames.IndexOf(difficultyBeatmap.environmentName.ToString()),
                    noteJumpMovementSpeed = difficultyBeatmap.noteJumpMovementSpeed,
                    noteJumpStartBeatOffset = difficultyBeatmap.noteJumpStartBeatOffset,
                    beatmapDataFilename = difficultyBeatmap.beatmapFilename,
                    lightshowDataFilename = difficultyBeatmap.lightshowFilename,
                    customData = _levelCustomDataModel.BeatmapCustomDatasByFilename[difficultyBeatmap.beatmapFilename]
                }).ToArray()
            };


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
