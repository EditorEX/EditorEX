using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Scripts.SerializedData;
using BeatmapEditor3D.SerializedData;
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
    public static class WaypointCodec
    {
        public static WaypointEditorData LoadV2(
            V2CustomSaveData.WaypointSaveData data,
            BeatmapEditorRotationProcessor_v2 rotationProcessor
        )
        {
            return BeatmapDataModelsLoader.CreateWaypointEditorData_v2(data, rotationProcessor);
        }

        public static WaypointEditorData LoadV3(
            V3CustomSaveData.WaypointSaveData data,
            BeatmapEditorRotationProcessor_v3 rotationProcessor
        )
        {
            return BeatmapDataModelsLoader.CreateWaypointEditorData_v3(data, rotationProcessor);
        }

        public static WaypointEditorData LoadV4(float beat, V4.Waypoint data)
        {
            return WaypointEditorData.CreateNew(
                beat,
                data.x,
                data.y,
                0,
                BeatmapTypeConverters.ConvertOffsetDirection(data.o)
            );
        }

        public static V2.WaypointData SaveV2(
            WaypointEditorData w,
            ICustomDataRepository customDataRepository
        )
        {
            return CustomDataUtil.SaveCustom(w, customDataRepository, out var customData)
                ? new V2CustomSaveData.WaypointSaveData(
                    w.beat,
                    w.column,
                    (BeatmapSaveDataCommon.NoteLineLayer)w.row,
                    (BeatmapSaveDataCommon.OffsetDirection)w.offsetDirection,
                    customData
                )
                : new V2.WaypointData(
                    w.beat,
                    w.column,
                    (BeatmapSaveDataCommon.NoteLineLayer)w.row,
                    (BeatmapSaveDataCommon.OffsetDirection)w.offsetDirection
                );
        }

        public static V3.WaypointData SaveV3(
            WaypointEditorData w,
            ICustomDataRepository customDataRepository
        )
        {
            return CustomDataUtil.SaveCustom(w, customDataRepository, out var customData)
                ? new V3CustomSaveData.WaypointSaveData(
                    w.beat,
                    w.column,
                    w.row,
                    (BeatmapSaveDataCommon.OffsetDirection)w.offsetDirection,
                    customData
                )
                : new V3.WaypointData(
                    w.beat,
                    w.column,
                    w.row,
                    (BeatmapSaveDataCommon.OffsetDirection)w.offsetDirection
                );
        }

        public static V4.Waypoint SaveV4Data(WaypointEditorData w)
        {
            return new V4.Waypoint
            {
                x = w.column,
                y = w.row,
                o = (BeatmapSaveDataCommon.OffsetDirection)w.offsetDirection,
            };
        }
    }
}
