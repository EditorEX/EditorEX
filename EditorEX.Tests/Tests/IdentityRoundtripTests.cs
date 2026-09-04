using System.Threading.Tasks;
using EditorEX.Tests.BeatSaver;
using EditorEX.Tests.Harness;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class IdentityRoundtripTests : RoundtripTestBase
    {
        [SkippableFact]
        public async Task LoadSaveReload_PreservesV3StandardExpertPlus()
        {
            MapFixture fixture = MapCatalog.V3VanillaExpertPlus;
            string project = await EnsureMapAsync(fixture);
            LoadedDifficulty original = DifficultyRoundtripHarness.Load(project, fixture);
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
