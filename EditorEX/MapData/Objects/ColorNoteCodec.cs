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
    public static class ColorNoteCodec
    {
        public static NoteEditorData LoadV2(
            V2CustomSaveData.NoteSaveData data,
            BeatmapEditorRotationProcessor_v2 rotationProcessor
        )
        {
            return BeatmapDataModelsLoader.CreateNoteEditorData_v2(data, rotationProcessor);
        }

        public static NoteEditorData LoadV3(
            V3CustomSaveData.ColorNoteSaveData data,
            BeatmapEditorRotationProcessor_v3 rotationProcessor
        )
        {
            return BeatmapDataModelsLoader.CreateColorNoteEditorData_v3(data, rotationProcessor);
        }

        public static NoteEditorData LoadV4(float beat, int rotation, V4.ColorNote data)
        {
            return NoteEditorData.CreateNew(
                beat,
                data.x,
                data.y,
                rotation,
                data.c != NoteColorType.ColorA ? ColorType.ColorB : ColorType.ColorA,
                NoteType.Note,
                BeatmapTypeConverters.ConvertNoteCutDirection(data.d),
                data.a
            );
        }

        public static V2.NoteData SaveV2(
            NoteEditorData n,
            ICustomDataRepository customDataRepository
        )
        {
            V2.NoteType noteType;
            switch (n.noteType)
            {
                case NoteType.Note:
                    noteType = (n.type == ColorType.ColorA) ? V2.NoteType.NoteA : V2.NoteType.NoteB;
                    break;
                case NoteType.Bomb:
                    noteType = V2.NoteType.Bomb;
                    break;
                default:
                    noteType = V2.NoteType.None;
                    break;
            }

            return CustomDataUtil.SaveCustom(n, customDataRepository, out var customData)
                ? new V2CustomSaveData.NoteSaveData(
                    n.beat,
                    n.column,
                    (BeatmapSaveDataCommon.NoteLineLayer)n.row,
                    noteType,
                    (BeatmapSaveDataCommon.NoteCutDirection)n.cutDirection,
                    customData
                )
                : new V2.NoteData(
                    n.beat,
                    n.column,
                    (BeatmapSaveDataCommon.NoteLineLayer)n.row,
                    noteType,
                    (BeatmapSaveDataCommon.NoteCutDirection)n.cutDirection
                );
        }

        public static V3.ColorNoteData SaveV3(
            NoteEditorData n,
            ICustomDataRepository customDataRepository
        )
        {
            return CustomDataUtil.SaveCustom(n, customDataRepository, out var customData)
                ? new V3CustomSaveData.ColorNoteSaveData(
                    n.beat,
                    n.column,
                    n.row,
                    (n.type == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB,
                    (BeatmapSaveDataCommon.NoteCutDirection)n.cutDirection,
                    n.angle,
                    customData
                )
                : new V3.ColorNoteData(
                    n.beat,
                    n.column,
                    n.row,
                    (n.type == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB,
                    (BeatmapSaveDataCommon.NoteCutDirection)n.cutDirection,
                    n.angle
                );
        }

        public static V3.ColorNoteData SaveV3FromChain(
            ChainEditorData c,
            ICustomDataRepository customDataRepository
        )
        {
            return CustomDataUtil.SaveCustom(c, customDataRepository, out var customData)
                ? new V3CustomSaveData.ColorNoteSaveData(
                    c.beat,
                    c.column,
                    c.row,
                    (c.colorType == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB,
                    (BeatmapSaveDataCommon.NoteCutDirection)c.cutDirection,
                    0,
                    customData
                )
                : new V3.ColorNoteData(
                    c.beat,
                    c.column,
                    c.row,
                    (c.colorType == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB,
                    (BeatmapSaveDataCommon.NoteCutDirection)c.cutDirection,
                    0
                );
        }

        public static V4.ColorNote SaveV4Data(NoteEditorData n)
        {
            return new V4.ColorNote
            {
                x = n.column,
                y = n.row,
                c = n.type != ColorType.ColorA ? NoteColorType.ColorB : NoteColorType.ColorA,
                d = (BeatmapSaveDataCommon.NoteCutDirection)n.cutDirection,
                a = n.angle,
            };
        }

        public static V4.ColorNote SaveV4DataFromChain(ChainEditorData c)
        {
            return new V4.ColorNote
            {
                x = c.column,
                y = c.row,
                c = c.colorType != ColorType.ColorA ? NoteColorType.ColorB : NoteColorType.ColorA,
                d = (BeatmapSaveDataCommon.NoteCutDirection)c.cutDirection,
                a = 0,
            };
        }

        public static V4.ColorNote SaveV4DataFromArcHead(ArcEditorData a)
        {
            return new V4.ColorNote
            {
                x = a.column,
                y = a.row,
                c = a.colorType != ColorType.ColorA ? NoteColorType.ColorB : NoteColorType.ColorA,
                d = (BeatmapSaveDataCommon.NoteCutDirection)a.cutDirection,
                a = 0,
            };
        }

        public static V4.ColorNote SaveV4DataFromArcTail(ArcEditorData a)
        {
            return new V4.ColorNote
            {
                x = a.tailColumn,
                y = a.tailRow,
                c = a.colorType != ColorType.ColorA ? NoteColorType.ColorB : NoteColorType.ColorA,
                d = (BeatmapSaveDataCommon.NoteCutDirection)a.tailCutDirection,
                a = 0,
            };
        }
    }
}
