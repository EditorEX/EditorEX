using System;
using System.Collections.Generic;
using EditorEX.Essentials.PreviewState;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class PreviewStateOwnershipTests
    {
        [Fact]
        public void NextExclusiveEnd_returns_beat_of_next_conflict()
        {
            var items = new List<(float Beat, string Id)>
            {
                (0f, "a"),
                (5f, "b"),
                (10f, "c"),
            };

            float end = PreviewStateOwnership.NextExclusiveEnd(
                items,
                0,
                item => item.Beat,
                (left, right) => left.Id == "a" && right.Id == "b"
            );

            Assert.Equal(5f, end);
        }

        [Fact]
        public void NextExclusiveEnd_skips_non_conflicting_later_items()
        {
            var items = new List<(float Beat, string Id)>
            {
                (0f, "a"),
                (5f, "other"),
                (10f, "a2"),
            };

            float end = PreviewStateOwnership.NextExclusiveEnd(
                items,
                0,
                item => item.Beat,
                (left, right) => left.Id[0] == right.Id[0]
            );

            Assert.Equal(10f, end);
        }

        [Fact]
        public void NextExclusiveEnd_is_max_value_when_nothing_conflicts()
        {
            var items = new List<(float Beat, string Id)> { (0f, "a"), (5f, "b") };

            float end = PreviewStateOwnership.NextExclusiveEnd(
                items,
                0,
                item => item.Beat,
                (_, _) => false
            );

            Assert.Equal(float.MaxValue, end);
        }

        [Fact]
        public void NextExclusiveEnd_same_beat_conflict_returns_that_beat()
        {
            var items = new List<(float Beat, string Id)> { (10f, "first"), (10f, "second") };

            float end = PreviewStateOwnership.NextExclusiveEnd(
                items,
                0,
                item => item.Beat,
                (_, _) => true
            );

            Assert.Equal(10f, end);
        }

        [Fact]
        public void PreviousConflicting_returns_nearest_earlier_match()
        {
            var items = new List<(float Beat, string Id, string Value)>
            {
                (0f, "a", "first"),
                (5f, "other", "skip"),
                (10f, "a", "second"),
            };

            string? previous = PreviewStateOwnership.PreviousConflicting(
                items,
                2,
                (left, right) => left.Id == right.Id,
                item => item.Value
            );

            Assert.Equal("first", previous);
        }

        [Fact]
        public void PreviousConflicting_is_null_when_nothing_conflicts()
        {
            var items = new List<(float Beat, string Id, string Value)>
            {
                (0f, "a", "first"),
                (10f, "b", "second"),
            };

            string? previous = PreviewStateOwnership.PreviousConflicting(
                items,
                1,
                (left, right) => left.Id == right.Id,
                item => item.Value
            );

            Assert.Null(previous);
        }
    }
}
