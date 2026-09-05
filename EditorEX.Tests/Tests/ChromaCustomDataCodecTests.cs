using System;
using CustomJSONData.CustomBeatmap;
using EditorEX.Chroma.Codecs;
using EditorEX.Heck.Codecs;
using Heck.Deserialize;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class ChromaCustomDataCodecTests
    {
        private static readonly Version V2 = new(2, 6, 0);
        private static readonly Version V3 = new(3, 3, 0);

        [Fact]
        public void Convert_v2_to_v3_remaps_color_and_keeps_unknown()
        {
            var json = new CustomData { ["_color"] = new[] { 1f, 0f, 0f }, ["cinema"] = 1 };
            new ChromaCustomDataCodec().Convert(
                json,
                new CustomDataCodecContext { SourceVersion = V2, TargetVersion = V3 }
            );

            Assert.False(json.ContainsKey("_color"));
            Assert.True(json.ContainsKey("color"));
            Assert.Equal(1, json["cinema"]);
        }

        [Fact]
        public void Convert_same_version_does_not_rewrite_keys()
        {
            var json = new CustomData { ["_color"] = 1 };
            new ChromaCustomDataCodec().Convert(
                json,
                new CustomDataCodecContext { SourceVersion = V2, TargetVersion = V2 }
            );
            Assert.True(json.ContainsKey("_color"));
            Assert.False(json.ContainsKey("color"));
        }

        [Fact]
        public void Deserialize_unknown_custom_event_is_null()
        {
            var evt = EditorEX.CustomJSONData.CustomEvents.CustomEventEditorData.CreateNew(
                1f,
                "NotAChromaEvent",
                new CustomData(),
                false
            );

            ICustomEventCustomData? typed = new ChromaCustomDataCodec().Deserialize(
                evt,
                evt.customData,
                new CustomDataCodecContext { SourceVersion = V3, TargetVersion = V3 }
            );

            Assert.Null(typed);
        }
    }
}
