using EditorEX.Chroma.Events;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class ChromaAnimateComponentPreviewTests
    {
        [Fact]
        public void Same_track_component_and_property_conflict()
        {
            var track = new object();

            Assert.True(
                ChromaAnimateComponentOwnership.Conflicts(
                    track,
                    "BloomFogEnvironment",
                    "attenuation",
                    track,
                    "BloomFogEnvironment",
                    "attenuation"
                )
            );
        }

        [Fact]
        public void Different_property_on_same_component_does_not_conflict()
        {
            var track = new object();

            Assert.False(
                ChromaAnimateComponentOwnership.Conflicts(
                    track,
                    "BloomFogEnvironment",
                    "attenuation",
                    track,
                    "BloomFogEnvironment",
                    "offset"
                )
            );
        }

        [Fact]
        public void Fog_channel_uses_original_when_track_property_is_null()
        {
            Assert.Equal(0.5f, ChromaFogPreview.Channel(null, 0.5f));
            Assert.Equal(0.2f, ChromaFogPreview.Channel(0.2f, 0.5f));
        }
    }
}
