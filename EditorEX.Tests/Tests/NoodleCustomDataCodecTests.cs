using System;
using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using CustomJSONData.CustomBeatmap;
using EditorEX.Heck.Codecs;
using EditorEX.NoodleExtensions.Codecs;
using EditorEX.NoodleExtensions.ObjectData;
using Xunit;
using static NoodleExtensions.NoodleController;

namespace EditorEX.Tests.Tests
{
    public class NoodleCustomDataCodecTests
    {
        private static readonly Version V2 = new(2, 6, 0);
        private static readonly Version V3 = new(3, 3, 0);

        [Fact]
        public void Convert_v2_to_v3_remaps_position_and_preserves_unknown()
        {
            var json = new CustomData
            {
                ["_position"] = new List<object> { 0f, 1f },
                ["cinema"] = new CustomData { ["foo"] = 1 },
            };
            var codec = new NoodleCustomDataCodec();
            codec.Convert(
                json,
                new CustomDataCodecContext { SourceVersion = V2, TargetVersion = V3 }
            );

            Assert.False(json.ContainsKey("_position"));
            Assert.True(json.ContainsKey("coordinates"));
            Assert.True(json.ContainsKey("cinema"));
        }

        [Fact]
        public void Convert_v2_interactable_false_to_uninteractable_true()
        {
            var json = new CustomData { ["_interactable"] = false };
            new NoodleCustomDataCodec().Convert(
                json,
                new CustomDataCodecContext { SourceVersion = V2, TargetVersion = V3 }
            );
            Assert.False(json.ContainsKey("_interactable"));
            Assert.Equal(true, json["uninteractable"]);
        }

        [Fact]
        public void Convert_preserves_unknown_nested_animation_keys()
        {
            var json = new CustomData
            {
                ["_animation"] = new CustomData
                {
                    ["_position"] = new List<object>(),
                    ["mapperExtra"] = 3,
                },
            };
            new NoodleCustomDataCodec().Convert(
                json,
                new CustomDataCodecContext { SourceVersion = V2, TargetVersion = V3 }
            );
            var anim = json.Get<CustomData>("animation");
            Assert.NotNull(anim);
            Assert.Equal(3, anim!["mapperExtra"]);
        }

        [Fact]
        public void Convert_same_version_does_not_rewrite_keys()
        {
            var json = new CustomData { ["_position"] = 1 };
            new NoodleCustomDataCodec().Convert(
                json,
                new CustomDataCodecContext { SourceVersion = V2, TargetVersion = V2 }
            );
            Assert.True(json.ContainsKey("_position"));
            Assert.False(json.ContainsKey("coordinates"));
        }

        [Fact]
        public void Convert_v3_to_v2_remaps_coordinates_back()
        {
            var json = new CustomData { ["coordinates"] = new List<object> { 1f, 2f } };
            new NoodleCustomDataCodec().Convert(
                json,
                new CustomDataCodecContext { SourceVersion = V3, TargetVersion = V2 }
            );
            Assert.True(json.ContainsKey("_position"));
            Assert.False(json.ContainsKey("coordinates"));
        }

        [Fact]
        public void Deserialize_note_reads_coordinates_njs_and_link()
        {
            NoteEditorData note = NoteEditorData.CreateNew(
                4f,
                1,
                0,
                0,
                ColorType.ColorA,
                NoteType.Note,
                NoteCutDirection.Up,
                0
            );
            var json = new CustomData
            {
                [NOTE_OFFSET] = new List<object> { 1.5f, 2f },
                [NOTE_JUMP_SPEED] = 18f,
                [UNINTERACTABLE] = true,
                ["link"] = "group",
            };

            var typed = (EditorNoodleNoteData)
                new NoodleCustomDataCodec().Deserialize(
                    note,
                    json,
                    new CustomDataCodecContext { SourceVersion = V3, TargetVersion = V3 }
                )!;

            Assert.Equal(1.5f, typed.StartX);
            Assert.Equal(2f, typed.StartY);
            Assert.Equal(18f, typed.Njs);
            Assert.True(typed.Uninteractable);
            Assert.Equal("group", typed.Link);
        }

        [Fact]
        public void Deserialize_v2_cuttable_false_is_uninteractable()
        {
            NoteEditorData note = NoteEditorData.CreateNew(
                4f,
                1,
                0,
                0,
                ColorType.ColorA,
                NoteType.Note,
                NoteCutDirection.Up,
                0
            );
            var json = new CustomData { [V2_CUTTABLE] = false };

            var typed = (EditorNoodleObjectData)
                new NoodleCustomDataCodec().Deserialize(
                    note,
                    json,
                    new CustomDataCodecContext { SourceVersion = V2, TargetVersion = V2 }
                )!;

            Assert.True(typed.Uninteractable);
        }

        [Fact]
        public void Convert_custom_event_remaps_parent_track_keys()
        {
            var json = new CustomData { ["_parentTrack"] = "p", ["_childrenTracks"] = "c" };
            NoodleCustomDataCodec.ConvertCustomEvent(
                json,
                new CustomDataCodecContext { SourceVersion = V2, TargetVersion = V3 }
            );

            Assert.False(json.ContainsKey("_parentTrack"));
            Assert.True(json.ContainsKey("parentTrack"));
            Assert.True(json.ContainsKey("childrenTracks"));
        }
    }
}
