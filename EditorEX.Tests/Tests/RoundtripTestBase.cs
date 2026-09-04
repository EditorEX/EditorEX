using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EditorEX.Tests.BeatSaver;
using EditorEX.Tests.Harness;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public abstract class RoundtripTestBase
    {
        protected static readonly BeatSaverClient Client = new();

        protected static async Task<string> EnsureMapAsync(MapFixture fixture)
        {
            try
            {
                return await Client.EnsureExtractedAsync(fixture);
            }
            catch (BeatSaverSkippedException ex)
            {
                Skip.If(true, ex.Message);
                throw;
            }
            catch (Exception ex) when (!Client.IsCached(fixture.Hash))
            {
                Skip.If(true, "BeatSaver download failed and cache is empty: " + ex.Message);
                throw;
            }
        }

        protected static void AssertSnapshotsEqual(
            LoadedMapSnapshot expected,
            LoadedMapSnapshot actual,
            [CallerMemberName] string? testName = null
        )
        {
            string? diff = LoadedMapComparer.Diff(expected, actual);
            Assert.True(diff == null, testName + Environment.NewLine + diff);
        }
    }
}
