using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Scripts.SerializedData;
using BeatmapEditor3D.SerializedData;
using BeatmapEditor3D.Types;
using BeatmapSaveDataCommon;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using V3 = BeatmapSaveDataVersion3;
using V3CustomSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;
using V4 = BeatmapSaveDataVersion4;

namespace EditorEX.MapData.Objects
{
    public static class ChainCodec
    {
        public static ChainEditorData LoadV3(
            V3CustomSaveData.BurstSliderSaveData data,
            BeatmapEditorRotationProcessor_v3 rotationProcessor
        )
        {
            return BeatmapDataModelsLoader.CreateChainEditorData_v3(data, rotationProcessor);
        }

        public static ChainEditorData LoadV4(
            float headBeat,
            int headRotation,
            V4.ColorNote headNote,
            float tailBeat,
            int tailRotation,
            V4.Chain chain
        )
        {
            ColorType colorType =
                headNote.c != NoteColorType.ColorA ? ColorType.ColorB : ColorType.ColorA;
            return ChainEditorData.CreateNew(
                headBeat,
                colorType,
                headNote.x,
                headNote.y,
                headRotation,
                BeatmapTypeConverters.ConvertNoteCutDirection(headNote.d),
                tailBeat,
                chain.tx,
                chain.ty,
                tailRotation,
                chain.c,
                chain.s
            );
        }

        public static V3.BurstSliderData SaveV3(
            ChainEditorData c,
            ICustomDataRepository customDataRepository
        )
        {
            return CustomDataUtil.SaveCustom(c, customDataRepository, out var customData)
                ? new V3CustomSaveData.BurstSliderSaveData(
                    (c.colorType == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB,
                    c.beat,
                    c.column,
                    c.row,
                    (BeatmapSaveDataCommon.NoteCutDirection)c.cutDirection,
                    c.tailBeat,
                    c.tailColumn,
                    c.tailRow,
                    c.sliceCount,
                    c.squishAmount,
                    customData
                )
                : new V3.BurstSliderData(
                    (c.colorType == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB,
                    c.beat,
                    c.column,
                    c.row,
                    (BeatmapSaveDataCommon.NoteCutDirection)c.cutDirection,
                    c.tailBeat,
                    c.tailColumn,
                    c.tailRow,
                    c.sliceCount,
                    c.squishAmount
                );
        }

        public static V4.Chain SaveV4Data(ChainEditorData c)
        {
            return new V4.Chain
            {
                tx = c.tailColumn,
                ty = c.tailRow,
                c = c.sliceCount,
                s = c.squishAmount,
            };
        }
    }
}
