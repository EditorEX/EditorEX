using System;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.SpawnProcessing;
using EditorEX.MapData.Contexts;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class RebuildEditorSpawnDataCommandTests : IDisposable
    {
        private readonly Version? _previousVersion;

        public RebuildEditorSpawnDataCommandTests()
        {
            _previousVersion = MapContext.Version;
            EditorSpawnDataRepository.ClearAll();
        }

        public void Dispose()
        {
            EditorSpawnDataRepository.ClearAll();
            MapContext.Version = _previousVersion!;
        }

        [Fact]
        public void Command_rebuilds_dirty_rows_and_is_not_history()
        {
            MapContext.Version = new Version(4, 0, 0);
            var index = new EditorSpawnTimeIndex();
            var tracker = new EditorSpawnDirtyTracker(index);
            NoteEditorData first = EditorSpawnProcessorTests.ColorNote(4f, 1, 0);
            NoteEditorData second = EditorSpawnProcessorTests.ColorNote(8f, 2, 0);
            tracker.NoteAdded(first);
            tracker.NoteAdded(second);
            var processor = new EditorSpawnProcessor(index);
            var command = new RebuildEditorSpawnDataCommand(tracker, processor);
            Assert.DoesNotContain(
                typeof(IBeatmapEditorCommandWithHistory),
                command.GetType().GetInterfaces()
            );
            command.Execute();
            Assert.Equal(4f, EditorSpawnDataRepository.GetSpawnData(first).timeToNextColorNote);
        }

        [Fact]
        public void Command_empty_dirty_is_noop()
        {
            var index = new EditorSpawnTimeIndex();
            var tracker = new EditorSpawnDirtyTracker(index);
            var processor = new EditorSpawnProcessor(index);
            new RebuildEditorSpawnDataCommand(tracker, processor).Execute();
        }
    }
}
