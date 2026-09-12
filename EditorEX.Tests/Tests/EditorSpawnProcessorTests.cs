using System;
using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using CustomJSONData.CustomBeatmap;
using EditorEX.Essentials.SpawnProcessing;
using EditorEX.Heck.Codecs;
using EditorEX.Heck.Deserialize;
using EditorEX.MapData.Contexts;
using EditorEX.NoodleExtensions.Codecs;
using EditorEX.NoodleExtensions.ObjectData;
using Xunit;
using static NoodleExtensions.NoodleController;

namespace EditorEX.Tests.Tests
{
    public class EditorSpawnProcessorTests : IDisposable
    {
        private readonly Version? _previousVersion;

        public EditorSpawnProcessorTests()
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
        public void GetSpawnData_applies_identity_defaults()
        {
            NoteEditorData note = ColorNote(4f, column: 2, row: 1);
            EditorObjectSpawnData spawn = EditorSpawnDataRepository.GetSpawnData(note);
            Assert.Equal(2f, spawn.flipLineIndex);
            Assert.Equal(NoteLineLayer.Upper, spawn.beforeJumpNoteLineLayer);
            Assert.Equal(1f, spawn.startNoteLineLayer);
            Assert.Equal(0f, spawn.flipYSide);
            Assert.Equal(float.MaxValue, spawn.timeToNextColorNote);
        }

        [Fact]
        public void Consecutive_color_notes_get_prev_and_next_gaps()
        {
            MapContext.Version = new Version(4, 0, 0);
            var index = new EditorSpawnTimeIndex();
            NoteEditorData first = ColorNote(4f, 1, 0);
            NoteEditorData second = ColorNote(8f, 2, 0);
            index.Add(first);
            index.Add(second);
            var processor = new EditorSpawnProcessor(index);
            processor.RebuildAll();
            Assert.Equal(4f, EditorSpawnDataRepository.GetSpawnData(first).timeToNextColorNote);
            Assert.Equal(4f, EditorSpawnDataRepository.GetSpawnData(second).timeToPrevColorNote);
        }

        [Fact]
        public void Stacked_notes_in_a_column_get_ascending_before_jump_layers()
        {
            MapContext.Version = new Version(4, 0, 0);
            var index = new EditorSpawnTimeIndex();
            NoteEditorData lower = ColorNote(4f, 1, 0);
            NoteEditorData upper = ColorNote(4f, 1, 2);
            index.Add(lower);
            index.Add(upper);
            new EditorSpawnProcessor(index).Rebuild(EditorSpawnTimeIndex.Quantize(4f));
            Assert.Equal(
                NoteLineLayer.Base,
                EditorSpawnDataRepository.GetSpawnData(lower).beforeJumpNoteLineLayer
            );
            Assert.Equal(
                NoteLineLayer.Upper,
                EditorSpawnDataRepository.GetSpawnData(upper).beforeJumpNoteLineLayer
            );
        }

        [Fact]
        public void Two_crossover_color_notes_flip_to_each_others_columns()
        {
            MapContext.Version = new Version(4, 0, 0);
            var index = new EditorSpawnTimeIndex();
            NoteEditorData a = ColorNote(4f, 3, 0, ColorType.ColorA);
            NoteEditorData b = ColorNote(4f, 1, 0, ColorType.ColorB);
            index.Add(a);
            index.Add(b);
            new EditorSpawnProcessor(index).Rebuild(EditorSpawnTimeIndex.Quantize(4f));
            Assert.Equal(1f, EditorSpawnDataRepository.GetSpawnData(a).flipLineIndex);
            Assert.Equal(3f, EditorSpawnDataRepository.GetSpawnData(b).flipLineIndex);
        }

        [Fact]
        public void Noodle_startX_stacks_by_float_column_on_v3()
        {
            MapContext.Version = new Version(3, 3, 0);
            var index = new EditorSpawnTimeIndex();
            NoteEditorData lower = ColorNote(4f, 0, 0);
            NoteEditorData upper = ColorNote(4f, 3, 2);
            index.Add(lower);
            index.Add(upper);
            var cache = new EditorDeserializedData();
            cache.SetObject(lower, DeserializeNote(lower, startX: 1f, startY: 0f));
            cache.SetObject(upper, DeserializeNote(upper, startX: 1f, startY: 1f));
            new EditorSpawnProcessor(index, cache).Rebuild(EditorSpawnTimeIndex.Quantize(4f));
            Assert.Equal(0f, EditorSpawnDataRepository.GetSpawnData(lower).startNoteLineLayer);
            Assert.Equal(1f, EditorSpawnDataRepository.GetSpawnData(upper).startNoteLineLayer);
        }

        [Fact]
        public void V4_does_not_noodle_stack_start_note_line_layer()
        {
            MapContext.Version = new Version(4, 0, 0);
            var index = new EditorSpawnTimeIndex();
            NoteEditorData lower = ColorNote(4f, 1, 0);
            NoteEditorData upper = ColorNote(4f, 1, 2);
            index.Add(lower);
            index.Add(upper);
            new EditorSpawnProcessor(index).Rebuild(EditorSpawnTimeIndex.Quantize(4f));
            Assert.Equal(0f, EditorSpawnDataRepository.GetSpawnData(lower).startNoteLineLayer);
            Assert.Equal(2f, EditorSpawnDataRepository.GetSpawnData(upper).startNoteLineLayer);
        }

        [Fact]
        public void Authored_flip_wins_over_auto_flip()
        {
            MapContext.Version = new Version(3, 3, 0);
            var index = new EditorSpawnTimeIndex();
            NoteEditorData a = ColorNote(4f, 3, 0, ColorType.ColorA);
            NoteEditorData b = ColorNote(4f, 1, 0, ColorType.ColorB);
            index.Add(a);
            index.Add(b);
            var cache = new EditorDeserializedData();
            cache.SetObject(a, DeserializeNote(a, flipX: 0f, flipY: 1f));
            cache.SetObject(b, DeserializeNote(b, flipX: 2f, flipY: -1f));
            new EditorSpawnProcessor(index, cache).Rebuild(EditorSpawnTimeIndex.Quantize(4f));
            Assert.Equal(2f, EditorSpawnDataRepository.GetSpawnData(a).flipLineIndex);
            Assert.Equal(1f, EditorSpawnDataRepository.GetSpawnData(a).flipYSide);
        }

        [Fact]
        public void Slider_head_overlap_sets_hasHeadNote()
        {
            MapContext.Version = new Version(4, 0, 0);
            var index = new EditorSpawnTimeIndex();
            NoteEditorData note = ColorNote(4f, 1, 0);
            ChainEditorData chain = ChainEditorData.CreateNew(
                4f,
                ColorType.ColorA,
                1,
                0,
                0,
                NoteCutDirection.Up,
                8f,
                2,
                0,
                0,
                3,
                1f
            );
            index.Add(note);
            index.Add(chain);
            new EditorSpawnProcessor(index).Rebuild(EditorSpawnTimeIndex.Quantize(4f));
            Assert.True(EditorSpawnDataRepository.GetSpawnData(chain).hasHeadNote);
            Assert.Equal(
                NoteData.GameplayType.BurstSliderHead,
                EditorSpawnDataRepository.GetSpawnData(note).gameplayType
            );
        }

        internal static NoteEditorData ColorNote(
            float beat,
            int column,
            int row,
            ColorType color = ColorType.ColorA,
            NoteCutDirection cut = NoteCutDirection.Up
        )
        {
            return NoteEditorData.CreateNew(beat, column, row, 0, color, NoteType.Note, cut, 0);
        }

        private static EditorNoodleBaseNoteData DeserializeNote(
            NoteEditorData note,
            float? startX = null,
            float? startY = null,
            float? flipX = null,
            float? flipY = null
        )
        {
            var json = new CustomData();
            if (startX.HasValue || startY.HasValue)
            {
                json[NOTE_OFFSET] = new List<object> { startX ?? 0f, startY ?? 0f };
            }
            if (flipX.HasValue || flipY.HasValue)
            {
                json[FLIP] = new List<object> { flipX ?? 0f, flipY ?? 0f };
            }
            return (EditorNoodleBaseNoteData)
                new NoodleCustomDataCodec().Deserialize(
                    note,
                    json,
                    new CustomDataCodecContext
                    {
                        SourceVersion = new Version(3, 3, 0),
                        TargetVersion = new Version(3, 3, 0),
                    }
                )!;
        }
    }
}
