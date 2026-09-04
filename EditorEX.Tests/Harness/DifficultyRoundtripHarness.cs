using System;
using System.IO;
using EditorEX.CustomJSONData;
using EditorEX.MapData.Contexts;
using EditorEX.MapData.LevelDataLoaders;
using EditorEX.MapData.LevelDataSavers;
using EditorEX.Tests.BeatSaver;

namespace EditorEX.Tests.Harness
{
    public sealed class LoadedDifficulty
    {
        public LoadedDifficulty(
            DifficultyLoadResult result,
            ICustomDataRepository repository,
            string projectPath,
            string beatmapFilename,
            Version version,
            MapFixture fixture,
            string? lightshowFilename
        )
        {
            Result = result;
            Repository = repository;
            ProjectPath = projectPath;
            BeatmapFilename = beatmapFilename;
            Version = version;
            Fixture = fixture;
            LightshowFilename = lightshowFilename;
        }

        public DifficultyLoadResult Result { get; }

        public ICustomDataRepository Repository { get; }

        public string ProjectPath { get; }

        public string BeatmapFilename { get; }

        public Version Version { get; }

        public MapFixture Fixture { get; }

        public string? LightshowFilename { get; }
    }

    public static class DifficultyRoundtripHarness
    {
        public static LoadedDifficulty Load(string projectPath, MapFixture fixture)
        {
            string beatmapFilename = InfoDatResolver.ResolveBeatmapFilename(
                projectPath,
                fixture.Characteristic,
                fixture.Difficulty
            );
            Version version = InfoDatResolver.ReadDifficultyVersion(projectPath, beatmapFilename);
            if (version.Major != fixture.ExpectedMajorVersion)
            {
                throw new InvalidOperationException(
                    $"Expected difficulty version major {fixture.ExpectedMajorVersion} but found {version} in {beatmapFilename}"
                );
            }

            string? lightshowFilename = InfoDatResolver.ResolveLightshowFilename(
                projectPath,
                fixture.Characteristic,
                fixture.Difficulty
            );

            MapContext.Version = version;
            var repository = new CustomDataRepository();
            ICustomLevelDataLoader loader = CreateLoader(version.Major, repository);
            try
            {
                DifficultyLoadResult result = loader.Load(
                    null!,
                    projectPath,
                    beatmapFilename,
                    lightshowFilename
                );
                return new LoadedDifficulty(
                    result,
                    repository,
                    projectPath,
                    beatmapFilename,
                    version,
                    fixture,
                    lightshowFilename
                );
            }
            catch (TypeLoadException ex)
            {
                throw new InvalidOperationException(
                    $"Failed to load game type '{ex.TypeName}': {ex.Message}",
                    ex
                );
            }
        }

        public static void Save(LoadedDifficulty loaded, string outputDirectory, string filename)
        {
            Directory.CreateDirectory(outputDirectory);
            MapContext.Version = loaded.Version;
            DifficultySaveInput input = DifficultySaveInput.FromLoadResult(
                loaded.Result,
                loaded.Repository,
                loaded.Version
            );
            switch (loaded.Version.Major)
            {
                case 2:
                    LegacySavingUtil.SerializeAndSave(
                        outputDirectory,
                        filename,
                        V2CustomLevelDataSaver.Build(input, loaded.Repository)
                    );
                    break;
                case 3:
                    LegacySavingUtil.SerializeAndSave(
                        outputDirectory,
                        filename,
                        V3CustomLevelDataSaver.Build(input, loaded.Repository)
                    );
                    break;
                case 4:
                    LegacySavingUtil.SerializeAndSave(
                        outputDirectory,
                        filename,
                        V4CustomLevelDataSaver.BuildBeatmap(input, loaded.Repository)
                    );
                    if (!string.IsNullOrEmpty(loaded.LightshowFilename))
                    {
                        LegacySavingUtil.SerializeAndSave(
                            outputDirectory,
                            loaded.LightshowFilename,
                            V4CustomLevelDataSaver.BuildLightshow(input, loaded.Repository)
                        );
                    }

                    break;
                default:
                    throw new NotSupportedException(
                        "Unsupported difficulty version " + loaded.Version
                    );
            }
        }

        public static LoadedDifficulty Roundtrip(LoadedDifficulty original)
        {
            string temp = Path.Combine(
                Path.GetTempPath(),
                "EditorEX.Tests",
                Guid.NewGuid().ToString("N")
            );
            Save(original, temp, original.BeatmapFilename);
            File.Copy(
                InfoDatResolver.FindInfoDat(original.ProjectPath),
                Path.Combine(temp, "Info.dat"),
                overwrite: true
            );
            return Load(temp, original.Fixture);
        }

        private static ICustomLevelDataLoader CreateLoader(
            int majorVersion,
            ICustomDataRepository repository
        )
        {
            return majorVersion switch
            {
                2 => new LevelDataLoaderV2(repository),
                3 => new LevelDataLoaderV3(repository),
                4 => new LevelDataLoaderV4(repository),
                _ => throw new NotSupportedException(
                    "Unsupported difficulty version major " + majorVersion
                ),
            };
        }
    }
}
