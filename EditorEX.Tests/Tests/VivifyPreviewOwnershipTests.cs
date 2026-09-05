using EditorEX.Vivify.Events;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class VivifyPreviewOwnershipTests
    {
        [Fact]
        public void Same_id_conflicts()
        {
            Assert.True(VivifyPreviewOwnership.Conflicts(["cube"], ["cube"]));
        }

        [Fact]
        public void Different_ids_do_not_conflict()
        {
            Assert.False(VivifyPreviewOwnership.Conflicts(["cube"], ["wall"]));
        }

        [Fact]
        public void Destroy_array_conflicts_when_it_contains_the_id()
        {
            Assert.True(VivifyPreviewOwnership.Conflicts(["cube"], ["wall", "cube"]));
        }

        [Fact]
        public void Anonymous_empty_ids_do_not_conflict()
        {
            Assert.False(VivifyPreviewOwnership.Conflicts([], ["cube"]));
            Assert.False(VivifyPreviewOwnership.Conflicts([], []));
        }

        [Fact]
        public void Post_processing_end_is_from_plus_duration()
        {
            Assert.True(
                VivifyPreviewOwnership.TryPostProcessingExclusiveEnd(8f, 2f, out float to)
            );
            Assert.Equal(10f, to);
        }

        [Fact]
        public void Duration_zero_post_processing_is_not_registered()
        {
            Assert.False(
                VivifyPreviewOwnership.TryPostProcessingExclusiveEnd(4f, 0f, out float to)
            );
            Assert.Equal(4f, to);
        }
    }
}
