using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using BeatmapSaveDataCommon;
using V3 = BeatmapSaveDataVersion3;
using V4 = BeatmapSaveDataVersion4;

namespace EditorEX.MapData.Objects
{
    public static class NjsEventCodec
    {
        public static NoteJumpSpeedEditorData LoadV4(float beat, V4.NoteJumpMovementSpeedEvent data)
        {
            return NoteJumpSpeedEditorData.CreateNew(
                beat,
                data.d,
                BeatmapTypeConverters.ConvertEaseType(data.e),
                data.p == 1
            );
        }

        public static V4.NoteJumpMovementSpeedEvent SaveV4Data(NoteJumpSpeedEditorData njs)
        {
            return new V4.NoteJumpMovementSpeedEvent
            {
                p = njs.usePreviousValue ? 1 : 0,
                d = njs.noteJumpSpeedDelta,
                e = EaseTypeConvertor.Convert(njs.easeType),
            };
        }
    }
}
