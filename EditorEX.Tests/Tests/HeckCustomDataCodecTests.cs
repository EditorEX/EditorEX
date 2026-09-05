using System;
using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Heck.Codecs;
using EditorEX.Heck.EventData;
using EditorEX.Heck.ObjectData;
using Heck.Animation;
using Heck.Deserialize;
using Xunit;
using static EditorEX.Heck.Constants;

namespace EditorEX.Tests.Tests
{
    public class HeckCustomDataCodecTests
    {
        private static readonly Version V2 = new(2, 6, 0);
        private static readonly Version V3 = new(3, 3, 0);

        [Fact]
        public void Convert_v2_to_v3_remaps_object_track()
        {
            var json = new CustomData { ["_track"] = "lane", ["cinema"] = 1 };
            new HeckCustomDataCodec().Convert(
                json,
                new CustomDataCodecContext { SourceVersion = V2, TargetVersion = V3 }
            );

            Assert.False(json.ContainsKey("_track"));
            Assert.Equal("lane", json["track"]);
            Assert.Equal(1, json["cinema"]);
        }

        [Fact]
        public void Convert_custom_event_remaps_duration_and_easing()
        {
            var json = new CustomData
            {
                ["_duration"] = 2f,
                ["_easing"] = "easeOutQuad",
                ["_track"] = "lane",
            };
            ((ICustomEventCustomDataCodec)new HeckCustomDataCodec()).Convert(
                json,
                new CustomDataCodecContext { SourceVersion = V2, TargetVersion = V3 }
            );

            Assert.Equal(2f, json["duration"]);
            Assert.Equal("easeOutQuad", json["easing"]);
            Assert.Equal("lane", json["track"]);
        }

        [Fact]
        public void ConvertPointDefinitions_v3_to_v2_keeps_sibling_unknown_keys()
        {
            var beatmap = new CustomData
            {
                ["pointDefinitions"] = new CustomData { ["p"] = new List<object>() },
                ["bookmarks"] = 1,
            };

            new HeckCustomDataCodec().ConvertPointDefinitions(
                beatmap,
                new CustomDataCodecContext { SourceVersion = V3, TargetVersion = V2 }
            );

            Assert.False(beatmap.ContainsKey("pointDefinitions"));
            Assert.True(beatmap.ContainsKey("_pointDefinitions"));
            Assert.Equal(1, beatmap["bookmarks"]);
        }

        [Fact]
        public void Deserialize_object_resolves_named_tracks()
        {
            var lane = new Track();
            var tracks = new Dictionary<string, Track> { ["lane"] = lane };
            var json = new CustomData { ["track"] = "lane" };
            NoteEditorData note = NoteEditorData.CreateNew(
                4f,
                0,
                0,
                0,
                ColorType.ColorA,
                NoteType.Note,
                NoteCutDirection.Up,
                0
            );

            IObjectCustomData? typed = new HeckCustomDataCodec().Deserialize(
                note,
                json,
                new CustomDataCodecContext { SourceVersion = V3, TargetVersion = V3, Tracks = tracks }
            );

            var heck = Assert.IsType<EditorHeckObjectData>(typed);
            Assert.Same(lane, heck.Track[0]);
        }

        [Fact]
        public void Deserialize_InvokeEvent_on_v3_returns_typed_data()
        {
            CustomEventEditorData evt = CustomEventEditorData.CreateNew(
                1f,
                INVOKE_EVENT,
                new CustomData { ["event"] = "go" },
                false
            );

            ICustomEventCustomData? typed = new HeckCustomDataCodec().Deserialize(
                evt,
                evt.customData,
                new CustomDataCodecContext { SourceVersion = V3, TargetVersion = V3 }
            );

            Assert.IsType<EditorInvokeEventData>(typed);
        }

        [Fact]
        public void Deserialize_AnimateTrack_without_log_returns_null()
        {
            CustomEventEditorData evt = CustomEventEditorData.CreateNew(
                1f,
                ANIMATE_TRACK,
                new CustomData(),
                false
            );

            ICustomEventCustomData? typed = new HeckCustomDataCodec().Deserialize(
                evt,
                evt.customData,
                new CustomDataCodecContext()
            );

            Assert.Null(typed);
        }
    }
}
