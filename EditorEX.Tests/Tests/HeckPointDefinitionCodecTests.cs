using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using EditorEX.Heck.Codecs;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class HeckPointDefinitionCodecTests
    {
        [Fact]
        public void ConvertPointDefinitions_v2_to_v3_keeps_sibling_unknown_keys()
        {
            var beatmap = new CustomData
            {
                ["_pointDefinitions"] = new List<object>
                {
                    new CustomData { ["_name"] = "p", ["_points"] = new List<object>() },
                },
                ["bookmarks"] = 1,
                ["environment"] = 2,
            };

            new HeckCustomDataCodec().ConvertPointDefinitions(
                beatmap,
                new CustomDataCodecContext
                {
                    SourceVersion = new Version(2, 6, 0),
                    TargetVersion = new Version(3, 3, 0),
                }
            );

            Assert.False(beatmap.ContainsKey("_pointDefinitions"));
            Assert.True(beatmap.ContainsKey("pointDefinitions"));
            Assert.Equal(1, beatmap["bookmarks"]);
            Assert.Equal(2, beatmap["environment"]);
        }
    }
}
