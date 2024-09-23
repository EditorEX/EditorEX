using BeatmapEditor3D.DataModels;
using BetterEditor.CustomJSONData;
using BetterEditor.CustomJSONData.CustomEvents;
using BetterEditor.Heck.EventData;
using BetterEditor.Heck.ObjectData;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using System;
using System.Collections.Generic;

namespace BetterEditor.Heck.Deserializer
{
	internal class EditorHeckCustomDataManager
	{
		[ObjectsDeserializer]
		private static Dictionary<BaseEditorData, IObjectCustomData> DeserializeObjects(IBeatmapLevelDataModel beatmapLevelDataModel, Dictionary<string, Track> beatmapTracks, bool v2)
		{
			Dictionary<BaseEditorData, IObjectCustomData> dictionary = new Dictionary<BaseEditorData, IObjectCustomData>();
			foreach (BaseEditorData baseEditorData in (beatmapLevelDataModel as BeatmapLevelDataModel).allBeatmapObjects)
			{
				CustomData customData = CustomDataRepository.GetCustomData(baseEditorData);
				if (customData == null)
				{
					Plugin.Log.Info(baseEditorData.GetType().Name);
				}
				else
				{
					dictionary.Add(baseEditorData, new EditorHeckObjectData(customData, beatmapTracks, v2));
				}
			}
			return dictionary;
		}

		[CustomEventsDeserializer]
		private static Dictionary<CustomEventEditorData, ICustomEventCustomData> DeserializeCustomEvents(IBeatmapLevelDataModel beatmapLevelDataModel, Dictionary<string, Track> beatmapTracks, Dictionary<string, List<object>> pointDefinitions)
		{
			Dictionary<CustomEventEditorData, ICustomEventCustomData> dictionary = new Dictionary<CustomEventEditorData, ICustomEventCustomData>();
			foreach (CustomEventEditorData customEventData in CustomDataRepository.GetCustomEvents())
			{
				bool v2 = customEventData.version2_6_0AndEarlier;
				try
				{
					string eventType = customEventData.eventType;
					if (!(eventType == "AnimateTrack") && !(eventType == "AssignPathAnimation"))
					{
						if (eventType == "InvokeEvent")
						{
							if (!v2)
							{
								dictionary.Add(customEventData, new EditorInvokeEventData(customEventData));
							}
						}
					}
					else
					{
						dictionary.Add(customEventData, new EditorCoroutineEventData(customEventData, pointDefinitions, beatmapTracks, v2));
					}
				}
				catch (Exception e)
				{
					Plugin.Log.Error(e);
				}
			}
			return dictionary;
		}
	}
}
