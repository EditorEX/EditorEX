using System.Threading.Tasks;
using EditorEX.Tests.BeatSaver;
using EditorEX.Tests.Harness;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class IdentityRoundtripTests : RoundtripTestBase
    {
        [SkippableTheory]
        [MemberData(nameof(MapCatalog.AllTheoryData), MemberType = typeof(MapCatalog))]
        public async Task LoadSaveReload_PreservesDifficulty(MapFixture fixture)
        {
            string project = await EnsureMapAsync(fixture);
            LoadedDifficulty original = DifficultyRoundtripHarness.Load(project, fixture);
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
