using System.Collections.Generic;
using EditorEX.Heck.EventData;
using EditorEX.Heck.Events;
using Heck.Animation;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class HeckTrackPreviewActionTests
    {
        [Fact]
        public void After_duration_later_ticks_do_not_rewrite_the_latched_value()
        {
            (HeckTrackPreviewAction action, Property<float> property) = CreateAnimate(
                fromBeat: 4f,
                durationBeats: 8f,
                repeat: 0,
                Points(0f, 0f, 1f, 1f)
            );

            action.Execute();
            action.Tick(12f);
            Assert.Equal(1f, property.Value);

            property.Value = 999f;
            action.Tick(20f);

            Assert.Equal(999f, property.Value);
        }

        [Fact]
        public void Rewind_into_duration_resamples()
        {
            (HeckTrackPreviewAction action, Property<float> property) = CreateAnimate(
                fromBeat: 4f,
                durationBeats: 8f,
                repeat: 0,
                Points(0f, 0f, 1f, 1f)
            );

            action.Execute();
            action.Tick(20f);
            Assert.Equal(1f, property.Value);

            action.Tick(8f);

            Assert.Equal(0.5f, property.Value);
        }

        [Fact]
        public void Mid_duration_ticks_still_update()
        {
            (HeckTrackPreviewAction action, Property<float> property) = CreateAnimate(
                fromBeat: 4f,
                durationBeats: 8f,
                repeat: 0,
                Points(0f, 0f, 1f, 1f)
            );

            action.Execute();
            action.Tick(6f);
            Assert.Equal(0.25f, property.Value);

            action.Tick(8f);
            Assert.Equal(0.5f, property.Value);
        }

        [Fact]
        public void Reverse_clears_latch_so_a_later_execute_can_sample_again()
        {
            (HeckTrackPreviewAction action, Property<float> property) = CreateAnimate(
                fromBeat: 4f,
                durationBeats: 8f,
                repeat: 0,
                Points(0f, 0f, 1f, 1f)
            );

            action.Execute();
            action.Tick(20f);
            action.Reverse();
            action.Execute();
            action.Tick(8f);

            Assert.Equal(0.5f, property.Value);
        }

        [Fact]
        public void Last_keyframe_before_duration_end_latches_on_the_last_repeat()
        {
            (HeckTrackPreviewAction action, Property<float> property) = CreateAnimate(
                fromBeat: 4f,
                durationBeats: 8f,
                repeat: 0,
                Points(0f, 0f, 1f, 0.5f)
            );

            action.Execute();
            action.Tick(8f);
            Assert.Equal(1f, property.Value);

            property.Value = 999f;
            action.Tick(10f);

            Assert.Equal(999f, property.Value);
        }

        [Fact]
        public void Repeat_cycle_is_not_latched_by_the_previous_cycle_end()
        {
            (HeckTrackPreviewAction action, Property<float> property) = CreateAnimate(
                fromBeat: 4f,
                durationBeats: 4f,
                repeat: 1,
                Points(0f, 0f, 1f, 1f)
            );

            action.Execute();
            action.Tick(7.99f);
            action.Tick(10f);

            Assert.Equal(0.5f, property.Value);
        }

        [Fact]
        public void After_duration_WantsTick_is_false_at_a_later_beat()
        {
            (HeckTrackPreviewAction action, _) = CreateAnimate(
                fromBeat: 4f,
                durationBeats: 8f,
                repeat: 0,
                Points(0f, 0f, 1f, 1f)
            );

            action.Execute();
            action.Tick(12f);

            Assert.False(action.WantsTick(20f));
        }

        [Fact]
        public void After_duration_WantsTick_is_true_when_rewinding_into_duration()
        {
            (HeckTrackPreviewAction action, _) = CreateAnimate(
                fromBeat: 4f,
                durationBeats: 8f,
                repeat: 0,
                Points(0f, 0f, 1f, 1f)
            );

            action.Execute();
            action.Tick(12f);

            Assert.True(action.WantsTick(8f));
        }

        [Fact]
        public void Path_rewind_into_duration_resamples_time()
        {
            (HeckTrackPreviewAction action, PointDefinitionInterpolation<float> interpolation) =
                CreatePath(fromBeat: 4f, durationBeats: 8f, Points(0f, 0f, 1f, 1f));

            action.Execute();
            action.Tick(20f);
            Assert.Equal(1f, interpolation.Time);

            interpolation.Time = 0.25f;
            action.Tick(30f);
            Assert.Equal(0.25f, interpolation.Time);

            action.Tick(8f);
            Assert.Equal(0.5f, interpolation.Time);
        }

        private static (
            HeckTrackPreviewAction Action,
            Property<float> Property
        ) CreateAnimate(float fromBeat, float durationBeats, int repeat, IPointDefinition points)
        {
            var track = new Track();
            var builder = new PropertyBuilder<float>();
            var property = (Property<float>)track.GetOrCreateProperty("p", builder);
            var info = new EditorCoroutineEventData.CoroutineInfo(points, property, track);
            var action = new HeckTrackPreviewAction(
                info,
                fromBeat,
                durationBeats,
                repeat,
                Functions.easeLinear,
                path: false
            );
            return (action, property);
        }

        private static (
            HeckTrackPreviewAction Action,
            PointDefinitionInterpolation<float> Interpolation
        ) CreatePath(float fromBeat, float durationBeats, IPointDefinition points)
        {
            var track = new Track();
            var builder = new PropertyBuilder<float>();
            var property = (PathProperty<float>)track.GetOrCreatePathProperty("p", builder);
            var info = new EditorCoroutineEventData.CoroutineInfo(points, property, track);
            var action = new HeckTrackPreviewAction(
                info,
                fromBeat,
                durationBeats,
                repeat: 0,
                Functions.easeLinear,
                path: true
            );
            return (action, property.Interpolation);
        }

        private static FloatPointDefinition Points(
            float value0,
            float time0,
            float value1,
            float time1
        )
        {
            return new FloatPointDefinition(
                new List<object>
                {
                    new List<object> { value0, time0 },
                    new List<object> { value1, time1 },
                }
            );
        }
    }
}
