using System.Linq;
using System.Threading.Tasks;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using EditorEX.Tests.BeatSaver;
using EditorEX.Tests.Harness;
using EditorEX.Tests.Transforms;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class MutationRoundtripTests : RoundtripTestBase
    {
        [SkippableFact]
        public async Task ShiftBeats_IsPreservedAfterSaveReload()
        {
            await RoundtripMutation(loaded =>
            {
                MapTransforms.ShiftBeats(loaded.Result, loaded.Repository, 1f);
            });
        }

        [SkippableFact]
        public async Task AddColorNote_IsPreservedAfterSaveReload()
        {
            await RoundtripMutation(loaded =>
            {
                MapTransforms.AddColorNote(loaded.Result, loaded.Repository);
            });
        }

        [SkippableFact]
        public async Task RemoveOneNote_IsPreservedAfterSaveReload()
        {
            await RoundtripMutation(loaded =>
            {
                NoteEditorData? removed = MapTransforms.RemoveOneNote(loaded.Result);
                Assert.NotNull(removed);
            });
        }

        [SkippableFact]
        public async Task CustomDataAddAndStrip_IsPreservedAfterSaveReload()
        {
            await RoundtripMutation(loaded =>
            {
                MapTransforms.AddAndStripCustomData(loaded.Result, loaded.Repository);
                NoteEditorData? note = loaded.Result.Notes.FirstOrDefault(n =>
                    n.noteType == NoteType.Note
                );
                Assert.NotNull(note);
            });
        }

        private static async Task RoundtripMutation(System.Action<LoadedDifficulty> mutate)
        {
            MapFixture fixture = MapCatalog.V3VanillaExpertPlus;
            string project = await EnsureMapAsync(fixture);
            LoadedDifficulty original = DifficultyRoundtripHarness.Load(project, fixture);
            mutate(original);
            LoadedMapSnapshot expected = LoadedMapSnapshot.Capture(
                original.Result,
                original.Repository
            );

            LoadedDifficulty reloaded = DifficultyRoundtripHarness.Roundtrip(original);
            LoadedMapSnapshot actual = LoadedMapSnapshot.Capture(
                reloaded.Result,
                reloaded.Repository
            );

            AssertSnapshotsEqual(expected, actual);
        }
    }
}
