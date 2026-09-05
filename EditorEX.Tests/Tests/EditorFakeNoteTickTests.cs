using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using CustomJSONData.CustomBeatmap;
using EditorEX.Heck.Deserialize;
using EditorEX.NoodleExtensions.ObjectData;
using EditorEX.NoodleExtensions.Patches;
using Heck.Animation;
using NoodleExtensions;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class EditorFakeNoteTickTests
    {
        [Fact]
        public void Frame_with_only_a_fake_note_does_not_tick()
        {
            NoteEditorData note = ColorNote(column: 1);
            EditorDeserializedData noodle = WithFake(note, fake: true);
            BeatmapObjectsFrameDataContainer frame = FrameWith(note);

            Assert.False(EditorFakeNoteTick.FrameHasTickableNote(frame, noodle));
        }

        [Fact]
        public void Frame_with_a_real_note_ticks()
        {
            NoteEditorData note = ColorNote(column: 1);
            EditorDeserializedData noodle = WithFake(note, fake: false);
            BeatmapObjectsFrameDataContainer frame = FrameWith(note);

            Assert.True(EditorFakeNoteTick.FrameHasTickableNote(frame, noodle));
        }

        [Fact]
        public void Frame_with_fake_and_real_notes_ticks()
        {
            NoteEditorData fake = ColorNote(column: 0);
            NoteEditorData real = ColorNote(column: 2);
            var noodle = new EditorDeserializedData();
            noodle.RegisterNewObject(fake, NoodleData(fake, fake: true));
            noodle.RegisterNewObject(real, NoodleData(real, fake: false));
            var frame = new BeatmapObjectsFrameDataContainer(4f, 4, 3);
            frame.AddToGrid(fake, fake.column, fake.row);
            frame.AddToGrid(real, real.column, real.row);

            Assert.True(EditorFakeNoteTick.FrameHasTickableNote(frame, noodle));
        }

        [Fact]
        public void Frame_with_a_note_and_no_noodle_data_ticks()
        {
            NoteEditorData note = ColorNote(column: 1);
            var frame = FrameWith(note);

            Assert.True(EditorFakeNoteTick.FrameHasTickableNote(frame, noodle: null));
        }

        private static NoteEditorData ColorNote(int column)
        {
            return NoteEditorData.CreateNew(
                4f,
                column,
                0,
                0,
                ColorType.ColorA,
                NoteType.Note,
                NoteCutDirection.Up,
                0
            );
        }

        private static BeatmapObjectsFrameDataContainer FrameWith(NoteEditorData note)
        {
            var frame = new BeatmapObjectsFrameDataContainer(note.beat, 4, 3);
            frame.AddToGrid(note, note.column, note.row);
            return frame;
        }

        private static EditorDeserializedData WithFake(NoteEditorData note, bool fake)
        {
            var noodle = new EditorDeserializedData();
            noodle.RegisterNewObject(note, NoodleData(note, fake));
            return noodle;
        }

        private static EditorNoodleObjectData NoodleData(NoteEditorData note, bool fake)
        {
            return new EditorNoodleObjectData(
                note,
                new CustomData { [NoodleController.INTERNAL_FAKE_NOTE] = fake },
                new Dictionary<string, List<object>>(),
                new Dictionary<string, Track>(),
                v2: false,
                leftHanded: false
            );
        }
    }
}
