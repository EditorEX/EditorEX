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
    public static class ObstacleCodec
    {
        public static ObstacleEditorData LoadV2(
            V2CustomSaveData.ObstacleSaveData data,
            BeatmapEditorRotationProcessor_v2 rotationProcessor
        )
        {
            return BeatmapDataModelsLoader.CreateObstacleEditorData_v2(data, rotationProcessor);
        }

        public static ObstacleEditorData LoadV3(
            V3CustomSaveData.ObstacleSaveData data,
            BeatmapEditorRotationProcessor_v3 rotationProcessor
        )
        {
            return BeatmapDataModelsLoader.CreateObstacleEditorData_v3(data, rotationProcessor);
        }

        public static ObstacleEditorData LoadV4(float beat, int rotation, V4.Obstacle data)
        {
            return ObstacleEditorData.CreateNew(
                beat,
                data.x,
                data.y,
                rotation,
                data.d,
                data.w,
                data.h - 2
            );
        }

        public static V2.ObstacleData SaveV2(
            ObstacleEditorData o,
            ICustomDataRepository customDataRepository
        )
        {
            V2.ObstacleType type = o.row == 2 ? V2.ObstacleType.Top : V2.ObstacleType.FullHeight;
            return CustomDataUtil.SaveCustom(o, customDataRepository, out var customData)
                ? new V2CustomSaveData.ObstacleSaveData(
                    o.beat,
                    o.column,
                    type,
                    o.duration,
                    o.width,
                    customData
                )
                : new V2.ObstacleData(o.beat, o.column, type, o.duration, o.width);
        }

        public static V3.ObstacleData SaveV3(
            ObstacleEditorData o,
            ICustomDataRepository customDataRepository
        )
        {
            int layer = o.row * 2;
            int height = o.height * 2 - 1;
            return CustomDataUtil.SaveCustom(o, customDataRepository, out var customData)
                ? new V3CustomSaveData.ObstacleSaveData(
                    o.beat,
                    o.column,
                    layer,
                    o.duration,
                    o.width,
                    height,
                    customData
                )
                : new V3.ObstacleData(o.beat, o.column, layer, o.duration, o.width, height);
        }

        public static bool CanSaveV4(ObstacleEditorData o)
        {
            return o.width > 0 && o.height > 0 && o.duration > 0f;
        }

        public static V4.Obstacle SaveV4Data(ObstacleEditorData o)
        {
            return new V4.Obstacle
            {
                x = o.column,
                y = o.row,
                d = o.duration,
                w = o.width,
                h = o.height + 2,
            };
        }
    }
}
