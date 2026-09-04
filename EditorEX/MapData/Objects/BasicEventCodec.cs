using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
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
    public static class BasicEventCodec
    {
        public static BasicEventEditorData LoadV2(V2CustomSaveData.EventSaveData data)
        {
            return BeatmapDataModelsLoader.CreateEventEditorData_v2(data);
        }

        public static BasicEventEditorData LoadV3(V3CustomSaveData.BasicEventSaveData data)
        {
            return BeatmapDataModelsLoader.CreateEventEditorData_v3(data);
        }

        public static BasicEventEditorData LoadV4(float beat, V4.BasicEvent data)
        {
            return BasicEventEditorData.CreateNew(
                (BasicBeatmapEventType)data.t,
                beat,
                data.i,
                data.f
            );
        }

        public static V2.EventData SaveV2(
            BasicEventEditorData e,
            bool supportsFloatValue,
            ICustomDataRepository customDataRepository
        )
        {
            return CustomDataUtil.SaveCustom(e, customDataRepository, out var customData)
                ? new V2CustomSaveData.EventSaveData(
                    e.beat,
                    (BeatmapEventType)e.type,
                    e.value,
                    e.floatValue,
                    customData
                )
                : new V2.EventData(
                    e.beat,
                    (BeatmapEventType)e.type,
                    e.value,
                    supportsFloatValue ? e.floatValue : 0f
                );
        }

        public static V3.BasicEventData SaveV3(
            BasicEventEditorData e,
            ICustomDataRepository customDataRepository
        )
        {
            return CustomDataUtil.SaveCustom(e, customDataRepository, out var customData)
                ? new V3CustomSaveData.BasicEventSaveData(
                    e.beat,
                    (BeatmapEventType)e.type,
                    e.value,
                    e.floatValue,
                    customData
                )
                : new V3.BasicEventData(e.beat, (BeatmapEventType)e.type, e.value, e.floatValue);
        }

        public static V3.BasicEventData SaveV3(
            BasicEventEditorData e,
            bool supportsFloatValue,
            ICustomDataRepository customDataRepository
        )
        {
            return CustomDataUtil.SaveCustom(e, customDataRepository, out var customData)
                ? new V3CustomSaveData.BasicEventSaveData(
                    e.beat,
                    (BeatmapEventType)e.type,
                    e.value,
                    e.floatValue,
                    customData
                )
                : new V3.BasicEventData(
                    e.beat,
                    (BeatmapEventType)e.type,
                    e.value,
                    supportsFloatValue ? e.floatValue : 0f
                );
        }

        public static V4.BasicEvent SaveV4Data(BasicEventEditorData e)
        {
            return new V4.BasicEvent
            {
                t = (BeatmapEventType)e.type,
                i = e.value,
                f = e.floatValue,
            };
        }

        public static List<V2.SpecialEventsForKeyword> SaveKeywordV2(
            BasicEventTypesForKeywordEditorData e
        )
        {
            return e
                .eventTypes.Select(x => new V2.SpecialEventsForKeyword(
                    e.keyword,
                    e.eventTypes.Select(y => (BeatmapEventType)y).ToList()
                ))
                .ToList();
        }

        public static BasicEventTypesWithKeywords.BasicEventTypesForKeyword SaveKeywordV3(
            BasicEventTypesForKeywordEditorData data
        )
        {
            return new BasicEventTypesWithKeywords.BasicEventTypesForKeyword(
                data.keyword,
                data.eventTypes.Select((BasicBeatmapEventType e) => (BeatmapEventType)e).ToList()
            );
        }
    }
}
