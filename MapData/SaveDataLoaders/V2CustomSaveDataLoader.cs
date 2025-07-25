﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomDataModels;
using EditorEX.CustomJSONData;
using Zenject;

namespace EditorEX.MapData.SaveDataLoaders
{
    public class V2CustomSaveDataLoader : ICustomSaveDataLoader
    {
        private BeatmapCharacteristicCollection _beatmapCharacteristicCollection = null!;
        private BeatmapLevelDataModel _beatmapLevelDataModel = null!;
        private LevelCustomDataModel _levelCustomDataModel = null!;
        private CustomLevelLoader _customLevelLoader = null!;

        [Inject]
        private void Construct(
            BeatmapCharacteristicCollection beatmapCharacteristicCollection,
            BeatmapLevelDataModel beatmapLevelDataModel,
            LevelCustomDataModel levelCustomDataModel,
            CustomLevelLoader customLevelLoader
        )
        {
            _beatmapCharacteristicCollection = beatmapCharacteristicCollection;
            _beatmapLevelDataModel = beatmapLevelDataModel;
            _levelCustomDataModel = levelCustomDataModel;
            _customLevelLoader = customLevelLoader;
        }

        public bool IsVersion(Version version)
        {
            return version.Major == 2;
        }

        public void Load(string projectPath)
        {
            CustomDataRepository.ClearAll();

            CustomLevelInfoSaveData standardLevelInfoSaveData = CustomLevelInfoSaveData.Deserialize(
                File.ReadAllText(Path.Combine(projectPath, "Info.dat"))
            );
            if (standardLevelInfoSaveData == null)
            {
                return;
            }
            ValueTuple<string, string> correctedPathAndFilename =
                BeatmapProjectFileHelper.GetCorrectedPathAndFilename(
                    projectPath,
                    standardLevelInfoSaveData.songFilename
                );
            string item = correctedPathAndFilename.Item1;
            string item2 = correctedPathAndFilename.Item2;
            ValueTuple<string, string> correctedPathAndFilename2 =
                BeatmapProjectFileHelper.GetCorrectedPathAndFilename(
                    projectPath,
                    standardLevelInfoSaveData.coverImageFilename
                );
            string item3 = correctedPathAndFilename2.Item1;
            string item4 = correctedPathAndFilename2.Item2;
            List<BeatmapLevelColorSchemeEditorData> beatmapLevelColorSchemes =
                new List<BeatmapLevelColorSchemeEditorData>();
            if (standardLevelInfoSaveData.colorSchemes != null)
            {
                foreach (
                    BeatmapLevelColorSchemeSaveData beatmapLevelColorSchemeSaveData in standardLevelInfoSaveData.colorSchemes
                )
                {
                    beatmapLevelColorSchemes.Add(
                        BeatmapLevelColorSchemeEditorData.Create(
                            beatmapLevelColorSchemeSaveData.colorScheme.colorSchemeId,
                            true,
                            beatmapLevelColorSchemeSaveData.colorScheme.saberAColor,
                            beatmapLevelColorSchemeSaveData.colorScheme.saberBColor,
                            beatmapLevelColorSchemeSaveData.colorScheme.obstaclesColor,
                            true,
                            beatmapLevelColorSchemeSaveData.colorScheme.environmentColor0,
                            beatmapLevelColorSchemeSaveData.colorScheme.environmentColor1,
                            beatmapLevelColorSchemeSaveData.colorScheme.environmentColor0Boost,
                            beatmapLevelColorSchemeSaveData.colorScheme.environmentColor1Boost
                        )
                    );
                }
            }

            bool flag3 =
                standardLevelInfoSaveData.environmentNames != null
                && standardLevelInfoSaveData.environmentNames.Length != 0;
            List<EnvironmentName> list = new List<EnvironmentName>();
            if (!flag3)
            {
                list.Add(
                    _customLevelLoader.CreateEnvironmentName(
                        standardLevelInfoSaveData.environmentName,
                        _customLevelLoader._defaultEnvironmentInfo
                    )
                );
                list.Add(
                    _customLevelLoader.CreateEnvironmentName(
                        standardLevelInfoSaveData.allDirectionsEnvironmentName,
                        _customLevelLoader._defaultAllDirectionsEnvironmentInfo
                    )
                );
            }
            else
            {
                foreach (string text in standardLevelInfoSaveData.environmentNames ?? [])
                {
                    list.Add(
                        _customLevelLoader.CreateEnvironmentName(
                            text,
                            _customLevelLoader._defaultEnvironmentInfo
                        )
                    );
                }
            }

            string[] authors;
            if (string.IsNullOrEmpty(standardLevelInfoSaveData.levelAuthorName))
            {
                authors = Array.Empty<string>();
            }
            else
            {
                authors = (
                    from a in standardLevelInfoSaveData.levelAuthorName.Split(',', '&')
                    select a.Trim()
                ).ToArray();
            }

            Dictionary<
                ValueTuple<BeatmapCharacteristicSO, BeatmapDifficulty>,
                DifficultyBeatmapData
            > dictionary =
                new Dictionary<
                    ValueTuple<BeatmapCharacteristicSO, BeatmapDifficulty>,
                    DifficultyBeatmapData
                >();
            foreach (
                StandardLevelInfoSaveData.DifficultyBeatmapSet difficultyBeatmapSet in standardLevelInfoSaveData.difficultyBeatmapSets
            )
            {
                BeatmapCharacteristicSO beatmapCharacteristicBySerializedName =
                    _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(
                        difficultyBeatmapSet.beatmapCharacteristicName
                    );
                if (!(beatmapCharacteristicBySerializedName == null))
                {
                    foreach (
                        StandardLevelInfoSaveData.DifficultyBeatmap difficultyBeatmap in difficultyBeatmapSet.difficultyBeatmaps
                    )
                    {
                        BeatmapDifficulty beatmapDifficulty;
                        difficultyBeatmap.difficulty.BeatmapDifficultyFromSerializedName(
                            out beatmapDifficulty
                        );

                        var data =
                            new BeatmapLevelSaveDataVersion4.BeatmapLevelSaveData.DifficultyBeatmap();
                        data.characteristic = difficultyBeatmapSet.beatmapCharacteristicName;
                        data.difficulty = difficultyBeatmap.difficulty;
                        data.beatmapAuthors =
                            new BeatmapLevelSaveDataVersion4.BeatmapLevelSaveData.BeatmapAuthors()
                            {
                                mappers = authors,
                                lighters = new string[0],
                            };
                        data.environmentNameIdx = difficultyBeatmap.environmentNameIdx;
                        data.beatmapColorSchemeIdx = difficultyBeatmap.beatmapColorSchemeIdx;
                        data.noteJumpMovementSpeed = difficultyBeatmap.noteJumpMovementSpeed;
                        data.noteJumpStartBeatOffset = difficultyBeatmap.noteJumpStartBeatOffset;
                        data.lightshowDataFilename = "";
                        data.beatmapDataFilename = difficultyBeatmap.beatmapFilename;

                        dictionary.Add(
                            new ValueTuple<BeatmapCharacteristicSO, BeatmapDifficulty>(
                                beatmapCharacteristicBySerializedName,
                                beatmapDifficulty
                            ),
                            new DifficultyBeatmapData(
                                beatmapCharacteristicBySerializedName,
                                data,
                                beatmapLevelColorSchemes,
                                list,
                                flag3
                                    ? list[difficultyBeatmap.environmentNameIdx]
                                    : (
                                        beatmapCharacteristicBySerializedName.containsRotationEvents
                                            ? list[1]
                                            : list[0]
                                    )
                            )
                        );
                    }
                }
            }

            Dictionary<string, CustomData> beatmapCustomDatasByFilename = standardLevelInfoSaveData
                .difficultyBeatmapSets.SelectMany(x =>
                    x.difficultyBeatmaps.Select(x =>
                        (
                            x.beatmapFilename,
                            (x as CustomLevelInfoSaveData.DifficultyBeatmap)!.customData
                        )
                    )
                )
                .ToDictionary(x => x.beatmapFilename, x => x.customData);

            _levelCustomDataModel.UpdateWith(
                standardLevelInfoSaveData.levelAuthorName,
                standardLevelInfoSaveData.allDirectionsEnvironmentName,
                standardLevelInfoSaveData.environmentName,
                standardLevelInfoSaveData.shuffle,
                standardLevelInfoSaveData.shufflePeriod,
                standardLevelInfoSaveData.customData,
                beatmapCustomDatasByFilename
            );
            _beatmapLevelDataModel.UpdateWith(
                standardLevelInfoSaveData.songName,
                standardLevelInfoSaveData.songSubName,
                standardLevelInfoSaveData.songAuthorName,
                new float?(standardLevelInfoSaveData.beatsPerMinute),
                new float?(standardLevelInfoSaveData.songTimeOffset),
                new float?(standardLevelInfoSaveData.previewStartTime),
                new float?(standardLevelInfoSaveData.previewDuration),
                "BPMInfo.dat",
                item2,
                item,
                item4,
                item3,
                standardLevelInfoSaveData.environmentName,
                beatmapLevelColorSchemes,
                dictionary,
                true
            );
        }
    }
}
