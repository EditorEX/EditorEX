using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Essentials.SpawnProcessing;
using Xunit;
using static NoodleExtensions.NoodleController;

namespace EditorEX.Tests.Tests
{
    public class EditorColorNoteFlipTests
    {
        [Fact]
        public void Apply_does_not_throw_when_notes_have_no_custom_data()
        {
            var repo = new CustomDataRepository();
            NoteEditorData colorA = ColorNote(column: 3, ColorType.ColorA);
            NoteEditorData colorB = ColorNote(column: 1, ColorType.ColorB);

            var exception = Record.Exception(() =>
                EditorColorNoteFlip.Apply(new[] { colorA, colorB }, repo, v2: false)
            );

            Assert.Null(exception);
        }

        [Fact]
        public void Apply_writes_flip_scratch_when_crossover_notes_lack_custom_data()
        {
            var repo = new CustomDataRepository();
            NoteEditorData colorA = ColorNote(column: 3, ColorType.ColorA);
            NoteEditorData colorB = ColorNote(column: 1, ColorType.ColorB);

            EditorColorNoteFlip.Apply(new[] { colorA, colorB }, repo, v2: false);

            CustomData aData = repo.GetCustomData(colorA);
            CustomData bData = repo.GetCustomData(colorB);
            Assert.NotNull(aData);
            Assert.NotNull(bData);
            Assert.Equal(1f, aData.Get<float?>(INTERNAL_FLIPLINEINDEX));
            Assert.Equal(3f, bData.Get<float?>(INTERNAL_FLIPLINEINDEX));
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
