using System.Collections.Generic;
using EditorEX.Heck.Events;
using Heck.Animation;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class HeckTrackPreviewPathInitTests
    {
        [Fact]
        public void Inits_previous_then_current_so_handoff_can_blend()
        {
            var interpolation = new RecordingInterpolation();
            var previous = new DummyPointDefinition();
            var current = new DummyPointDefinition();

            HeckTrackPreviewPathInit.Apply(interpolation, previous, current);

            Assert.Equal(new IPointDefinition[] { previous, current }, interpolation.Inits);
        }

        [Fact]
        public void Skips_previous_when_this_is_the_first_path()
        {
            var interpolation = new RecordingInterpolation();
            var current = new DummyPointDefinition();

            HeckTrackPreviewPathInit.Apply(interpolation, previous: null, current);

            Assert.Equal(new IPointDefinition?[] { current }, interpolation.Inits);
        }

        [Fact]
        public void Nulls_when_current_point_definition_is_missing()
        {
            var interpolation = new RecordingInterpolation();
            var previous = new DummyPointDefinition();

            HeckTrackPreviewPathInit.Apply(interpolation, previous, current: null);

            Assert.Equal(new IPointDefinition?[] { null }, interpolation.Inits);
        }

        private sealed class DummyPointDefinition : IPointDefinition
        {
            public int Count => 0;

            public bool HasBaseProvider => false;
        }

        private sealed class RecordingInterpolation : IPointDefinitionInterpolation
        {
            public List<IPointDefinition?> Inits { get; } = new();

            public float Time { get; set; }

            public void Finish() { }

            public void Init(IPointDefinition? pointDefinition)
            {
                Inits.Add(pointDefinition);
            }
        }
    }
}
