using System.Collections.Generic;
using EditorEX.Essentials.PreviewState;
using EditorEX.NoodleExtensions.Events;
using NoodleExtensions.Managers;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class AssignPlayerToTrackOwnershipTests
    {
        [Fact]
        public void Same_player_object_conflicts()
        {
            Assert.True(
                AssignPlayerToTrackOwnership.Conflicts(PlayerObject.Root, PlayerObject.Root)
            );
        }

        [Fact]
        public void Different_player_objects_do_not_conflict()
        {
            Assert.False(
                AssignPlayerToTrackOwnership.Conflicts(PlayerObject.Root, PlayerObject.Head)
            );
        }

        [Fact]
        public void Later_root_assignment_ends_the_earlier_root_interval()
        {
            var items = new List<(float Beat, PlayerObject Target)>
            {
                (0f, PlayerObject.Root),
                (4f, PlayerObject.Head),
                (8f, PlayerObject.Root),
            };

            float ExclusiveEnd(int index) =>
                PreviewStateOwnership.NextExclusiveEnd(
                    items,
                    index,
                    item => item.Beat,
                    (left, right) => AssignPlayerToTrackOwnership.Conflicts(left.Target, right.Target)
                );

            Assert.Equal(8f, ExclusiveEnd(0));
            Assert.Equal(float.MaxValue, ExclusiveEnd(1));
        }
    }
}
