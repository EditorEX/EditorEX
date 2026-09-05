using System.Collections.Generic;
using EditorEX.UI.Patches;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class BeatmapSourcePathsTests
    {
        private const string InstanceLevels =
            @"C:\Users\Owens\BSManager\BSInstances\1.42.1\Beat Saber_Data\CustomLevels";
        private const string SteamLevels =
            @"C:\Program Files (x86)\Steam\steamapps\common\Beat Saber\Beat Saber_Data\CustomLevels";
        private const string WipLevels =
            @"C:\Users\Owens\BSManager\BSInstances\1.42.1\Beat Saber_Data\CustomWIPLevels";

        [Fact]
        public void ResolveNewMapRoot_uses_the_selected_source_not_vanilla_settings()
        {
            var sources = new Dictionary<string, string>
            {
                { "Custom Levels", InstanceLevels.Replace('\\', '/') },
                { "Custom WIP Levels", WipLevels.Replace('\\', '/') },
            };

            Assert.Equal(
                InstanceLevels,
                BeatmapSourcePaths.ResolveNewMapRoot(sources, "Custom Levels", SteamLevels),
                ignoreCase: true
            );
            Assert.Equal(
                WipLevels,
                BeatmapSourcePaths.ResolveNewMapRoot(sources, "Custom WIP Levels", SteamLevels),
                ignoreCase: true
            );
        }

        [Fact]
        public void ResolveNewMapRoot_falls_back_to_vanilla_when_selected_source_is_missing()
        {
            Assert.Equal(
                SteamLevels,
                BeatmapSourcePaths.ResolveNewMapRoot(
                    new Dictionary<string, string>(),
                    "Custom Levels",
                    SteamLevels
                ),
                ignoreCase: true
            );
        }

        [Fact]
        public void GenerateRelativePath_returns_empty_for_a_direct_child_of_a_source()
        {
            string relative = BeatmapSourcePaths.GenerateRelativePath(
                InstanceLevels + @"\project",
                new[] { InstanceLevels.Replace('\\', '/') },
                fallbackRoot: SteamLevels
            );

            Assert.Equal(string.Empty, relative);
        }

        [Fact]
        public void GenerateRelativePath_does_not_emit_a_drive_colon_when_only_fallback_matches()
        {
            string relative = BeatmapSourcePaths.GenerateRelativePath(
                SteamLevels + @"\project",
                new[] { InstanceLevels.Replace('\\', '/') },
                fallbackRoot: SteamLevels
            );

            Assert.Equal(string.Empty, relative);
            Assert.DoesNotContain(":", relative);
        }

        [Fact]
        public void GenerateRelativePath_returns_empty_instead_of_throwing_when_no_root_matches()
        {
            string relative = BeatmapSourcePaths.GenerateRelativePath(
                SteamLevels + @"\project",
                new[] { InstanceLevels },
                fallbackRoot: null
            );

            Assert.Equal(string.Empty, relative);
        }

        [Fact]
        public void TryAddMissingFolder_adds_vanilla_folder_when_no_source_matches()
        {
            var sources = new Dictionary<string, string>
            {
                { "Custom Levels", InstanceLevels.Replace('\\', '/') },
            };

            Assert.True(BeatmapSourcePaths.TryAddMissingFolder(sources, SteamLevels));
            Assert.True(
                sources.ContainsKey(BeatmapSourcePaths.ImportedOfficialSourceName)
            );
            Assert.Equal(
                SteamLevels,
                BeatmapSourcePaths.ResolveNewMapRoot(
                    sources,
                    BeatmapSourcePaths.ImportedOfficialSourceName,
                    SteamLevels
                ),
                ignoreCase: true
            );
        }

        [Fact]
        public void TryAddMissingFolder_skips_when_a_source_already_has_that_path()
        {
            var sources = new Dictionary<string, string>
            {
                { "Custom Levels", SteamLevels.Replace('\\', '/') },
            };

            Assert.False(BeatmapSourcePaths.TryAddMissingFolder(sources, SteamLevels));
            Assert.Single(sources);
        }

        [Fact]
        public void TryAddMissingFolder_skips_empty_folder()
        {
            var sources = new Dictionary<string, string>();

            Assert.False(BeatmapSourcePaths.TryAddMissingFolder(sources, "  "));
            Assert.Empty(sources);
        }

        [Fact]
        public void ResolveSaveSource_keeps_a_configured_source()
        {
            var sources = new Dictionary<string, string>
            {
                { "Custom Levels", InstanceLevels },
                { "Custom WIP Levels", WipLevels },
            };

            Assert.Equal(
                "Custom Levels",
                BeatmapSourcePaths.ResolveSaveSource(sources, "Custom Levels")
            );
        }

        [Fact]
        public void ResolveSaveSource_defaults_to_wip_when_unset_or_missing()
        {
            var sources = new Dictionary<string, string>
            {
                { "Custom Levels", InstanceLevels },
                { "Custom WIP Levels", WipLevels },
            };

            Assert.Equal(
                "Custom WIP Levels",
                BeatmapSourcePaths.ResolveSaveSource(sources, null)
            );
            Assert.Equal(
                "Custom WIP Levels",
                BeatmapSourcePaths.ResolveSaveSource(sources, "Gone")
            );
        }

        [Fact]
        public void ResolveSaveSource_falls_back_to_first_source_when_wip_is_absent()
        {
            var sources = new Dictionary<string, string> { { "Custom Levels", InstanceLevels } };

            Assert.Equal(
                "Custom Levels",
                BeatmapSourcePaths.ResolveSaveSource(sources, "Custom WIP Levels")
            );
        }
    }
}
