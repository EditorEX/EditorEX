using System;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using EditorEX.Essentials.SpawnProcessing;
using EditorEX.MapData.Contexts;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class EditorBeatmapObjectsInTimeRowProcessorTests : IDisposable
    {
        private readonly Version? _previousVersion;

        public EditorBeatmapObjectsInTimeRowProcessorTests()
        {
            _previousVersion = MapContext.Version;
            MapContext.Version = new Version(4, 0, 0);
            EditorSpawnDataRepository.ClearAll();
        }

        public void Dispose()
        {
            EditorSpawnDataRepository.ClearAll();
            MapContext.Version = _previousVersion!;
        }

        [Fact]
        public void Consecutive_color_notes_get_prev_and_next_gaps()
        {
            NoteEditorData first = ColorNote(4f, 1, 0);
            NoteEditorData second = ColorNote(8f, 2, 0);
            var processor = new EditorBeatmapObjectsInTimeRowProcessor();

            processor.ProcessNote(first);
            processor.ProcessNote(second);
            processor.ProcessAllRemainingData();

            Assert.Equal(4f, EditorSpawnDataRepository.GetSpawnData(first).timeToNextColorNote);
            Assert.Equal(4f, EditorSpawnDataRepository.GetSpawnData(second).timeToPrevColorNote);
        }

        [Fact]
        public void Stacked_notes_in_a_column_get_ascending_before_jump_layers()
        {
            NoteEditorData lower = ColorNote(4f, 1, 0);
            NoteEditorData upper = ColorNote(4f, 1, 2);
            var processor = new EditorBeatmapObjectsInTimeRowProcessor();

            processor.ProcessNote(lower);
            processor.ProcessNote(upper);
            processor.ProcessAllRemainingData();

            Assert.Equal(
                NoteLineLayer.Base,
                EditorSpawnDataRepository.GetSpawnData(lower).beforeJumpNoteLineLayer
            );
            Assert.Equal(
                NoteLineLayer.Upper,
                EditorSpawnDataRepository.GetSpawnData(upper).beforeJumpNoteLineLayer
            );
        }

        private static NoteEditorData ColorNote(float beat, int column, int row)
        {
            return NoteEditorData.CreateNew(
                beat,
                column,
                row,
                0,
                ColorType.ColorA,
                NoteType.Note,
                NoteCutDirection.Up,
                0
            );
        }
    }
}
