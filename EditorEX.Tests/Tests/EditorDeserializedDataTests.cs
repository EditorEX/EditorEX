using System;
using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using CustomJSONData.CustomBeatmap;
using EditorEX.Heck.Deserialize;
using EditorEX.Heck.ObjectData;
using EditorEX.NoodleExtensions.ObjectData;
using Heck.Animation;
using Heck.Deserialize;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class EditorDeserializedDataTests
    {
        [Fact]
        public void Resolve_object_returns_registered_typed_data()
        {
            var cache = new EditorDeserializedData();
            NoteEditorData note = ColorNote();
            var typed = new EditorHeckObjectData(
                new CustomData(),
                new Dictionary<string, Track>(),
                false
            );
            cache.SetObject(note, typed);

            Assert.True(cache.Resolve(note, out EditorHeckObjectData? resolved));
            Assert.Same(typed, resolved);
        }

        [Fact]
        public void Resolve_null_object_is_false()
        {
            var cache = new EditorDeserializedData();
            Assert.False(cache.Resolve<EditorHeckObjectData>(null, out _));
        }

        [Fact]
        public void Resolve_wrong_type_throws()
        {
            var cache = new EditorDeserializedData();
            NoteEditorData note = ColorNote();
            cache.SetObject(
                note,
                new EditorHeckObjectData(new CustomData(), new Dictionary<string, Track>(), false)
            );

            Assert.Throws<InvalidOperationException>(() =>
                cache.Resolve(note, out EditorNoodleNoteData? _)
            );
        }

        [Fact]
        public void Resolve_event_matches_by_id_not_instance()
        {
            var cache = new EditorDeserializedData();
            BasicEventEditorData original = BasicEventEditorData.CreateNew(
                BasicBeatmapEventType.Event0,
                4f,
                1,
                0f
            );
            var typed = new DummyEventData();
            cache.SetEvent(original, typed);

            BasicEventEditorData copy = BasicEventEditorData.CreateNewWithId(
                original.id,
                BasicBeatmapEventType.Event0,
                4f,
                1,
                0f
            );

            Assert.True(cache.Resolve(copy, out DummyEventData? resolved));
            Assert.Same(typed, resolved);
        }

        [Fact]
        public void Clear_drops_object_and_event_entries()
        {
            var cache = new EditorDeserializedData();
            NoteEditorData note = ColorNote();
            cache.SetObject(
                note,
                new EditorHeckObjectData(new CustomData(), new Dictionary<string, Track>(), false)
            );
            cache.Clear();
            Assert.False(cache.Resolve(note, out EditorHeckObjectData? _));
        }

        private static NoteEditorData ColorNote()
        {
            return NoteEditorData.CreateNew(
                4f,
                1,
                0,
                0,
                ColorType.ColorA,
                NoteType.Note,
                NoteCutDirection.Up,
                0
            );
        }

        private sealed class DummyEventData : IEventCustomData { }
    }
}
