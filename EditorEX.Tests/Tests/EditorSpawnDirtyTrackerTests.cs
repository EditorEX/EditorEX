using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.SpawnProcessing;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class EditorSpawnDirtyTrackerTests
    {
        [Fact]
        public void Adding_a_note_dirties_its_row_and_color_neighbors()
        {
            var index = new EditorSpawnTimeIndex();
            var tracker = new EditorSpawnDirtyTracker(index);
            tracker.NoteAdded(EditorSpawnProcessorTests.ColorNote(4f, 1, 0));
            tracker.NoteAdded(EditorSpawnProcessorTests.ColorNote(8f, 2, 0));
            HashSet<int> dirty = tracker.SwapDirtyKeys();
            Assert.Contains(EditorSpawnTimeIndex.Quantize(4f), dirty);
            Assert.Contains(EditorSpawnTimeIndex.Quantize(8f), dirty);
            Assert.Empty(tracker.SwapDirtyKeys());
        }
    }
}
