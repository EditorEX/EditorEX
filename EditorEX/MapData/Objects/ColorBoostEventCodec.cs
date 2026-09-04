using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using V3 = BeatmapSaveDataVersion3;
using V3CustomSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;
using V4 = BeatmapSaveDataVersion4;

namespace EditorEX.MapData.Objects
{
    public static class ColorBoostEventCodec
    {
        public static BasicEventEditorData LoadV3(V3CustomSaveData.ColorBoostEventSaveData data)
        {
            return BeatmapDataModelsLoader.CreateEventEditorDataFromColorBoost_v3(data);
        }

        public static BasicEventEditorData LoadV4(float beat, V4.ColorBoostEvent data)
        {
            return BasicEventEditorData.CreateNew(BasicBeatmapEventType.Event5, beat, data.b, 0f);
        }

        public static V3.ColorBoostEventData SaveV3(
            BasicEventEditorData e,
            ICustomDataRepository customDataRepository
        )
        {
            return CustomDataUtil.SaveCustom(e, customDataRepository, out var customData)
                ? new V3CustomSaveData.ColorBoostEventSaveData(e.beat, e.value == 1, customData)
                : new V3.ColorBoostEventData(e.beat, e.value == 1);
        }

        public static V4.ColorBoostEvent SaveV4Data(BasicEventEditorData e)
        {
            return new V4.ColorBoostEvent { b = e.value };
        }
    }
}
