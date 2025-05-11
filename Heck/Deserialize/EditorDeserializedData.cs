using BeatmapEditor3D.DataModels;
using EditorEX.CustomJSONData.CustomEvents;
using Heck.Deserialize;
using System;
using System.Collections.Generic;

namespace EditorEX.Heck.Deserialize
{
    public class EditorDeserializedData
    {
        internal EditorDeserializedData(
            Dictionary<CustomEventEditorData, ICustomEventCustomData> customEventCustomDatas,
            Dictionary<BasicEventEditorData, IEventCustomData> eventCustomDatas,
            Dictionary<BaseEditorData, IObjectCustomData> objectCustomDatas)
        {
            CustomEventCustomDatas = customEventCustomDatas;
            _eventCustomDatas = eventCustomDatas;
            _objectCustomDatas = objectCustomDatas;
        }

        public bool Resolve<T>(CustomEventEditorData customEventData, out T result) where T : ICustomEventCustomData?
        {
            if (customEventData == null)
            {
                result = default;
                return false;
            }
            return Resolve(CustomEventCustomDatas, customEventData, out result);
        }

        public bool Resolve<T>(BasicEventEditorData beatmapEventData, out T result) where T : IEventCustomData
        {
            if (beatmapEventData == null)
            {
                result = default;
                return false;
            }
            return Resolve(_eventCustomDatas, beatmapEventData, out result);
        }

        public bool Resolve<T>(BaseEditorData? beatmapObjectData, out T? result) where T : IObjectCustomData?
        {
            if (beatmapObjectData == null)
            {
                result = default;
                return false;
            }
            return Resolve(_objectCustomDatas, beatmapObjectData, out result);
        }

        internal void RegisterNewObject(BaseEditorData beatmapObjectData, IObjectCustomData objectCustomData)
        {
            _objectCustomDatas.Add(beatmapObjectData, objectCustomData);
        }

        internal void Remap(EditorDeserializedData source)
        {
            _eventCustomDatas = source._eventCustomDatas;
            _objectCustomDatas = source._objectCustomDatas;
        }

        private static bool Resolve<TBaseData, TResultType, TResultData>(Dictionary<TBaseData, TResultType> dictionary, TBaseData baseData, out TResultData result) where TResultData : TResultType
        {
            TResultType customData;
            if (!dictionary.TryGetValue(baseData, out customData) || customData == null)
            {
                result = default;
                return false;
            }
            if (customData is TResultData)
            {
                TResultData t = (TResultData)((object)customData);
                result = t;
                return true;
            }
            throw new InvalidOperationException(string.Concat("Custom data was not of correct type. Expected: [", typeof(TResultType).Name, "], was: [", customData.GetType().Name, "]."));
        }

        internal Dictionary<CustomEventEditorData, ICustomEventCustomData> CustomEventCustomDatas;

        private Dictionary<BasicEventEditorData, IEventCustomData> _eventCustomDatas;

        private Dictionary<BaseEditorData, IObjectCustomData> _objectCustomDatas;
    }
}
