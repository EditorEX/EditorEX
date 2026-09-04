using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Scripts.SerializedData;
using BeatmapEditor3D.SerializedData;
using BeatmapEditor3D.Types;
using BeatmapSaveDataCommon;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using V2 = BeatmapSaveDataVersion2_6_0AndEarlier;
using V2CustomSaveData = CustomJSONData.CustomBeatmap.Version2_6_0AndEarlierCustomBeatmapSaveData;
using V3 = BeatmapSaveDataVersion3;
using V3CustomSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;
using V4 = BeatmapSaveDataVersion4;

namespace EditorEX.MapData.Objects
{
    public static class ArcCodec
    {
        public static ArcEditorData LoadV2(
            V2CustomSaveData.SliderSaveData data,
            BeatmapEditorRotationProcessor_v2 rotationProcessor
        )
        {
            return BeatmapDataModelsLoader.CreateSliderEditorData_v2(data, rotationProcessor);
        }

        public static ArcEditorData LoadV3(
            V3CustomSaveData.SliderSaveData data,
            BeatmapEditorRotationProcessor_v3 rotationProcessor
        )
        {
            return BeatmapDataModelsLoader.CreateArcEditorData_v3(data, rotationProcessor);
        }

        public static ArcEditorData LoadV4(
            float headBeat,
            int headRotation,
            V4.ColorNote headNote,
            float tailBeat,
            int tailRotation,
            V4.ColorNote tailNote,
            V4.Arc arc
        )
        {
            return ArcEditorData.CreateNew(
                headNote.c != NoteColorType.ColorA ? ColorType.ColorB : ColorType.ColorA,
                headBeat,
                headNote.x,
                headNote.y,
                headRotation,
                BeatmapTypeConverters.ConvertNoteCutDirection(headNote.d),
                arc.m,
                tailBeat,
                tailNote.x,
                tailNote.y,
                tailRotation,
                BeatmapTypeConverters.ConvertNoteCutDirection(tailNote.d),
                arc.tm,
                BeatmapTypeConverters.ConvertSliderMidAnchorMode(arc.a)
            );
        }

        public static V2.SliderData SaveV2(
            ArcEditorData a,
            ICustomDataRepository customDataRepository
        )
        {
            return CustomDataUtil.SaveCustom(a, customDataRepository, out var customData)
                ? new V2CustomSaveData.SliderSaveData(
                    (a.colorType == ColorType.ColorA) ? V2.ColorType.ColorA : V2.ColorType.ColorB,
                    a.beat,
                    a.column,
                    (BeatmapSaveDataCommon.NoteLineLayer)a.row,
                    a.controlPointLengthMultiplier,
                    (BeatmapSaveDataCommon.NoteCutDirection)a.cutDirection,
                    a.tailBeat,
                    a.tailColumn,
                    (BeatmapSaveDataCommon.NoteLineLayer)a.tailRow,
                    a.tailControlPointLengthMultiplier,
                    (BeatmapSaveDataCommon.NoteCutDirection)a.tailCutDirection,
                    (BeatmapSaveDataCommon.SliderMidAnchorMode)a.midAnchorMode,
                    customData
                )
                : new V2.SliderData(
                    (a.colorType == ColorType.ColorA) ? V2.ColorType.ColorA : V2.ColorType.ColorB,
                    a.beat,
                    a.column,
                    (BeatmapSaveDataCommon.NoteLineLayer)a.row,
                    a.controlPointLengthMultiplier,
                    (BeatmapSaveDataCommon.NoteCutDirection)a.cutDirection,
                    a.tailBeat,
                    a.tailColumn,
                    (BeatmapSaveDataCommon.NoteLineLayer)a.tailRow,
                    a.tailControlPointLengthMultiplier,
                    (BeatmapSaveDataCommon.NoteCutDirection)a.tailCutDirection,
                    (BeatmapSaveDataCommon.SliderMidAnchorMode)a.midAnchorMode
                );
        }

        public static V3.SliderData SaveV3(
            ArcEditorData a,
            ICustomDataRepository customDataRepository
        )
        {
            return CustomDataUtil.SaveCustom(a, customDataRepository, out var customData)
                ? new V3CustomSaveData.SliderSaveData(
                    (a.colorType == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB,
                    a.beat,
                    a.column,
                    a.row,
                    a.controlPointLengthMultiplier,
                    (BeatmapSaveDataCommon.NoteCutDirection)a.cutDirection,
                    a.tailBeat,
                    a.tailColumn,
                    a.tailRow,
                    a.tailControlPointLengthMultiplier,
                    (BeatmapSaveDataCommon.NoteCutDirection)a.tailCutDirection,
                    (BeatmapSaveDataCommon.SliderMidAnchorMode)a.midAnchorMode,
                    customData
                )
                : new V3.SliderData(
                    (a.colorType == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB,
                    a.beat,
                    a.column,
                    a.row,
                    a.controlPointLengthMultiplier,
                    (BeatmapSaveDataCommon.NoteCutDirection)a.cutDirection,
                    a.tailBeat,
                    a.tailColumn,
                    a.tailRow,
                    a.tailControlPointLengthMultiplier,
                    (BeatmapSaveDataCommon.NoteCutDirection)a.tailCutDirection,
                    (BeatmapSaveDataCommon.SliderMidAnchorMode)a.midAnchorMode
                );
        }

        public static V4.Arc SaveV4Data(ArcEditorData a)
        {
            return new V4.Arc
            {
                m = a.controlPointLengthMultiplier,
                tm = a.tailControlPointLengthMultiplier,
                a = (BeatmapSaveDataCommon.SliderMidAnchorMode)a.midAnchorMode,
            };
        }
    }
}
