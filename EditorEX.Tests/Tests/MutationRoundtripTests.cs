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
        [SkippableTheory]
        [MemberData(nameof(MapCatalog.AllTheoryData), MemberType = typeof(MapCatalog))]
        public async Task ShiftBeats_IsPreservedAfterSaveReload(MapFixture fixture)
        {
            await RoundtripMutation(
                fixture,
                loaded =>
                {
                    MapTransforms.ShiftBeats(loaded.Result, loaded.Repository, 1f);
                }
            );
        }

        [SkippableTheory]
        [MemberData(nameof(MapCatalog.AllTheoryData), MemberType = typeof(MapCatalog))]
        public async Task AddColorNote_IsPreservedAfterSaveReload(MapFixture fixture)
        {
            await RoundtripMutation(
                fixture,
                loaded =>
                {
                    MapTransforms.AddColorNote(loaded.Result, loaded.Repository);
                }
            );
        }

        [SkippableTheory]
        [MemberData(nameof(MapCatalog.AllTheoryData), MemberType = typeof(MapCatalog))]
        public async Task RemoveOneNote_IsPreservedAfterSaveReload(MapFixture fixture)
        {
            await RoundtripMutation(
                fixture,
                loaded =>
                {
                    NoteEditorData? removed = MapTransforms.RemoveOneNote(loaded.Result);
                    Assert.NotNull(removed);
                }
            );
        }

        [SkippableTheory]
        [MemberData(nameof(MapCatalog.AllTheoryData), MemberType = typeof(MapCatalog))]
        public async Task AddObstacle_IsPreservedAfterSaveReload(MapFixture fixture)
        {
            await RoundtripMutation(
                fixture,
                loaded =>
                {
                    MapTransforms.AddObstacle(loaded.Result, loaded.Repository);
                }
            );
        }

        [SkippableTheory]
        [MemberData(nameof(MapCatalog.AllTheoryData), MemberType = typeof(MapCatalog))]
        public async Task AddCustomEvent_IsPreservedAfterSaveReload(MapFixture fixture)
        {
            await RoundtripMutation(
                fixture,
                loaded =>
                {
                    MapTransforms.AddCustomEvent(loaded.Repository);
                }
            );
        }

        [SkippableTheory]
        [MemberData(nameof(MapCatalog.AllTheoryData), MemberType = typeof(MapCatalog))]
        public async Task CustomDataAddAndStrip_IsPreservedAfterSaveReload(MapFixture fixture)
        {
            await RoundtripMutation(
                fixture,
                loaded =>
                {
                    MapTransforms.AddAndStripCustomData(loaded.Result, loaded.Repository);
                    NoteEditorData? note = loaded.Result.Notes.FirstOrDefault(n =>
                        n.noteType == NoteType.Note
                    );
                    Assert.NotNull(note);
                }
            );
        }

        private static async Task RoundtripMutation(
            MapFixture fixture,
            System.Action<LoadedDifficulty> mutate
        )
        {
            string project = await EnsureMapAsync(fixture);
            LoadedDifficulty original = DifficultyRoundtripHarness.Load(project, fixture);
            mutate(original);
            LoadedMapSnapshot expected = LoadedMapSnapshot.Capture(
                original.Result,
                original.Repository,
                original.Version
            );

            LoadedDifficulty reloaded = DifficultyRoundtripHarness.Roundtrip(original);
            LoadedMapSnapshot actual = LoadedMapSnapshot.Capture(
                reloaded.Result,
                reloaded.Repository,
                reloaded.Version
            );

            AssertSnapshotsEqual(expected, actual, fixture);
        }
    }
}
