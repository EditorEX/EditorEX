using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.MapData.Objects;
using Xunit;
using V3Custom = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;

namespace EditorEX.Tests.Tests
{
    public class CustomEventCodecTests
    {
        [Fact]
        public void LoadV3_maps_beat_type_and_payload()
        {
            var data = new V3Custom.CustomEventSaveData(
                8f,
                "AnimateTrack",
                new CustomData { ["track"] = "lane" }
            );

            CustomEventEditorData loaded = CustomEventCodec.LoadV3(data);

            Assert.Equal(8f, loaded.beat);
            Assert.Equal("AnimateTrack", loaded.eventType);
            Assert.Equal("lane", loaded.customData["track"]);
            Assert.False(loaded.version2_6_0AndEarlier);
        }

        [Fact]
        public void LoadV4_reads_short_keys()
        {
            var entry = new CustomData
            {
                ["b"] = 4f,
                ["t"] = "AssignPathAnimation",
                ["d"] = new CustomData { ["track"] = "p" },
            };

            CustomEventEditorData loaded = CustomEventCodec.LoadV4(entry);

            Assert.Equal(4f, loaded.beat);
            Assert.Equal("AssignPathAnimation", loaded.eventType);
            Assert.Equal("p", loaded.customData["track"]);
        }

        [Fact]
        public void LoadV4_falls_back_to_legacy_keys()
        {
            var entry = new CustomData
            {
                ["_time"] = 2f,
                ["_type"] = "InvokeEvent",
                ["_data"] = new CustomData { ["event"] = "go" },
            };

            CustomEventEditorData loaded = CustomEventCodec.LoadV4(entry);

            Assert.Equal(2f, loaded.beat);
            Assert.Equal("InvokeEvent", loaded.eventType);
            Assert.Equal("go", loaded.customData["event"]);
        }

        [Fact]
        public void Write_then_Read_v3_roundtrips_events()
        {
            var beatmap = new CustomData { ["_customEvents"] = 1 };
            var events = new List<CustomEventEditorData>
            {
                CustomEventEditorData.CreateNew(
                    6f,
                    "AnimateTrack",
                    new CustomData { ["track"] = "a", ["NE_tmp"] = 1 },
                    false
                ),
            };

            CustomEventCodec.Write(beatmap, events, v3: true);

            Assert.False(beatmap.ContainsKey("_customEvents"));
            List<CustomEventEditorData> read = CustomEventCodec.Read(beatmap, v3: true);
            Assert.Single(read);
            Assert.Equal(6f, read[0].beat);
            Assert.Equal("AnimateTrack", read[0].eventType);
            Assert.Equal("a", read[0].customData["track"]);
            Assert.False(read[0].customData.ContainsKey("NE_tmp"));
        }

        [Fact]
        public void Write_empty_list_removes_key()
        {
            var beatmap = new CustomData
            {
                ["customEvents"] = new List<object> { new CustomData() },
            };

            CustomEventCodec.Write(beatmap, new List<CustomEventEditorData>(), v3: true);

            Assert.False(beatmap.ContainsKey("customEvents"));
        }

        [Fact]
        public void SaveV3_filters_scratch_keys()
        {
            CustomEventEditorData evt = CustomEventEditorData.CreateNew(
                1f,
                "AnimateTrack",
                new CustomData { ["track"] = "a", ["NE_x"] = 1 },
                false
            );

            V3Custom.CustomEventSaveData saved = CustomEventCodec.SaveV3(evt);

            Assert.Equal("a", saved.customData["track"]);
            Assert.False(saved.customData.ContainsKey("NE_x"));
        }
    }
}
