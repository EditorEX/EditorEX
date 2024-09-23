using BeatmapEditor3D.DataModels;
using BetterEditor.Chroma.Events;
using BetterEditor.Chroma.Lighting;
using BetterEditor.CustomJSONData;
using BetterEditor.CustomJSONData.CustomEvents;
using BetterEditor.CustomJSONData.Util;
using Chroma;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BetterEditor.Chroma.Constants;

namespace BetterEditor.Chroma.Deserializer
{
	internal class EditorChromaCustomDataManager
	{
		[EarlyDeserializer]
		internal static void DeserializerEarly(
			TrackBuilder trackBuilder,
			bool v2)
		{
			var beatmapData = CustomDataRepository.GetCustomBeatmapSaveData().customData;
			IEnumerable<CustomData>? environmentData = beatmapData.Get<List<object>>(v2 ? V2_ENVIRONMENT : ENVIRONMENT)?.Cast<CustomData>();
			if (environmentData != null)
			{
				foreach (CustomData gameObjectData in environmentData)
				{
					trackBuilder.AddManyFromCustomData(gameObjectData, v2, false);

					CustomData? geometryData = gameObjectData.Get<CustomData?>(v2 ? V2_GEOMETRY : GEOMETRY);
					object? materialData = geometryData?.Get<object?>(v2 ? V2_MATERIAL : MATERIAL);
					if (materialData is CustomData materialCustomData)
					{
						trackBuilder.AddFromCustomData(materialCustomData, v2, false);
					}
				}
			}

			CustomData? materialsData = beatmapData.Get<CustomData>(v2 ? V2_MATERIALS : MATERIALS);
			if (materialsData != null)
			{
				foreach ((string _, object? value) in materialsData)
				{
					if (value == null)
					{
						continue;
					}

					trackBuilder.AddFromCustomData((CustomData)value, v2, false);
				}
			}

			if (!v2)
			{
				return;
			}

			foreach (CustomEventEditorData customEventData in CustomDataRepository.GetCustomEvents())
			{
				try
				{
					switch (customEventData.eventType)
					{
						case ASSIGN_FOG_TRACK:
							trackBuilder.AddFromCustomData(customEventData.customData, v2);
							break;

						default:
							continue;
					}
				}
				catch (Exception e)
				{
					Plugin.Log.Error(e);
				}
			}
		}

		[CustomEventsDeserializer]
		internal static Dictionary<CustomEventEditorData, ICustomEventCustomData> DeserializeCustomEvents(
			Dictionary<string, Track> beatmapTracks,
			Dictionary<string, List<object>> pointDefinitions)
		{
			var dictionary = new Dictionary<CustomEventEditorData, ICustomEventCustomData>();
			foreach (CustomEventEditorData customEventData in CustomDataRepository.GetCustomEvents())
			{
				bool v2 = customEventData.version2_6_0AndEarlier;
				try
				{
					ICustomEventCustomData chromaCustomEventData;

					switch (customEventData.eventType)
					{
						case ASSIGN_FOG_TRACK:
							if (!v2)
							{
								continue;
							}

							chromaCustomEventData = new ChromaAssignFogEventData(customEventData.customData.GetTrack(beatmapTracks, v2));
							break;

						case ANIMATE_COMPONENT:
							if (v2)
							{
								continue;
							}

							chromaCustomEventData = new ChromaAnimateComponentData(customEventData.customData, beatmapTracks, pointDefinitions);
							break;

						default:
							continue;
					}

					dictionary.Add(customEventData, chromaCustomEventData);
				}
				catch (Exception e)
				{
					Plugin.Log.Error(e);
				}
			}

			return dictionary;
		}

		//TODO: Arcs
		[ObjectsDeserializer]
		internal static Dictionary<BaseEditorData, IObjectCustomData> DeserializeObjects(
			BeatmapLevelDataModel beatmapLevelDataModel,
			Dictionary<string, Track> beatmapTracks,
			Dictionary<string, List<object>> pointDefinitions,
			bool v2)
		{
			var dictionary = new Dictionary<BaseEditorData, IObjectCustomData>();

			foreach (BaseBeatmapObjectEditorData beatmapObjectData in beatmapLevelDataModel.allBeatmapObjects)
			{
				try
				{
					CustomData customData = beatmapObjectData.GetCustomData();
					if (customData == null)
					{
						Plugin.Log.Warn("Chroma | customData is null...");
						continue;
					}
					switch (beatmapObjectData)
					{
						case NoteEditorData noteData:
							dictionary.Add(beatmapObjectData, new ChromaNoteData(customData, beatmapTracks, pointDefinitions, v2));
							break;

						case ChainEditorData sliderData:
							dictionary.Add(beatmapObjectData, new ChromaNoteData(customData, beatmapTracks, pointDefinitions, v2));
							break;

						case ArcEditorData arcData:
							dictionary.Add(beatmapObjectData, new ChromaNoteData(customData, beatmapTracks, pointDefinitions, v2));
							break;

						case ObstacleEditorData obstacleData:
							dictionary.Add(beatmapObjectData, new ChromaObjectData(customData, beatmapTracks, pointDefinitions, v2));
							break;

						default:
							continue;
					}
				}
				catch (Exception e)
				{
					Plugin.Log.Error(e);
				}
			}

			return dictionary;
		}

		[EventsDeserializer]
		internal static Dictionary<BasicEventEditorData, IEventCustomData> DeserializeEvents(
			BeatmapBasicEventsDataModel eventsDataModel,
			bool v2)
		{
			List<BasicEventEditorData> beatmapEventDatas = eventsDataModel.GetAllEventsAsList().ToList();

			EditorLegacyLightHelper legacyLightHelper = null;
			if (v2)
			{
				legacyLightHelper = new EditorLegacyLightHelper(beatmapEventDatas);
			}

			var dictionary = new Dictionary<BasicEventEditorData, IEventCustomData>();
			foreach (BasicEventEditorData beatmapEventData in beatmapEventDatas)
			{

				try
				{
					dictionary.Add(beatmapEventData, new EditorChromaEventData(beatmapEventData, legacyLightHelper, v2));
				}
				catch (Exception e)
				{
					Plugin.Log.Error(e);
				}
			}

			// Horrible stupid logic to get next same type event per light id
			// what am i even doing anymore
			var allNextSameTypes = new Dictionary<int, Dictionary<int, BasicEventEditorData>>();
			for (int i = beatmapEventDatas.Count - 1; i >= 0; i--)
			{
				var beatmapEventData = beatmapEventDatas[i];
				if (!(beatmapEventData is BasicEventEditorData basicBeatmapEventData)) continue;
				if (!TryGetEventData(beatmapEventDatas[i], out EditorChromaEventData currentEventData))
				{
					continue;
				}

				int type = (int)beatmapEventData.type;
				if (!allNextSameTypes.TryGetValue(
						type,
						out Dictionary<int, BasicEventEditorData> nextSameTypes))
				{
					allNextSameTypes[type] = nextSameTypes = new Dictionary<int, BasicEventEditorData>();
				}

				currentEventData.NextSameTypeEvent = currentEventData.NextSameTypeEvent ?? new Dictionary<int, BasicEventEditorData>(nextSameTypes);
				IEnumerable<int> ids = currentEventData.LightID;
				if (ids == null)
				{
					nextSameTypes[-1] = basicBeatmapEventData;
					foreach (int key in nextSameTypes.Keys.ToArray())
					{
						nextSameTypes[key] = basicBeatmapEventData;
					}
				}
				else
				{
					foreach (int id in ids)
					{
						nextSameTypes[id] = basicBeatmapEventData;
					}
				}
			}

			return dictionary;

			bool TryGetEventData(BasicEventEditorData beatmapEventData, out EditorChromaEventData chromaEventData)
			{
				if (dictionary.TryGetValue(beatmapEventData, out IEventCustomData eventCustomData))
				{
					chromaEventData = (EditorChromaEventData)eventCustomData;
					return true;
				}

				chromaEventData = null;
				return false;
			}
		}

		internal static Color? GetColorFromData(CustomData data, bool v2)
		{
			return GetColorFromData(data, v2 ? V2_COLOR : COLOR);
		}

		internal static Color? GetColorFromData(CustomData data, string member = COLOR)
		{
			List<float>? color = data.Get<List<object>>(member)?.Select(Convert.ToSingle).ToList();
			if (color == null)
			{
				return null;
			}

			return new Color(color[0], color[1], color[2], color.Count > 3 ? color[3] : 1);
		}
	}
}
