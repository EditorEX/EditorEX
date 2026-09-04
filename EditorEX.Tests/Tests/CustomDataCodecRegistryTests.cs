using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using EditorEX.Heck.Codecs;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.Codecs;
using Heck.Animation;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class CustomDataCodecRegistryTests
    {
        [Fact]
        public void ConvertJson_runs_object_codecs_and_preserves_unknown_keys()
        {
            var json = new CustomData
            {
                ["_position"] = new List<object> { 0f, 1f },
                ["cinema"] = 1,
            };
            var registry = new CustomDataCodecRegistry(
                new List<IEarlyCustomDataCodec>(),
                new List<IObjectCustomDataCodec> { new NoodleCustomDataCodec() },
                new List<IEventCustomDataCodec>(),
                new List<ICustomEventCustomDataCodec>(),
                new List<IEventListCustomDataCodec>(),
                new EditorDeserializedData(),
                new EditorDeserializedData(),
                new EditorDeserializedData(),
                new EditorDeserializedData(),
                new Dictionary<string, Track>()
            );

            registry.ConvertJson(
                json,
                new CustomDataCodecContext
                {
                    SourceVersion = new Version(2, 6, 0),
                    TargetVersion = new Version(3, 3, 0),
                }
            );

            Assert.True(json.ContainsKey("coordinates"));
            Assert.False(json.ContainsKey("_position"));
            Assert.Equal(1, json["cinema"]);
        }
    }
}
