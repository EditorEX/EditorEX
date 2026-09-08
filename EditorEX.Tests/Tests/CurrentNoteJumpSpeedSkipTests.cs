using EditorEX.Essentials.Patches;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class CurrentNoteJumpSpeedSkipTests
    {
        [Fact]
        public void CanSkip_is_false_when_the_map_has_njs_events()
        {
            Assert.False(
                CurrentNoteJumpSpeedSkip.CanSkipSignalFire(
                    njsEventFrameCount: 1,
                    stateNjs: 16f,
                    stateHalfJumpDurationInBeats: 2f,
                    stateOneBeat: 0.5f,
                    baseNjs: 16f,
                    halfJumpDurationInBeats: 2f,
                    oneBeat: 0.5f
                )
            );
        }

        [Fact]
        public void CanSkip_is_false_until_static_njs_has_been_applied()
        {
            Assert.False(
                CurrentNoteJumpSpeedSkip.CanSkipSignalFire(
                    njsEventFrameCount: 0,
                    stateNjs: 0f,
                    stateHalfJumpDurationInBeats: 0f,
                    stateOneBeat: 0f,
                    baseNjs: 16f,
                    halfJumpDurationInBeats: 2f,
                    oneBeat: 0.5f
                )
            );
        }

        [Fact]
        public void CanSkip_is_true_when_static_njs_already_matches_state()
        {
            Assert.True(
                CurrentNoteJumpSpeedSkip.CanSkipSignalFire(
                    njsEventFrameCount: 0,
                    stateNjs: 16f,
                    stateHalfJumpDurationInBeats: 2f,
                    stateOneBeat: 0.5f,
                    baseNjs: 16f,
                    halfJumpDurationInBeats: 2f,
                    oneBeat: 0.5f
                )
            );
        }

        [Fact]
        public void CanSkip_is_false_when_static_njs_settings_changed()
        {
            Assert.False(
                CurrentNoteJumpSpeedSkip.CanSkipSignalFire(
                    njsEventFrameCount: 0,
                    stateNjs: 16f,
                    stateHalfJumpDurationInBeats: 2f,
                    stateOneBeat: 0.5f,
                    baseNjs: 18f,
                    halfJumpDurationInBeats: 2f,
                    oneBeat: 0.5f
                )
            );
        }

        [Fact]
        public void CanSkip_uses_the_same_minimum_njs_as_vanilla()
        {
            Assert.True(
                CurrentNoteJumpSpeedSkip.CanSkipSignalFire(
                    njsEventFrameCount: 0,
                    stateNjs: 0.01f,
                    stateHalfJumpDurationInBeats: 2f,
                    stateOneBeat: 0.5f,
                    baseNjs: 0f,
                    halfJumpDurationInBeats: 2f,
                    oneBeat: 0.5f
                )
            );
        }
    }
}
