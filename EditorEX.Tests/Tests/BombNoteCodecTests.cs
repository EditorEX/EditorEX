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
    public class BombNoteCodecTests
    {
        [Fact]
        public void SaveV3_then_LoadV3_preserves_cells()
        {
            NoteEditorData bomb = Bomb(6f, 2, 1);
            var repo = new CustomDataRepository();
            repo.AddCustomData(bomb, new CustomData { ["track"] = "a" });
            var saved = (V3Custom.BombNoteSaveData)BombNoteCodec.SaveV3(bomb, repo);

            NoteEditorData loaded = BombNoteCodec.LoadV3(saved, EmptyV3Rotation());

            Assert.Equal(6f, loaded.beat);
            Assert.Equal(2, loaded.column);
            Assert.Equal(1, loaded.row);
            Assert.Equal(NoteType.Bomb, loaded.noteType);
        }

        [Fact]
        public void SaveV4Data_then_LoadV4_preserves_template()
        {
            NoteEditorData bomb = Bomb(0f, 1, 2);
            V4.BombNote data = BombNoteCodec.SaveV4Data(bomb);

            NoteEditorData loaded = BombNoteCodec.LoadV4(11f, 15, data);

            Assert.Equal(11f, loaded.beat);
            Assert.Equal(15, loaded.rotation);
            Assert.Equal(1, loaded.column);
            Assert.Equal(2, loaded.row);
            Assert.Equal(NoteType.Bomb, loaded.noteType);
            Assert.Equal(ColorType.None, loaded.type);
        }

        [Fact]
        public void SaveV3_attaches_filtered_custom_data()
        {
            var repo = new CustomDataRepository();
            NoteEditorData bomb = Bomb(1f, 0, 0);
            repo.AddCustomData(bomb, new CustomData { ["NE_x"] = 1, ["fake"] = true });

            var saved = (V3Custom.BombNoteSaveData)BombNoteCodec.SaveV3(bomb, repo);

            Assert.Equal(true, saved.customData["fake"]);
            Assert.False(saved.customData.ContainsKey("NE_x"));
        }

        private static NoteEditorData Bomb(float beat, int column, int row)
        {
            return NoteEditorData.CreateNew(
                beat,
                column,
                row,
                0,
                ColorType.None,
                NoteType.Bomb,
                NoteCutDirection.None,
                0
            );
        }

        private static BeatmapEditorRotationProcessor_v3 EmptyV3Rotation()
        {
            return new BeatmapEditorRotationProcessor_v3(new List<RotationEventData>());
        }
    }
}
