using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using EditorEX.Heck.Codecs;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class CustomDataKeyMapperTests
    {
        [Fact]
        public void MoveKey_renames_owned_key_and_keeps_unknown()
        {
            var json = new CustomData
            {
                ["_position"] = new List<object> { 0f, 1f },
                ["cinema"] = new CustomData { ["foo"] = 1 },
            };

            CustomDataKeyMapper.MoveKey(json, "_position", "coordinates");

            Assert.False(json.ContainsKey("_position"));
            Assert.True(json.ContainsKey("coordinates"));
            Assert.True(json.ContainsKey("cinema"));
        }

        [Fact]
        public void MoveKey_noops_when_source_missing()
        {
            var json = new CustomData { ["cinema"] = 1 };
            CustomDataKeyMapper.MoveKey(json, "_position", "coordinates");
            Assert.True(json.ContainsKey("cinema"));
            Assert.False(json.ContainsKey("coordinates"));
        }

        [Fact]
        public void MoveKey_noops_when_from_equals_to()
        {
            var json = new CustomData { ["track"] = "foo" };
            CustomDataKeyMapper.MoveKey(json, "track", "track");
            Assert.Equal("foo", json["track"]);
        }

        [Fact]
        public void RemapNested_converts_known_inner_keys_keeps_unknown_inner()
        {
            var json = new CustomData
            {
                ["_animation"] = new CustomData
                {
                    ["_position"] = new List<object>(),
                    ["mapperExtra"] = 3,
                },
            };

            CustomDataKeyMapper.MoveKey(json, "_animation", "animation");
            CustomDataKeyMapper.RemapNested(
                json,
                "animation",
                new Dictionary<string, string> { ["_position"] = "offsetPosition" }
            );

            var anim = json.Get<CustomData>("animation");
            Assert.NotNull(anim);
            Assert.True(anim!.ContainsKey("offsetPosition"));
            Assert.False(anim.ContainsKey("_position"));
            Assert.Equal(3, anim["mapperExtra"]);
        }

        [Fact]
        public void InvertBoolean_cuttable_false_becomes_uninteractable_true()
        {
            var json = new CustomData { ["_interactable"] = false };
            CustomDataKeyMapper.InvertBoolean(json, "_interactable", "uninteractable", invert: true);
            Assert.False(json.ContainsKey("_interactable"));
            Assert.Equal(true, json["uninteractable"]);
        }
    }
}
