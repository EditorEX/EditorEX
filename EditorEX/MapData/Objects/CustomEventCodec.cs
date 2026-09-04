using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData.CustomEvents;
using static EditorEX.CustomJSONData.VersionedSaveData.Custom2_6_0AndEarlierBeatmapSaveDataVersioned;
using V2CustomSaveData = CustomJSONData.CustomBeatmap.Version2_6_0AndEarlierCustomBeatmapSaveData;
using V3CustomSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;

namespace EditorEX.MapData.Objects
{
    public static class CustomEventCodec
    {
        public const string V2Key = "_customEvents";
        public const string V3Key = "customEvents";

        public static CustomEventEditorData LoadV2(V2CustomSaveData.CustomEventSaveData data)
        {
            return CustomEventEditorData.CreateNew(data.time, data.type, data.customData, true);
        }

        public static CustomEventEditorData LoadV3(V3CustomSaveData.CustomEventSaveData data)
        {
            return CustomEventEditorData.CreateNew(data.beat, data.type, data.customData, false);
        }

        public static CustomEventEditorData LoadV4(CustomData entry)
        {
            float beat = entry.Get<float?>("b") ?? entry.Get<float?>("_time") ?? 0f;
            string type = entry.Get<string>("t") ?? entry.Get<string>("_type") ?? "";
            CustomData data =
                entry.Get<CustomData>("data")
                ?? entry.Get<CustomData>("d")
                ?? entry.Get<CustomData>("_data")
                ?? new CustomData();
            return CustomEventEditorData.CreateNew(beat, type, data, false);
        }

        public static V3CustomSaveData.CustomEventSaveData SaveV3(CustomEventEditorData data)
        {
            return new V3CustomSaveData.CustomEventSaveData(
                data.beat,
                data.eventType,
                CustomDataUtil.Filter(data.customData)
            );
        }

        public static CustomEventDataSerialized SaveV2(CustomEventEditorData data)
        {
            return new CustomEventDataSerialized(data);
        }

        public static void Write(
            CustomData customData,
            IEnumerable<CustomEventEditorData> events,
            bool v3
        )
        {
            string key = v3 ? V3Key : V2Key;
            string otherKey = v3 ? V2Key : V3Key;
            customData.TryRemove(otherKey, out _);

            var list = new List<object>();
            foreach (var evt in events)
            {
                if (v3)
                {
                    var entry = new CustomData
                    {
                        ["b"] = evt.beat,
                        ["t"] = evt.eventType,
                        ["data"] = CustomDataUtil.Filter(evt.customData),
                    };
                    list.Add(entry);
                }
                else
                {
                    list.Add(SaveV2(evt));
                }
            }

            if (list.Count == 0)
            {
                customData.TryRemove(key, out _);
                return;
            }

            customData[key] = list;
        }

        public static List<CustomEventEditorData> Read(CustomData? customData, bool v3)
        {
            var result = new List<CustomEventEditorData>();
            if (customData == null)
            {
                return result;
            }

            var entries =
                customData.Get<List<object>>(v3 ? V3Key : V2Key)
                ?? customData.Get<List<object>>(v3 ? V2Key : V3Key);
            if (entries == null)
            {
                return result;
            }

            foreach (var raw in entries)
            {
                if (raw is CustomData entry)
                {
                    result.Add(LoadV4(entry));
                }
            }

            return result;
        }
    }
}
