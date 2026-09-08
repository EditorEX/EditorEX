using System.Collections.Generic;
using EditorEX.NoodleExtensions.Events;
using NoodleExtensions.Animation;
using NoodleExtensions.Managers;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class EditorAssignPlayerToTrackTests
    {
        [Fact]
        public void Clear_does_not_throw_when_the_player_track_is_gone()
        {
            EditorAssignPlayerToTrack.Clear(null);
        }

        [Fact]
        public void TryGetAlive_drops_a_destroyed_player_track()
        {
            var playerTracks = new Dictionary<PlayerObject, PlayerTrack>
            {
                [PlayerObject.Root] = null!,
            };

            Assert.False(
                EditorAssignPlayerToTrack.TryGetAlive(
                    playerTracks,
                    PlayerObject.Root,
                    out PlayerTrack? playerTrack
                )
            );
            Assert.Null(playerTrack);
            Assert.Empty(playerTracks);
        }
    }
}
