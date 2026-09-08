using System.Collections.Generic;
using EditorEX.Essentials.Movement;
using Heck.Animation;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class NoodleOffsetPresenceTests
    {
        [Fact]
        public void HasDefinitePosition_is_false_without_animation_or_tracks()
        {
            Assert.False(NoodleOffsetPresence.HasDefinitePosition(null, null));
        }

        [Fact]
        public void HasDefinitePosition_is_false_when_tracks_have_no_path_properties()
        {
            IReadOnlyList<Track> tracks = new[] { new Track() };

            Assert.False(NoodleOffsetPresence.HasDefinitePosition(null, tracks));
        }

        [Fact]
        public void HasDissolve_is_false_without_animation_or_tracks()
        {
            Assert.False(NoodleOffsetPresence.HasDissolve(null, null));
        }

        [Fact]
        public void HasDissolve_is_false_when_tracks_have_no_dissolve_properties()
        {
            IReadOnlyList<Track> tracks = new[] { new Track() };

            Assert.False(NoodleOffsetPresence.HasDissolve(null, tracks));
        }
    }
}
