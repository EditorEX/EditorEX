using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using EditorEX.Heck.Codecs;
using EditorEX.NoodleExtensions.Codecs;
using Xunit;

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
    }
}
