using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Scripts.SerializedData;
using BeatmapEditor3D.Types;
using BeatmapSaveDataCommon;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.MapData.Objects;
using Xunit;
using V3Custom = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;
using V4 = BeatmapSaveDataVersion4;

namespace EditorEX.Tests.Tests
{
    public class ArcCodecTests
    {
        [Fact]
        public void SaveV3_then_LoadV3_preserves_head_and_tail()
        {
            ArcEditorData arc = Arc();
            var repo = new CustomDataRepository();
            repo.AddCustomData(arc, new CustomData { ["track"] = "a" });
            var saved = (V3Custom.SliderSaveData)ArcCodec.SaveV3(arc, repo);
            ArcEditorData loaded = ArcCodec.LoadV3(
                saved,
                new BeatmapEditorRotationProcessor_v3(new List<RotationEventData>())
            );

            Assert.Equal(4f, loaded.beat);
            Assert.Equal(1, loaded.column);
            Assert.Equal(0, loaded.row);
            Assert.Equal(0.5f, loaded.controlPointLengthMultiplier);
            Assert.Equal(8f, loaded.tailBeat);
            Assert.Equal(2, loaded.tailColumn);
            Assert.Equal(1, loaded.tailRow);
            Assert.Equal(1.5f, loaded.tailControlPointLengthMultiplier);
            Assert.Equal(ColorType.ColorB, loaded.colorType);
        }

        [Fact]
        public void SaveV4Data_then_LoadV4_preserves_control_points()
        {
            ArcEditorData arc = Arc();
            V4.Arc data = ArcCodec.SaveV4Data(arc);
            V4.ColorNote head = ColorNoteCodec.SaveV4DataFromArcHead(arc);
            V4.ColorNote tail = ColorNoteCodec.SaveV4DataFromArcTail(arc);

            ArcEditorData loaded = ArcCodec.LoadV4(4f, 0, head, 8f, 15, tail, data);

            Assert.Equal(4f, loaded.beat);
            Assert.Equal(8f, loaded.tailBeat);
            Assert.Equal(1, loaded.column);
            Assert.Equal(2, loaded.tailColumn);
            Assert.Equal(0, loaded.row);
            Assert.Equal(1, loaded.tailRow);
            Assert.Equal(0.5f, loaded.controlPointLengthMultiplier);
            Assert.Equal(1.5f, loaded.tailControlPointLengthMultiplier);
            Assert.Equal(15, loaded.tailRotation);
            Assert.Equal(ColorType.ColorB, loaded.colorType);
        }

        private static ArcEditorData Arc()
        {
            return ArcEditorData.CreateNew(
                ColorType.ColorB,
                4f,
                1,
                0,
                0,
                NoteCutDirection.Up,
                0.5f,
                8f,
                2,
                1,
                0,
                NoteCutDirection.Down,
                1.5f,
                SliderMidAnchorMode.Straight
            );
        }
    }
}
