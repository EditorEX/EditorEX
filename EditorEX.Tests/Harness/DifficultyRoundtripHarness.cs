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
            Version version
        )
        {
            Result = result;
            Repository = repository;
            ProjectPath = projectPath;
            BeatmapFilename = beatmapFilename;
            Version = version;
        }

        public DifficultyLoadResult Result { get; }

        public ICustomDataRepository Repository { get; }

        public string ProjectPath { get; }

        public string BeatmapFilename { get; }

        public Version Version { get; }
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

            MapContext.Version = version;
            var repository = new CustomDataRepository();
            var loader = new LevelDataLoaderV3(repository);
            try
            {
                DifficultyLoadResult result = loader.Load(null!, projectPath, beatmapFilename, null);
                return new LoadedDifficulty(
                    result,
                    repository,
                    projectPath,
                    beatmapFilename,
                    version
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
                loaded.Repository
            );
            input.MapVersion = loaded.Version;
            var saveData = V3CustomLevelDataSaver.Build(input, loaded.Repository);
            LegacySavingUtil.SerializeAndSave(outputDirectory, filename, saveData);
        }

        public static LoadedDifficulty ReloadSaved(LoadedDifficulty original)
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
            return Load(
                temp,
                new MapFixture(
                    "saved",
                    Path.GetFileNameWithoutExtension(original.BeatmapFilename).Contains("Standard")
                        ? "Standard"
                        : "Standard",
                    GuessDifficulty(original.BeatmapFilename),
                    original.Version.Major
                )
            );
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

            MapContext.Version = original.Version;
            var repository = new CustomDataRepository();
            var loader = new LevelDataLoaderV3(repository);
            DifficultyLoadResult result = loader.Load(
                null!,
                temp,
                original.BeatmapFilename,
                null
            );
            return new LoadedDifficulty(
                result,
                repository,
                temp,
                original.BeatmapFilename,
                original.Version
            );
        }

        private static string GuessDifficulty(string beatmapFilename)
        {
            string name = Path.GetFileNameWithoutExtension(beatmapFilename);
            if (name.IndexOf("ExpertPlus", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "ExpertPlus";
            }

            if (name.IndexOf("Expert", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Expert";
            }

            if (name.IndexOf("Hard", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Hard";
            }

            if (name.IndexOf("Normal", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Normal";
            }

            if (name.IndexOf("Easy", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Easy";
            }

            return "ExpertPlus";
        }
    }
}
