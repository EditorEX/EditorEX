using BeatmapEditor3D.DataModels;
using BetterEditor.CustomJSONData;
using BetterEditor.CustomJSONData.CustomEvents;
using BetterEditor.CustomJSONData.Util;
using BetterEditor.NoodleExtensions.ObjectData;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using NoodleExtensions;
using System;
using System.Collections.Generic;

namespace BetterEditor.NoodleExtensions.Deserializer
{
	internal class EditorNoodleCustomDataManager
	{
		[EarlyDeserializer]
		internal static void DeserializeEarly(TrackBuilder trackBuilder)
		{
			foreach (CustomEventEditorData customEventEditorData in CustomDataRepository.GetCustomEvents())
			{
				bool v2 = customEventEditorData.version2_6_0AndEarlier;
				try
				{
					string eventType = customEventEditorData.eventType;
					if (!(eventType == "AssignPlayerToTrack"))
					{
						if (eventType == "AssignTrackParent")
						{
							trackBuilder.AddFromCustomData(customEventEditorData.GetCustomData(), v2 ? "_parentTrack" : "parentTrack", true);
						}
					}
					else
					{
						trackBuilder.AddFromCustomData(customEventEditorData.GetCustomData(), v2, true);
					}
				}
				catch (Exception e)
				{
					Plugin.Log.Error(e);
				}
			}
		}

		[ObjectsDeserializer]
		private static Dictionary<BaseEditorData, IObjectCustomData> DeserializeObjects(IBeatmapLevelDataModel beatmapLevelDataModel, Dictionary<string, List<object>> pointDefinitions, Dictionary<string, Track> beatmapTracks, bool v2)
		{
			Dictionary<BaseEditorData, IObjectCustomData> dictionary = new Dictionary<BaseEditorData, IObjectCustomData>();
			foreach (BaseEditorData baseEditorData in (beatmapLevelDataModel as BeatmapLevelDataModel).allBeatmapObjects)
			{
				CustomData customData = baseEditorData.GetCustomData();
				ObstacleEditorData customObstacleData = baseEditorData as ObstacleEditorData;
				if (customObstacleData == null)
				{
					NoteEditorData customNoteData = baseEditorData as NoteEditorData;
					if (customNoteData == null)
					{
						ArcEditorData customSliderData = baseEditorData as ArcEditorData;
						if (customSliderData == null)
						{
							ChainEditorData customChainData = baseEditorData as ChainEditorData;
							if (customChainData == null)
							{
								dictionary.Add(baseEditorData, new EditorNoodleObjectData(baseEditorData, customData, pointDefinitions, beatmapTracks, v2, false));
							}
							else
							{
								dictionary.Add(baseEditorData, new EditorNoodleObjectData(customChainData, customData, pointDefinitions, beatmapTracks, v2, false));
							}
						}
						else
						{
							dictionary.Add(baseEditorData, new EditorNoodleSliderData(customSliderData, customData, pointDefinitions, beatmapTracks, v2, false));
						}
					}
					else
					{
						dictionary.Add(baseEditorData, new EditorNoodleNoteData(customNoteData, customData, pointDefinitions, beatmapTracks, v2, false));
					}
				}
				else
				{
					dictionary.Add(baseEditorData, new EditorNoodleObstacleData(customObstacleData, customData, pointDefinitions, beatmapTracks, v2, false));
				}
			}
			return dictionary;
		}

		[CustomEventsDeserializer]
		private static Dictionary<CustomEventEditorData, ICustomEventCustomData> DeserializeCustomEvents(Dictionary<string, List<object>> pointDefinitions, Dictionary<string, Track> tracks)
		{
			Dictionary<CustomEventEditorData, ICustomEventCustomData> dictionary = new Dictionary<CustomEventEditorData, ICustomEventCustomData>();
			foreach (CustomEventEditorData customEventEditorData in CustomDataRepository.GetCustomEvents())
			{
				bool v2 = customEventEditorData.version2_6_0AndEarlier;
				try
				{
					CustomData data = customEventEditorData.customData;
					string eventType = customEventEditorData.eventType;
					if (!(eventType == "AssignPlayerToTrack"))
					{
						if (eventType == "AssignTrackParent")
						{
							dictionary.Add(customEventEditorData, new NoodleParentTrackEventData(data, tracks, v2));
						}
					}
					else
					{
						dictionary.Add(customEventEditorData, new NoodlePlayerTrackEventData(data, tracks, v2));
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
