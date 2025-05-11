using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomDataModels;
using EditorEX.CustomJSONData;
using EditorEX.MapData.SerializedSaveData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Zenject;

namespace EditorEX.MapData.SaveDataLoaders
{
    public class V4CustomSaveDataLoader : ICustomSaveDataLoader
    {
        private EnvironmentsListModel _environmentsListModel;
        private BeatmapCharacteristicCollection _beatmapCharacteristicCollection;
        private BeatmapLevelDataModel _beatmapLevelDataModel;
        private LevelCustomDataModel _levelCustomDataModel;
        private CustomLevelLoader _customLevelLoader;

        [Inject]
        private void Construct(
            EnvironmentsListModel environmentsListModel,
            BeatmapCharacteristicCollection beatmapCharacteristicCollection,
            BeatmapLevelDataModel beatmapLevelDataModel,
            LevelCustomDataModel levelCustomDataModel,
            CustomLevelLoader customLevelLoader)
        {
            _environmentsListModel = environmentsListModel;
            _beatmapCharacteristicCollection = beatmapCharacteristicCollection;
            _beatmapLevelDataModel = beatmapLevelDataModel;
            _levelCustomDataModel = levelCustomDataModel;
            _customLevelLoader = customLevelLoader;
        }

        public bool IsVersion(Version version)
        {
            return version.Major == 4;
        }

        private static Color GetColorFromHtmlString(string colorHtmlString)
        {
            Color color;
            if (!ColorExtensions.FromHtmlStringRGBA(colorHtmlString, out color))
            {
                return Color.black;
            }
            return color;
        }

        public void Load(string projectPath)
        {
            CustomDataRepository.ClearAll();

            SerializedCustomBeatmapLevelSaveData customBeatmapLevelSaveData = JsonConvert.DeserializeObject<SerializedCustomBeatmapLevelSaveData>(File.ReadAllText(Path.Combine(projectPath, "Info.dat")));
            if (customBeatmapLevelSaveData == null)
            {
                return;
            }

            ValueTuple<string, string> correctedPathAndFilename = BeatmapProjectFileHelper.GetCorrectedPathAndFilename(projectPath, customBeatmapLevelSaveData.audio.songFilename);
            string songPath = correctedPathAndFilename.Item1;
            string songFileName = correctedPathAndFilename.Item2;

            ValueTuple<string, string> correctedPathAndFilename2 = BeatmapProjectFileHelper.GetCorrectedPathAndFilename(projectPath, customBeatmapLevelSaveData.coverImageFilename);
            string coverPath = correctedPathAndFilename2.Item1;
            string coverFileName = correctedPathAndFilename2.Item2;

            List<BeatmapLevelColorSchemeEditorData> colorSchemes = customBeatmapLevelSaveData.colorSchemes
                .Select((SerializedCustomBeatmapLevelSaveData.ColorScheme colorScheme) =>
                BeatmapLevelColorSchemeEditorData.Create(colorScheme.colorSchemeName,
                    colorScheme.overrideNotes,
                    GetColorFromHtmlString(colorScheme.saberAColor),
                    GetColorFromHtmlString(colorScheme.saberBColor),
                    GetColorFromHtmlString(colorScheme.obstaclesColor),
                    colorScheme.overrideLights,
                    GetColorFromHtmlString(colorScheme.environmentColor0),
                    GetColorFromHtmlString(colorScheme.environmentColor1),
                    GetColorFromHtmlString(colorScheme.environmentColor0Boost),
                    GetColorFromHtmlString(colorScheme.environmentColor1Boost))).ToList();

            string defaultEnvironment = _environmentsListModel.GetLastEnvironmentInfoWithType(EnvironmentType.Normal).serializedName;
            List<EnvironmentName> environmentNames = customBeatmapLevelSaveData.environmentNames.Select((string environmentName) => new EnvironmentName(environmentName)).ToList<EnvironmentName>();

            Dictionary<ValueTuple<BeatmapCharacteristicSO, BeatmapDifficulty>, DifficultyBeatmapData> beatmapDatas = new Dictionary<ValueTuple<BeatmapCharacteristicSO, BeatmapDifficulty>, DifficultyBeatmapData>();

            foreach (SerializedCustomBeatmapLevelSaveData.DifficultyBeatmap difficultyBeatmap in customBeatmapLevelSaveData.difficultyBeatmaps)
            {
                BeatmapCharacteristicSO beatmapCharacteristicBySerializedName = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(difficultyBeatmap.characteristic);

                BeatmapDifficulty beatmapDifficulty2;
                BeatmapDifficulty beatmapDifficulty = (difficultyBeatmap.difficulty.BeatmapDifficultyFromSerializedName(out beatmapDifficulty2) ? beatmapDifficulty2 : BeatmapDifficulty.Easy);

                DifficultyBeatmapData difficultyBeatmapData =
                    new DifficultyBeatmapData(
                        beatmapCharacteristicBySerializedName,
                        beatmapDifficulty,
                        difficultyBeatmap.beatmapDataFilename,
                        difficultyBeatmap.lightshowDataFilename,
                        (difficultyBeatmap.environmentNameIdx >= 0 && difficultyBeatmap.environmentNameIdx < environmentNames.Count) ? environmentNames[difficultyBeatmap.environmentNameIdx] : defaultEnvironment);

                difficultyBeatmapData.noteJumpMovementSpeed = difficultyBeatmap.noteJumpMovementSpeed;
                difficultyBeatmapData.noteJumpStartBeatOffset = difficultyBeatmap.noteJumpStartBeatOffset;
                difficultyBeatmapData.mappers = difficultyBeatmap.beatmapAuthors.mappers.ToArray();
                difficultyBeatmapData.lighters = difficultyBeatmap.beatmapAuthors.lighters.ToArray();
                difficultyBeatmapData.colorScheme = ((difficultyBeatmap.beatmapColorSchemeIdx >= 0 && difficultyBeatmap.beatmapColorSchemeIdx < colorSchemes.Count) ? colorSchemes[difficultyBeatmap.beatmapColorSchemeIdx] : null);

                beatmapDatas[new ValueTuple<BeatmapCharacteristicSO, BeatmapDifficulty>(beatmapCharacteristicBySerializedName, beatmapDifficulty)] = difficultyBeatmapData;
            }

            BeatmapLevelDataModel beatmapLevelDataModel = _beatmapLevelDataModel;
            string title = customBeatmapLevelSaveData.song.title;
            string subTitle = customBeatmapLevelSaveData.song.subTitle;
            string author = customBeatmapLevelSaveData.song.author;
            float? bpm = new float?(customBeatmapLevelSaveData.audio.bpm);
            float? previewStartTime = new float?(customBeatmapLevelSaveData.audio.previewStartTime);
            float? previewDuration = new float?(customBeatmapLevelSaveData.audio.previewDuration);
            string audioDataFilename = customBeatmapLevelSaveData.audio.audioDataFilename;

            Dictionary<string, CustomData> beatmapCustomDatasByFilename = customBeatmapLevelSaveData.difficultyBeatmaps.ToDictionary(x => x.beatmapDataFilename, x => x.customData);

            _levelCustomDataModel.UpdateWith(null, null, null, null, null, customBeatmapLevelSaveData.customData, beatmapCustomDatasByFilename);
            beatmapLevelDataModel.UpdateWith(title, subTitle, author, bpm, 0f, previewStartTime, previewDuration, audioDataFilename, songFileName, songPath, coverFileName, coverPath, new EnvironmentName?(defaultEnvironment), colorSchemes, beatmapDatas, true);
        }
    }
}