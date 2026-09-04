using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using BeatmapSaveDataCommon;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using V3 = BeatmapSaveDataVersion3;
using V3CustomSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;

namespace EditorEX.MapData.Objects
{
    public static class RotationEventCodec
    {
        public static BasicEventEditorData LoadV3(V3CustomSaveData.RotationEventSaveData data)
        {
            return BeatmapDataModelsLoader.CreateEventEditorDataFromRotation_v3(data);
        }

        public static V3.RotationEventData SaveV3(
            BasicEventEditorData e,
            ICustomDataRepository customDataRepository
        )
        {
            return CustomDataUtil.SaveCustom(e, customDataRepository, out var customData)
                ? new V3CustomSaveData.RotationEventSaveData(
                    e.beat,
                    (e.type == BasicBeatmapEventType.Event14)
                        ? ExecutionTime.Early
                        : ExecutionTime.Late,
                    (float)e.value,
                    customData
                )
                : new V3.RotationEventData(
                    e.beat,
                    (e.type == BasicBeatmapEventType.Event14)
                        ? ExecutionTime.Early
                        : ExecutionTime.Late,
                    (float)e.value
                );
        }
    }
}
