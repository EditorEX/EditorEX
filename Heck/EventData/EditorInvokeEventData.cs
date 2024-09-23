using BetterEditor.CustomJSONData;
using BetterEditor.CustomJSONData.CustomEvents;
using CustomJSONData.CustomBeatmap;
using Heck;
using System.Collections.Generic;

namespace BetterEditor.Heck.EventData
{
	public class EditorInvokeEventData : ICustomEventCustomData
	{
		// ReSharper disable once SuggestBaseTypeForParameterInConstructor
		public EditorInvokeEventData(CustomEventEditorData customEventEditorData)
		{
			IDictionary<string, CustomEventData> eventDefinitions = CustomDataRepository.GetCustomBeatmapSaveData().beatmapCustomData.GetRequired<IDictionary<string, CustomEventData>>(Constants.EVENT_DEFINITIONS);
			string eventName = customEventEditorData.customData.GetRequired<string>(Constants.EVENT);
			CustomEventData = eventDefinitions[eventName];
		}

		internal CustomEventData CustomEventData { get; }
	}
}
