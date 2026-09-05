using System;
using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Heck.Codecs;
using EditorEX.Heck.Deserialize;
using EditorEX.Heck.ObjectData;
using EditorEX.NoodleExtensions.Codecs;
using EditorEX.NoodleExtensions.ObjectData;
using Heck.Animation;
using Heck.Deserialize;
using Xunit;

namespace EditorEX.Tests.Tests
{
    public class CustomDataCodecRegistryTests
    {
        [Fact]
        public void ConvertJson_runs_object_codecs_and_preserves_unknown_keys()
        {
            var json = new CustomData
            {
                ["_position"] = new List<object> { 0f, 1f },
                ["cinema"] = 1,
            };
            var registry = new CustomDataCodecRegistry(
                new List<IEarlyCustomDataCodec>(),
                new List<IObjectCustomDataCodec> { new NoodleCustomDataCodec() },
                new List<IEventCustomDataCodec>(),
                new List<ICustomEventCustomDataCodec>(),
                new List<IEventListCustomDataCodec>(),
                new EditorDeserializedData(),
                new EditorDeserializedData(),
                new EditorDeserializedData(),
                new EditorDeserializedData(),
                new Dictionary<string, Track>()
            );

            registry.ConvertJson(
                json,
                new CustomDataCodecContext
                {
                    SourceVersion = new Version(2, 6, 0),
                    TargetVersion = new Version(3, 3, 0),
                }
            );

            Assert.True(json.ContainsKey("coordinates"));
            Assert.False(json.ContainsKey("_position"));
            Assert.Equal(1, json["cinema"]);
        }

        [Fact]
        public void DeserializeObject_writes_noodle_typed_data_to_cache()
        {
            var repo = new CustomDataRepository();
            NoteEditorData note = NoteEditorData.CreateNew(
                4f,
                1,
                0,
                0,
                ColorType.ColorA,
                NoteType.Note,
                NoteCutDirection.Up,
                0
            );
            repo.AddCustomData(
                note,
                new CustomData { ["coordinates"] = new List<object> { 1f, 2f } }
            );

            var heck = new EditorDeserializedData();
            var noodle = new EditorDeserializedData();
            var chroma = new EditorDeserializedData();
            var vivify = new EditorDeserializedData();
            var registry = new CustomDataCodecRegistry(
                new List<IEarlyCustomDataCodec>(),
                new List<IObjectCustomDataCodec> { new NoodleCustomDataCodec() },
                new List<IEventCustomDataCodec>(),
                new List<ICustomEventCustomDataCodec>(),
                new List<IEventListCustomDataCodec>(),
                heck,
                noodle,
                chroma,
                vivify,
                new Dictionary<string, Track>()
            );

            registry.DeserializeObject(
                note,
                new CustomDataCodecContext
                {
                    SourceVersion = new Version(3, 3, 0),
                    TargetVersion = new Version(3, 3, 0),
                    Repository = repo,
                }
            );

            Assert.True(noodle.Resolve(note, out EditorNoodleNoteData? typed));
            Assert.Equal(1f, typed!.StartX);
            Assert.Equal(2f, typed.StartY);
            Assert.False(heck.Resolve(note, out IObjectCustomData? _));
        }

        [Fact]
        public void DeserializeObject_skips_when_object_has_no_custom_data()
        {
            var noodle = new EditorDeserializedData();
            var registry = new CustomDataCodecRegistry(
                new List<IEarlyCustomDataCodec>(),
                new List<IObjectCustomDataCodec> { new NoodleCustomDataCodec() },
                new List<IEventCustomDataCodec>(),
                new List<ICustomEventCustomDataCodec>(),
                new List<IEventListCustomDataCodec>(),
                new EditorDeserializedData(),
                noodle,
                new EditorDeserializedData(),
                new EditorDeserializedData(),
                new Dictionary<string, Track>()
            );
            NoteEditorData note = NoteEditorData.CreateNew(
                4f,
                1,
                0,
                0,
                ColorType.ColorA,
                NoteType.Note,
                NoteCutDirection.Up,
                0
            );

            registry.DeserializeObject(
                note,
                new CustomDataCodecContext { Repository = new CustomDataRepository() }
            );

            Assert.False(noodle.Resolve(note, out EditorNoodleNoteData? _));
        }

        [Fact]
        public void Clear_empties_registered_caches()
        {
            var noodle = new EditorDeserializedData();
            var tracks = new Dictionary<string, Track> { ["a"] = new Track() };
            var registry = new CustomDataCodecRegistry(
                new List<IEarlyCustomDataCodec>(),
                new List<IObjectCustomDataCodec>(),
                new List<IEventCustomDataCodec>(),
                new List<ICustomEventCustomDataCodec>(),
                new List<IEventListCustomDataCodec>(),
                new EditorDeserializedData(),
                noodle,
                new EditorDeserializedData(),
                new EditorDeserializedData(),
                tracks
            );
            NoteEditorData note = NoteEditorData.CreateNew(
                4f,
                1,
                0,
                0,
                ColorType.ColorA,
                NoteType.Note,
                NoteCutDirection.Up,
                0
            );
            noodle.SetObject(note, new EditorHeckObjectData(new CustomData(), tracks, false));

            registry.Clear();

            Assert.False(noodle.Resolve(note, out IObjectCustomData? _));
            Assert.Empty(tracks);
        }
    }
}
