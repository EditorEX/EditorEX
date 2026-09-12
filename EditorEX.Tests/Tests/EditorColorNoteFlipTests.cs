using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using EditorEX.Essentials.SpawnProcessing;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class EditorColorNoteFlipTests
    {
        public EditorColorNoteFlipTests()
        {
            EditorSpawnDataRepository.ClearAll();
        }

        [Fact]
        public void Apply_does_not_throw_when_notes_have_no_custom_data()
        {
            NoteEditorData colorA = ColorNote(column: 3, ColorType.ColorA);
            NoteEditorData colorB = ColorNote(column: 1, ColorType.ColorB);

            var exception = Record.Exception(() =>
                EditorColorNoteFlip.Apply(
                    new[] { colorA, colorB },
                    n => (n.column, (float)n.row)
                )
            );

            Assert.Null(exception);
        }

        [Fact]
        public void Apply_writes_flip_scratch_when_crossover_notes_lack_custom_data()
        {
            NoteEditorData colorA = ColorNote(column: 3, ColorType.ColorA);
            NoteEditorData colorB = ColorNote(column: 1, ColorType.ColorB);

            EditorColorNoteFlip.Apply(new[] { colorA, colorB }, n => (n.column, (float)n.row));

            Assert.Equal(1f, EditorSpawnDataRepository.GetSpawnData(colorA).flipLineIndex);
            Assert.Equal(3f, EditorSpawnDataRepository.GetSpawnData(colorB).flipLineIndex);
        }

        private static NoteEditorData ColorNote(int column, ColorType color)
        {
            return NoteEditorData.CreateNew(
                4f,
                column,
                0,
                0,
                color,
                NoteType.Note,
                NoteCutDirection.Up,
                0
            );
        }
    }
}
