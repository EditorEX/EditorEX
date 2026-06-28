using System;
using System.Collections.Generic;
using System.ComponentModel;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.CustomJSONData.CustomEvents;
using Heck.Deserialize;

namespace EditorEX.Heck.Deserialize
{
    public class EditorDeserializedData
    {
        internal EditorDeserializedData(
            Dictionary<CustomEventEditorData, ICustomEventCustomData> customEventCustomDatas,
            Dictionary<BasicEventEditorData, IEventCustomData> eventCustomDatas,
            Dictionary<BaseEditorData, IObjectCustomData> objectCustomDatas
        )
        {
            CustomEventCustomDatas = customEventCustomDatas;
            _eventCustomDatas = BuildEventLookup(eventCustomDatas);
            _objectCustomDatas = objectCustomDatas;
        }

        // Event transformers (e.g. modifiers) produce fresh BasicEventEditorData instances,
        // so the markers/effects hold a different object than the one used as the key during
        // deserialization. The instances keep their BeatmapEditorObjectId (copy constructor
        // copies id), so we key by id to keep resolving working across transforms.
        private static Dictionary<BeatmapEditorObjectId, IEventCustomData> BuildEventLookup(
            Dictionary<BasicEventEditorData, IEventCustomData> eventCustomDatas
        )
        {
            Dictionary<BeatmapEditorObjectId, IEventCustomData> lookup = new(
                eventCustomDatas.Count
            );
            foreach (KeyValuePair<BasicEventEditorData, IEventCustomData> pair in eventCustomDatas)
            {
                lookup[pair.Key.id] = pair.Value;
            }

            return lookup;
        }

        public bool Resolve<T>(CustomEventEditorData customEventData, out T? result)
            where T : ICustomEventCustomData?
        {
            if (customEventData == null)
            {
                result = default;
                return false;
            }
            return Resolve(CustomEventCustomDatas, customEventData, out result);
        }

        public bool Resolve<T>(BasicEventEditorData beatmapEventData, out T? result)
            where T : IEventCustomData
        {
            if (beatmapEventData == null)
            {
                result = default;
                return false;
            }
            // Dedicated struct-key path. The reference was already null-checked above, so the
            // id is valid and there is nothing left to null-check. Routing it through the generic
            // Resolve below would compile `baseData == null` into a `box BeatmapEditorObjectId`,
            // which Unity's Mono does not elide -> a heap allocation on every event resolve.
            return ResolveById(_eventCustomDatas, beatmapEventData.id, out result);
        }

        private static bool ResolveById<TResultType, TResultData>(
            Dictionary<BeatmapEditorObjectId, TResultType> dictionary,
            BeatmapEditorObjectId id,
            out TResultData? result
        )
            where TResultData : TResultType?
        {
            if (!dictionary.TryGetValue(id, out TResultType customData) || customData == null)
            {
                result = default;
                return false;
            }
            if (customData is TResultData t)
            {
                result = t;
                return true;
            }
            throw new InvalidOperationException(
                string.Concat(
                    "Custom data was not of correct type. Expected: [",
                    typeof(TResultType).Name,
                    "], was: [",
                    customData.GetType().Name,
                    "]."
                )
            );
        }

        public bool Resolve<T>(BaseEditorData? beatmapObjectData, out T? result)
            where T : IObjectCustomData?
        {
            if (beatmapObjectData == null)
            {
                result = default;
                return false;
            }
            return Resolve(_objectCustomDatas, beatmapObjectData, out result);
        }

        internal void RegisterNewObject(
            BaseEditorData beatmapObjectData,
            IObjectCustomData objectCustomData
        )
        {
            _objectCustomDatas.Add(beatmapObjectData, objectCustomData);
        }

        internal void Remap(EditorDeserializedData source)
        {
            _eventCustomDatas = source._eventCustomDatas;
            _objectCustomDatas = source._objectCustomDatas;
        }

        private static bool Resolve<TBaseData, TResultType, TResultData>(
            Dictionary<TBaseData, TResultType> dictionary,
            TBaseData? baseData,
            out TResultData? result
        )
            where TResultData : TResultType?
        {
            if (
                baseData == null
                || !dictionary.TryGetValue(baseData, out TResultType customData)
                || customData == null
            )
            {
                result = default;
                return false;
            }
            if (customData is TResultData t)
            {
                result = t;
                return true;
            }
            throw new InvalidOperationException(
                string.Concat(
                    "Custom data was not of correct type. Expected: [",
                    typeof(TResultType).Name,
                    "], was: [",
                    customData.GetType().Name,
                    "]."
                )
            );
        }

        internal Dictionary<CustomEventEditorData, ICustomEventCustomData> CustomEventCustomDatas;

        internal Dictionary<BeatmapEditorObjectId, IEventCustomData> _eventCustomDatas;

        private Dictionary<BaseEditorData, IObjectCustomData> _objectCustomDatas;
    }
}
