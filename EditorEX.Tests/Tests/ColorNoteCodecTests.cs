using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Scripts.SerializedData;
using BeatmapEditor3D.Types;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.MapData.Objects;
using Xunit;
using V3Custom = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;
using V4 = BeatmapSaveDataVersion4;

namespace EditorEX.Tests.Tests
{
    public class ColorNoteCodecTests
    {
        [Fact]
        public void OccupiesChainHead_when_note_shares_head_cell()
        {
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

            Assert.True(ColorNoteCodec.OccupiesChainHead(note, chain));
        }

        [Fact]
        public void OccupiesChainHead_is_false_for_bomb_or_different_cell()
        {
            NoteEditorData bomb = NoteEditorData.CreateNew(
                4f,
                1,
                0,
                0,
                ColorType.None,
                NoteType.Bomb,
                NoteCutDirection.None,
                0
            );
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

            Assert.False(ColorNoteCodec.OccupiesChainHead(bomb, chain));
            Assert.False(ColorNoteCodec.OccupiesChainHead(ColorNote(4f, 2, 0), chain));
        }

        [Fact]
        public void SaveV3_then_LoadV3_preserves_cells()
        {
            NoteEditorData note = ColorNote(12f, 2, 1, ColorType.ColorB, NoteCutDirection.Left, 15);
            var repo = new CustomDataRepository();
            repo.AddCustomData(note, new CustomData { ["track"] = "a" });
            var saved = (V3Custom.ColorNoteSaveData)ColorNoteCodec.SaveV3(note, repo);

            NoteEditorData loaded = ColorNoteCodec.LoadV3(saved, EmptyV3Rotation());

            Assert.Equal(12f, loaded.beat);
            Assert.Equal(2, loaded.column);
            Assert.Equal(1, loaded.row);
            Assert.Equal(ColorType.ColorB, loaded.type);
            Assert.Equal(NoteCutDirection.Left, loaded.cutDirection);
            Assert.Equal(15, loaded.angle);
        }

        [Fact]
        public void SaveV4Data_then_LoadV4_preserves_template()
        {
            NoteEditorData note = ColorNote(0f, 3, 2, ColorType.ColorA, NoteCutDirection.Down, 45);
            V4.ColorNote data = ColorNoteCodec.SaveV4Data(note);

            NoteEditorData loaded = ColorNoteCodec.LoadV4(9f, 90, data);

            Assert.Equal(9f, loaded.beat);
            Assert.Equal(90, loaded.rotation);
            Assert.Equal(3, loaded.column);
            Assert.Equal(2, loaded.row);
            Assert.Equal(ColorType.ColorA, loaded.type);
            Assert.Equal(NoteCutDirection.Down, loaded.cutDirection);
            Assert.Equal(45, loaded.angle);
        }

        [Fact]
        public void SaveV3_attaches_filtered_custom_data()
        {
            var repo = new CustomDataRepository();
            NoteEditorData note = ColorNote(1f, 0, 0);
            repo.AddCustomData(
                note,
                new CustomData { ["NE_flipLineIndex"] = 1f, ["track"] = "a" }
            );

            var saved = (V3Custom.ColorNoteSaveData)ColorNoteCodec.SaveV3(note, repo);

            Assert.Equal("a", saved.customData["track"]);
            Assert.False(saved.customData.ContainsKey("NE_flipLineIndex"));
        }

        private static NoteEditorData ColorNote(
            float beat,
            int column,
            int row,
            ColorType color = ColorType.ColorA,
            NoteCutDirection cut = NoteCutDirection.Up,
            int angle = 0
        )
        {
            return NoteEditorData.CreateNew(
                beat,
                column,
                row,
                0,
                color,
                NoteType.Note,
                cut,
                angle
            );
        }

        private static BeatmapEditorRotationProcessor_v3 EmptyV3Rotation()
        {
            return new BeatmapEditorRotationProcessor_v3(new List<RotationEventData>());
        }
    }
}
