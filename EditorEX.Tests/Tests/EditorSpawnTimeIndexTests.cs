using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using EditorEX.Essentials.SpawnProcessing;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class EditorSpawnTimeIndexTests
    {
        [Fact]
        public void Notes_within_epsilon_share_a_row()
        {
            var index = new EditorSpawnTimeIndex();
            NoteEditorData a = EditorSpawnProcessorTests.ColorNote(4f, 1, 0);
            NoteEditorData b = EditorSpawnProcessorTests.ColorNote(4.0004f, 2, 0);
            index.Add(a);
            index.Add(b);
            Assert.True(
                index.TryGetRow(EditorSpawnTimeIndex.Quantize(4f), out EditorSpawnTimeRow row)
            );
            Assert.Equal(2, row.Notes.Count);
        }

        [Fact]
        public void Slider_occupies_head_and_tail_rows()
        {
            var index = new EditorSpawnTimeIndex();
            ArcEditorData arc = ArcEditorData.CreateNew(
                ColorType.ColorA,
                4f,
                0,
                0,
                0,
                NoteCutDirection.Up,
                1f,
                8f,
                3,
                0,
                0,
                NoteCutDirection.Down,
                1f,
                SliderMidAnchorMode.Straight
            );
            index.Add(arc);
            Assert.True(
                index.TryGetRow(
                    EditorSpawnTimeIndex.Quantize(4f),
                    out EditorSpawnTimeRow head
                )
            );
            Assert.True(
                index.TryGetRow(
                    EditorSpawnTimeIndex.Quantize(8f),
                    out EditorSpawnTimeRow tail
                )
            );
            Assert.Single(head.SliderHeads);
            Assert.Single(tail.SliderTails);
        }

        [Fact]
        public void Previous_and_next_color_note_keys_skip_empty_rows()
        {
            var index = new EditorSpawnTimeIndex();
            index.Add(EditorSpawnProcessorTests.ColorNote(4f, 1, 0));
            index.Add(EditorSpawnProcessorTests.ColorNote(12f, 2, 0));
            int mid = EditorSpawnTimeIndex.Quantize(4f);
            Assert.True(index.TryGetNextColorNoteKey(mid, out int next));
            Assert.Equal(EditorSpawnTimeIndex.Quantize(12f), next);
        }

        [Fact]
        public void Remove_drops_empty_rows()
        {
            var index = new EditorSpawnTimeIndex();
            NoteEditorData note = EditorSpawnProcessorTests.ColorNote(4f, 1, 0);
            index.Add(note);
            index.Remove(note);
            Assert.False(index.TryGetRow(EditorSpawnTimeIndex.Quantize(4f), out _));
        }
    }
}
