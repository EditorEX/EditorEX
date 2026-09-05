using EditorEX.Heck.Events;
using Heck.Animation;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class HeckTrackPreviewSamplerTests
    {
        [Fact]
        public void Zero_duration_is_complete_at_start()
        {
            float progress = HeckTrackPreviewSampler.EasedProgress(
                beat: 10f,
                fromBeat: 10f,
                durationBeats: 0f,
                repeat: 0,
                Functions.easeLinear,
                out bool complete
            );

            Assert.Equal(1f, progress);
            Assert.True(complete);
        }

        [Fact]
        public void Start_of_interval_is_zero_and_not_complete()
        {
            float progress = HeckTrackPreviewSampler.EasedProgress(
                beat: 4f,
                fromBeat: 4f,
                durationBeats: 8f,
                repeat: 0,
                Functions.easeLinear,
                out bool complete
            );

            Assert.Equal(0f, progress);
            Assert.False(complete);
        }

        [Fact]
        public void Midpoint_is_half_for_linear_easing()
        {
            float progress = HeckTrackPreviewSampler.EasedProgress(
                beat: 8f,
                fromBeat: 4f,
                durationBeats: 8f,
                repeat: 0,
                Functions.easeLinear,
                out bool complete
            );

            Assert.Equal(0.5f, progress);
            Assert.False(complete);
        }

        [Fact]
        public void End_of_duration_latches_complete()
        {
            float progress = HeckTrackPreviewSampler.EasedProgress(
                beat: 12f,
                fromBeat: 4f,
                durationBeats: 8f,
                repeat: 0,
                Functions.easeLinear,
                out bool complete
            );

            Assert.Equal(1f, progress);
            Assert.True(complete);
        }

        [Fact]
        public void After_duration_stays_latched()
        {
            float progress = HeckTrackPreviewSampler.EasedProgress(
                beat: 100f,
                fromBeat: 4f,
                durationBeats: 8f,
                repeat: 0,
                Functions.easeLinear,
                out bool complete
            );

            Assert.Equal(1f, progress);
            Assert.True(complete);
        }

        [Fact]
        public void Repeat_restarts_progress_for_extra_cycles()
        {
            float progress = HeckTrackPreviewSampler.EasedProgress(
                beat: 10f,
                fromBeat: 4f,
                durationBeats: 4f,
                repeat: 1,
                Functions.easeLinear,
                out bool complete
            );

            Assert.Equal(0.5f, progress);
            Assert.False(complete);
        }

        [Fact]
        public void Repeat_boundary_starts_next_cycle_at_zero()
        {
            float progress = HeckTrackPreviewSampler.EasedProgress(
                beat: 8f,
                fromBeat: 4f,
                durationBeats: 4f,
                repeat: 1,
                Functions.easeLinear,
                out bool complete
            );

            Assert.Equal(0f, progress);
            Assert.False(complete);
        }

        [Fact]
        public void All_repeats_finished_latches()
        {
            float progress = HeckTrackPreviewSampler.EasedProgress(
                beat: 12f,
                fromBeat: 4f,
                durationBeats: 4f,
                repeat: 1,
                Functions.easeLinear,
                out bool complete
            );

            Assert.Equal(1f, progress);
            Assert.True(complete);
        }
    }
}
