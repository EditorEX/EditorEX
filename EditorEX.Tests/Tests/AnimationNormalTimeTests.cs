using EditorEX.Essentials.Movement;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class AnimationNormalTimeTests
    {
        [Fact]
        public void TryCompute_rejects_zero_jump_duration_at_the_note()
        {
            // Setup used _jump.jumpDuration before Jump.ManualUpdate, which is 0.
            // elapsed 0 / duration 0 is NaN and Heck PointDefinition.Interpolate throws.
            bool ok = AnimationNormalTime.TryCompute(
                noodleTimeProperty: null,
                playheadSeconds: 10f,
                noteSeconds: 10f,
                jumpDuration: 0f,
                out float normalTime
            );

            Assert.False(ok);
            Assert.Equal(0f, normalTime);
        }

        [Fact]
        public void TryCompute_uses_variable_movement_duration()
        {
            bool ok = AnimationNormalTime.TryCompute(
                noodleTimeProperty: null,
                playheadSeconds: 10.5f,
                noteSeconds: 10f,
                jumpDuration: 2f,
                out float normalTime
            );

            Assert.True(ok);
            Assert.Equal(0.75f, normalTime, 3);
        }

        [Fact]
        public void TryCompute_rejects_nan_noodle_time_property()
        {
            bool ok = AnimationNormalTime.TryCompute(
                noodleTimeProperty: float.NaN,
                playheadSeconds: 10f,
                noteSeconds: 10f,
                jumpDuration: 2f,
                out _
            );

            Assert.False(ok);
        }
    }
}
