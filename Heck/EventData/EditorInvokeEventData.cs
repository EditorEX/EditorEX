﻿using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData.CustomEvents;
using Heck.Deserialize;

namespace EditorEX.Heck.EventData
{
    public class EditorInvokeEventData : ICustomEventCustomData
    {
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        public EditorInvokeEventData(CustomEventEditorData customEventEditorData)
        {
            //IDictionary<string, CustomEventData> eventDefinitions = CustomDataRepository.GetCustomBeatmapSaveData().beatmapCustomData.GetRequired<IDictionary<string, CustomEventData>>(Constants.EVENT_DEFINITIONS);
            //string eventName = customEventEditorData.customData.GetRequired<string>(Constants.EVENT);
            //CustomEventData = eventDefinitions[eventName];
        }

        internal CustomEventData CustomEventData { get; }
    }
}
