using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Heck.Codecs;
using EditorEX.Vivify.Codecs;
using Heck.Deserialize;
using Vivify;
using Xunit;
using static Vivify.VivifyController;

namespace EditorEX.Tests.Tests
{
    public class VivifyCustomDataCodecTests
    {
        [Fact]
        public void Convert_is_a_noop()
        {
            var json = new CustomData { ["asset"] = "bundle.prefab" };
            new VivifyCustomDataCodec().Convert(json, new CustomDataCodecContext());
            Assert.Equal("bundle.prefab", json["asset"]);
        }

        [Fact]
        public void Deserialize_unknown_event_is_null()
        {
            CustomEventEditorData evt = CustomEventEditorData.CreateNew(
                1f,
                "NotAVivifyEvent",
                new CustomData(),
                false
            );

            ICustomEventCustomData? typed = new VivifyCustomDataCodec().Deserialize(
                evt,
                evt.customData,
                new CustomDataCodecContext()
            );

            Assert.Null(typed);
        }

        [Fact]
        public void Deserialize_instantiate_prefab_returns_typed_data()
        {
            CustomEventEditorData evt = CustomEventEditorData.CreateNew(
                2f,
                INSTANTIATE_PREFAB,
                new CustomData { ["asset"] = "prefab", ["id"] = "one" },
                false
            );

            ICustomEventCustomData? typed = new VivifyCustomDataCodec().Deserialize(
                evt,
                evt.customData,
                new CustomDataCodecContext()
            );

            Assert.IsType<InstantiatePrefabData>(typed);
        }

        [Fact]
        public void Deserialize_object_returns_vivify_object_data()
        {
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

            IObjectCustomData? typed = new VivifyCustomDataCodec().Deserialize(
                note,
                new CustomData(),
                new CustomDataCodecContext()
            );

            Assert.NotNull(typed);
        }
    }
}
