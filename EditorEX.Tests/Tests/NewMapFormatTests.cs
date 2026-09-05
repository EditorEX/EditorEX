using System;
using EditorEX.UI.Patches;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class NewMapFormatTests
    {
        [Theory]
        [InlineData(NewMapFormatPreset.V2, "2.1.0", "2.6.0")]
        [InlineData(NewMapFormatPreset.V3, "2.1.0", "3.3.0")]
        [InlineData(NewMapFormatPreset.V4, "4.0.0", "4.0.0")]
        public void FromPreset_maps_info_and_beatmap_versions(
            NewMapFormatPreset preset,
            string info,
            string beatmap
        )
        {
            NewMapFormat format = NewMapFormat.FromPreset(preset);

            Assert.Equal(new Version(info), format.InfoVersion);
            Assert.Equal(new Version(beatmap), format.BeatmapVersion);
        }

        [Fact]
        public void EmptyBeatmapJson_v2_has_underscore_version_and_empty_lists()
        {
            string json = NewMapEmptyBeatmap.Write(new Version(2, 6, 0));
            JObject obj = JObject.Parse(json);

            Assert.Equal("2.6.0", (string?)obj["_version"]);
            Assert.Empty(obj["_notes"]!);
            Assert.Empty(obj["_events"]!);
            Assert.Empty(obj["_obstacles"]!);
        }

        [Fact]
        public void EmptyBeatmapJson_v3_has_version_and_empty_lists()
        {
            string json = NewMapEmptyBeatmap.Write(new Version(3, 3, 0));
            JObject obj = JObject.Parse(json);

            Assert.Equal("3.3.0", (string?)obj["version"]);
            Assert.Empty(obj["colorNotes"]!);
            Assert.Empty(obj["basicBeatmapEvents"]!);
            Assert.Empty(obj["obstacles"]!);
        }

        [Fact]
        public void RewriteAsV2_copies_song_fields_and_stamps_beatmap_version()
        {
            const string v4Info = """
                {
                  "version": "4.0.0",
                  "song": { "title": "Demo", "subTitle": "Sub", "author": "Artist" },
                  "audio": {
                    "songFilename": "song.ogg",
                    "audioDataFilename": "AudioData.dat",
                    "bpm": 148.0,
                    "previewStartTime": 12.5,
                    "previewDuration": 10.0
                  },
                  "coverImageFilename": "cover.png",
                  "environmentNames": [ "DefaultEnvironment", "GlassDesertEnvironment" ]
                }
                """;

            string rewritten = NewMapInfoDat.RewriteAsV2(v4Info, new Version(3, 3, 0));
            JObject obj = JObject.Parse(rewritten);

            Assert.Equal("2.1.0", (string?)obj["_version"]);
            Assert.Equal("Demo", (string?)obj["_songName"]);
            Assert.Equal("Sub", (string?)obj["_songSubName"]);
            Assert.Equal("Artist", (string?)obj["_songAuthorName"]);
            Assert.Equal(148.0, (double?)obj["_beatsPerMinute"]);
            Assert.Equal("song.ogg", (string?)obj["_songFilename"]);
            Assert.Equal("cover.png", (string?)obj["_coverImageFilename"]);
            Assert.Equal("DefaultEnvironment", (string?)obj["_environmentName"]);
            Assert.Equal("GlassDesertEnvironment", (string?)obj["_allDirectionsEnvironmentName"]);
            Assert.Equal(
                "3.3.0",
                NewMapInfoDat.TryReadStoredBeatmapVersion(rewritten)?.ToString()
            );
        }

        [Fact]
        public void StampV4_keeps_info_version_and_stores_beatmap_version()
        {
            const string v4Info = """
                {
                  "version": "4.0.0",
                  "song": { "title": "Demo" },
                  "audio": { "songFilename": "song.ogg", "bpm": 120.0 }
                }
                """;

            string stamped = NewMapInfoDat.StampBeatmapVersion(v4Info, new Version(4, 0, 0));
            JObject obj = JObject.Parse(stamped);

            Assert.Equal("4.0.0", (string?)obj["version"]);
            Assert.Equal("4.0.0", NewMapInfoDat.TryReadStoredBeatmapVersion(stamped)?.ToString());
        }

        [Fact]
        public void ResolveBeatmapVersion_prefers_stamped_then_map_then_v3_for_v2_info()
        {
            Assert.Equal(
                new Version(2, 6, 0),
                NewMapFormat.ResolveBeatmapVersion(
                    new Version(2, 6, 0),
                    new Version(3, 3, 0),
                    new Version(2, 1, 0)
                )
            );
            Assert.Equal(
                new Version(3, 3, 0),
                NewMapFormat.ResolveBeatmapVersion(null, new Version(3, 3, 0), new Version(2, 1, 0))
            );
            Assert.Equal(
                new Version(3, 3, 0),
                NewMapFormat.ResolveBeatmapVersion(null, null, new Version(2, 1, 0))
            );
            Assert.Null(NewMapFormat.ResolveBeatmapVersion(null, null, new Version(4, 0, 0)));
            Assert.Null(NewMapFormat.ResolveBeatmapVersion(null, null, null));
        }
    }
}
