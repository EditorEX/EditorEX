using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Scripts.SerializedData;
using BeatmapEditor3D.SerializedData;
using BeatmapEditor3D.Types;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using V3 = BeatmapSaveDataVersion3;
using V3CustomSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;
using V4 = BeatmapSaveDataVersion4;

namespace EditorEX.MapData.Objects
{
    public static class BombNoteCodec
    {
        public static NoteEditorData LoadV3(
            V3CustomSaveData.BombNoteSaveData data,
            BeatmapEditorRotationProcessor_v3 rotationProcessor
        )
        {
            return BeatmapDataModelsLoader.CreateBombNoteEditorData_v3(data, rotationProcessor);
        }

        public static NoteEditorData LoadV4(float beat, int rotation, V4.BombNote data)
        {
            return NoteEditorData.CreateNew(
                beat,
                data.x,
                data.y,
                rotation,
                ColorType.None,
                NoteType.Bomb,
                NoteCutDirection.None,
                0
            );
        }

        public static V3.BombNoteData SaveV3(
            NoteEditorData n,
            ICustomDataRepository customDataRepository
        )
        {
            return CustomDataUtil.SaveCustom(n, customDataRepository, out var customData)
                ? new V3CustomSaveData.BombNoteSaveData(n.beat, n.column, n.row, customData)
                : new V3.BombNoteData(n.beat, n.column, n.row);
        }

        public static V4.BombNote SaveV4Data(NoteEditorData n)
        {
            return new V4.BombNote { x = n.column, y = n.row };
        }
    }
}
