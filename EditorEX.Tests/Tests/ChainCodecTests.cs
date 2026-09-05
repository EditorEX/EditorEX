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
    public class ChainCodecTests
    {
        [Fact]
        public void SaveV3_then_LoadV3_preserves_head_tail_and_slices()
        {
            ChainEditorData chain = Chain();
            var repo = new CustomDataRepository();
            repo.AddCustomData(chain, new CustomData { ["track"] = "a" });
            var saved = (V3Custom.BurstSliderSaveData)ChainCodec.SaveV3(chain, repo);
            ChainEditorData loaded = ChainCodec.LoadV3(
                saved,
                new BeatmapEditorRotationProcessor_v3(new List<RotationEventData>())
            );

            Assert.Equal(4f, loaded.beat);
            Assert.Equal(0, loaded.column);
            Assert.Equal(1, loaded.row);
            Assert.Equal(10f, loaded.tailBeat);
            Assert.Equal(3, loaded.tailColumn);
            Assert.Equal(2, loaded.tailRow);
            Assert.Equal(5, loaded.sliceCount);
            Assert.Equal(0.75f, loaded.squishAmount);
            Assert.Equal(ColorType.ColorA, loaded.colorType);
        }

        [Fact]
        public void SaveV4Data_then_LoadV4_preserves_chain_template()
        {
            ChainEditorData chain = Chain();
            V4.Chain data = ChainCodec.SaveV4Data(chain);
            V4.ColorNote head = ColorNoteCodec.SaveV4DataFromChain(chain);

            ChainEditorData loaded = ChainCodec.LoadV4(4f, 0, head, 10f, 30, data);

            Assert.Equal(4f, loaded.beat);
            Assert.Equal(10f, loaded.tailBeat);
            Assert.Equal(0, loaded.column);
            Assert.Equal(1, loaded.row);
            Assert.Equal(3, loaded.tailColumn);
            Assert.Equal(2, loaded.tailRow);
            Assert.Equal(5, loaded.sliceCount);
            Assert.Equal(0.75f, loaded.squishAmount);
            Assert.Equal(30, loaded.tailRotation);
            Assert.Equal(ColorType.ColorA, loaded.colorType);
        }

        private static ChainEditorData Chain()
        {
            return ChainEditorData.CreateNew(
                4f,
                ColorType.ColorA,
                0,
                1,
                0,
                NoteCutDirection.Up,
                10f,
                3,
                2,
                0,
                5,
                0.75f
            );
        }
    }
}
