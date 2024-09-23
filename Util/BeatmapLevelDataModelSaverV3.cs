using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using BeatmapEditor3D.Types;
using BeatmapSaveDataVersion3;
using BetterEditor.CustomJSONData.VersionedSaveData;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using System.Linq;
using TrackDefinitions.DataModels;
using static BetterEditor.CustomJSONData.VersionedSaveData.Custom2_6_0AndEarlierBeatmapSaveDataVersioned;

namespace BetterEditor.CustomJSONData.Util
{
	public class BeatmapLevelDataModelSaverV3
	{
		public BeatmapLevelDataModelSaver V3Saver { get; set; }

		public BeatmapLevelDataModelSaverV3(BeatmapLevelDataModelSaver v3Saver)
		{
			V3Saver = v3Saver;
		}

		public bool SaveCustom(BaseEditorData data)
		{
			var customData = data.GetCustomData();
			return (customData != null && !customData.IsEmpty);
		}

		public BeatmapSaveData.SliderData CreateSliderSaveData(ArcEditorData a)
		{
			return SaveCustom(a) ? new CustomBeatmapSaveData.SliderData((a.colorType == ColorType.ColorA) ? BeatmapSaveData.NoteColorType.ColorA : BeatmapSaveData.NoteColorType.ColorB, a.beat, a.column, a.row, a.controlPointLengthMultiplier, a.cutDirection, a.tailBeat, a.tailColumn, a.tailRow, a.tailControlPointLengthMultiplier, a.tailCutDirection, a.midAnchorMode, a.GetCustomData())
								 : new BeatmapSaveData.SliderData((a.colorType == ColorType.ColorA) ? BeatmapSaveData.NoteColorType.ColorA : BeatmapSaveData.NoteColorType.ColorB, a.beat, a.column, a.row, a.controlPointLengthMultiplier, a.cutDirection, a.tailBeat, a.tailColumn, a.tailRow, a.tailControlPointLengthMultiplier, a.tailCutDirection, a.midAnchorMode);
		}

		public BeatmapSaveData.ColorNoteData CreateColorNoteSaveData(NoteEditorData n)
		{
			return SaveCustom(n) ? new CustomBeatmapSaveData.ColorNoteData(n.beat, n.column, n.row, (n.type == ColorType.ColorA) ? BeatmapSaveData.NoteColorType.ColorA : BeatmapSaveData.NoteColorType.ColorB, n.cutDirection, n.angle, n.GetCustomData())
								 : new BeatmapSaveData.ColorNoteData(n.beat, n.column, n.row, (n.type == ColorType.ColorA) ? BeatmapSaveData.NoteColorType.ColorA : BeatmapSaveData.NoteColorType.ColorB, n.cutDirection, n.angle);
		}

		public BeatmapSaveData.BombNoteData CreateBombNoteSaveData(NoteEditorData n)
		{
			return SaveCustom(n) ? new CustomBeatmapSaveData.BombNoteData(n.beat, n.column, n.row, n.GetCustomData())
								 : new BeatmapSaveData.BombNoteData(n.beat, n.column, n.row);
		}

		public BeatmapSaveData.BasicEventData CreateBasicEventSaveData(BasicEventEditorData e, bool supportsFloatValue)
		{
			return SaveCustom(e) ? new CustomBeatmapSaveData.BasicEventData(e.beat, (BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType)e.type, e.value, e.floatValue, e.GetCustomData())
								 : new BeatmapSaveData.BasicEventData(e.beat, (BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType)e.type, e.value, supportsFloatValue ? e.floatValue : 0f);
		}

		public BeatmapSaveData.ObstacleData CreateObstacleSaveData(ObstacleEditorData o)
		{
			return SaveCustom(o) ? new CustomBeatmapSaveData.ObstacleData(o.beat, o.column, o.row, o.duration, o.width, o.height, o.GetCustomData())
								 : new BeatmapSaveData.ObstacleData(o.beat, o.column, o.row, o.duration, o.width, o.height);
		}

		public BeatmapSaveData.WaypointData CreateWaypointSaveData(WaypointEditorData w)
		{
			return SaveCustom(w) ? new CustomBeatmapSaveData.WaypointData(w.beat, w.column, w.row, w.offsetDirection, w.GetCustomData())
								 : new BeatmapSaveData.WaypointData(w.beat, w.column, w.row, w.offsetDirection);
		}

		public BeatmapSaveData.ColorNoteData CreateColorNoteSaveDataFromChains(ChainEditorData c)
		{
			return SaveCustom(c) ? new CustomBeatmapSaveData.ColorNoteData(c.beat, c.column, c.row, (c.colorType == ColorType.ColorA) ? BeatmapSaveData.NoteColorType.ColorA : BeatmapSaveData.NoteColorType.ColorB, c.cutDirection, 0, c.GetCustomData())
								 : new BeatmapSaveData.ColorNoteData(c.beat, c.column, c.row, (c.colorType == ColorType.ColorA) ? BeatmapSaveData.NoteColorType.ColorA : BeatmapSaveData.NoteColorType.ColorB, c.cutDirection, 0);
		}

		public BeatmapSaveData.BurstSliderData CreateBurstSliderSaveData(ChainEditorData c)
		{
			return SaveCustom(c) ? new CustomBeatmapSaveData.BurstSliderData((c.colorType == ColorType.ColorA) ? BeatmapSaveData.NoteColorType.ColorA : BeatmapSaveData.NoteColorType.ColorB, c.beat, c.column, c.row, c.cutDirection, c.tailBeat, c.tailColumn, c.tailRow, c.sliceCount, c.squishAmount, c.GetCustomData())
								 : new BeatmapSaveData.BurstSliderData((c.colorType == ColorType.ColorA) ? BeatmapSaveData.NoteColorType.ColorA : BeatmapSaveData.NoteColorType.ColorB, c.beat, c.column, c.row, c.cutDirection, c.tailBeat, c.tailColumn, c.tailRow, c.sliceCount, c.squishAmount);
		}

		public BeatmapSaveData.BpmChangeEventData CreateBpmChangeEventSaveData(BpmRegion r)
		{
			return new BeatmapSaveData.BpmChangeEventData(r.startBeat, r.bpm);
		}

		public BeatmapSaveData.RotationEventData CreateRotationEventSaveData(BasicEventEditorData e)
		{
			return SaveCustom(e) ? new CustomBeatmapSaveData.RotationEventData(e.beat, (e.type == BasicBeatmapEventType.Event14) ? BeatmapSaveData.ExecutionTime.Early : BeatmapSaveData.ExecutionTime.Late, (float)e.value, e.GetCustomData())
								 : new BeatmapSaveData.RotationEventData(e.beat, (e.type == BasicBeatmapEventType.Event14) ? BeatmapSaveData.ExecutionTime.Early : BeatmapSaveData.ExecutionTime.Late, (float)e.value);
		}

		public BeatmapSaveData.ColorBoostEventData CreateColorBoostSaveEventData(BasicEventEditorData e)
		{
			return SaveCustom(e) ? new CustomBeatmapSaveData.ColorBoostEventData(e.beat, e.value == 1, e.GetCustomData()) 
								 : new BeatmapSaveData.ColorBoostEventData(e.beat, e.value == 1);
		}

		public BeatmapSaveData.BasicEventData CreateBasicEventSaveData(BasicEventEditorData e)
		{
			return SaveCustom(e) ? new CustomBeatmapSaveData.BasicEventData(e.beat, (BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType)e.type, e.value, e.floatValue, e.GetCustomData()) 
								 : new BeatmapSaveData.BasicEventData(e.beat, (BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData.BeatmapEventType)e.type, e.value, e.floatValue);
		}

		public List<BeatmapSaveData.EventBoxGroup> CreateEventBoxGroupSaveDataWithFxCollection(EventBoxGroupEditorData e, BeatmapSaveData.FxEventsCollection fxEventsCollection)
		{
			IReadOnlyList<EventBoxEditorData> eventBoxesByEventBoxGroupId = V3Saver._beatmapEventBoxGroupsDataModel.GetEventBoxesByEventBoxGroupId(e.id);
			List<BeatmapSaveData.EventBoxGroup> list = new List<BeatmapSaveData.EventBoxGroup>();
			EventBoxGroupType type = e.type;
			switch (type)
			{
				case EventBoxGroupType.Color:
					{
						CustomBeatmapSaveData.LightColorEventBoxGroup lightColorEventBoxGroup = new CustomBeatmapSaveData.LightColorEventBoxGroup(e.beat, e.groupId, eventBoxesByEventBoxGroupId.Select((EventBoxEditorData eventBox) => (LightColorEventBoxEditorData)eventBox).Select((LightColorEventBoxEditorData eventBox) => new BeatmapSaveData.LightColorEventBox(BeatmapLevelDataModelSaver.CreateIndexFilter(eventBox.indexFilter), eventBox.beatDistributionParam, (BeatmapSaveData.EventBox.DistributionParamType)eventBox.beatDistributionParamType, eventBox.brightnessDistributionParam, eventBox.brightnessDistributionShouldAffectFirstBaseEvent, (BeatmapSaveData.EventBox.DistributionParamType)eventBox.brightnessDistributionParamType, BeatmapLevelDataModelSaver.ConvertEaseType(eventBox.brightnessDistributionEaseType), V3Saver._beatmapEventBoxGroupsDataModel.GetBaseEventsListByEventBoxId<LightColorBaseEditorData>(eventBox.id).OrderBy((LightColorBaseEditorData i) => i.beat).Select((LightColorBaseEditorData data) => new BeatmapSaveData.LightColorBaseData(data.beat, (BeatmapSaveData.TransitionType)data.transitionType, (BeatmapSaveData.EnvironmentColorType)data.colorType, data.brightness, data.strobeBeatFrequency, data.strobeBrightness, data.strobeFade))
							.ToList())).ToList(), e.GetCustomData());
						list.Add(lightColorEventBoxGroup);
						break;
					}
				case EventBoxGroupType.Rotation:
					{
						CustomBeatmapSaveData.LightRotationEventBoxGroup lightRotationEventBoxGroup = new CustomBeatmapSaveData.LightRotationEventBoxGroup(e.beat, e.groupId, eventBoxesByEventBoxGroupId.Select((EventBoxEditorData eventBox) => (LightRotationEventBoxEditorData)eventBox).Select((LightRotationEventBoxEditorData eventBox) => new BeatmapSaveData.LightRotationEventBox(BeatmapLevelDataModelSaver.CreateIndexFilter(eventBox.indexFilter), eventBox.beatDistributionParam, (BeatmapSaveData.EventBox.DistributionParamType)eventBox.beatDistributionParamType, eventBox.rotationDistributionParam, (BeatmapSaveData.EventBox.DistributionParamType)eventBox.rotationDistributionParamType, eventBox.rotationDistributionShouldAffectFirstBaseEvent, BeatmapLevelDataModelSaver.ConvertEaseType(eventBox.rotationDistributionEaseType), (BeatmapSaveData.Axis)eventBox.axis, eventBox.flipRotation, V3Saver._beatmapEventBoxGroupsDataModel.GetBaseEventsListByEventBoxId<LightRotationBaseEditorData>(eventBox.id).OrderBy((LightRotationBaseEditorData i) => i.beat).Select(delegate (LightRotationBaseEditorData data)
						{
							float beat = data.beat;
							BeatmapSaveData.EaseType easeType = BeatmapLevelDataModelSaver.ConvertEaseType(new ValueTuple<EaseLeadType, EaseCurveType>(data.easeLeadType, data.easeCurveType).ToEaseType());
							int loopsCount = data.loopsCount;
							float rotation = data.rotation;
							return new BeatmapSaveData.LightRotationBaseData(beat, data.usePreviousEventRotationValue, easeType, loopsCount, rotation, (BeatmapSaveData.LightRotationBaseData.RotationDirection)data.rotationDirection);
						}).ToList())).ToList(), e.GetCustomData());
						list.Add(lightRotationEventBoxGroup);
						break;
					}
				case EventBoxGroupType.Translation:
					{
						BeatmapSaveData.LightTranslationEventBoxGroup lightTranslationEventBoxGroup = new BeatmapSaveData.LightTranslationEventBoxGroup(e.beat, e.groupId, eventBoxesByEventBoxGroupId.Select((EventBoxEditorData eventBox) => (LightTranslationEventBoxEditorData)eventBox).Select((LightTranslationEventBoxEditorData eventBox) => new BeatmapSaveData.LightTranslationEventBox(BeatmapLevelDataModelSaver.CreateIndexFilter(eventBox.indexFilter), eventBox.beatDistributionParam, (BeatmapSaveData.EventBox.DistributionParamType)eventBox.beatDistributionParamType, eventBox.gapDistributionParam, (BeatmapSaveData.EventBox.DistributionParamType)eventBox.gapDistributionParamType, eventBox.gapDistributionShouldAffectFirstBaseEvent, BeatmapLevelDataModelSaver.ConvertEaseType(eventBox.gapDistributionEaseType), (BeatmapSaveData.Axis)eventBox.axis, eventBox.flipTranslation, V3Saver._beatmapEventBoxGroupsDataModel.GetBaseEventsListByEventBoxId<LightTranslationBaseEditorData>(eventBox.id).OrderBy((LightTranslationBaseEditorData i) => i.beat).Select(delegate (LightTranslationBaseEditorData data)
						{
							float beat2 = data.beat;
							BeatmapSaveData.EaseType easeType2 = BeatmapLevelDataModelSaver.ConvertEaseType(new ValueTuple<EaseLeadType, EaseCurveType>(data.easeLeadType, data.easeCurveType).ToEaseType());
							float translation = data.translation;
							return new BeatmapSaveData.LightTranslationBaseData(beat2, data.usePreviousEventTranslationValue, easeType2, translation);
						})
							.ToList())).ToList());
						list.Add(lightTranslationEventBoxGroup);
						break;
					}
				default:
					if (type == EventBoxGroupType.FloatFx)
					{
						BeatmapSaveData.FxEventBoxGroup fxEventBoxGroup = new BeatmapSaveData.FxEventBoxGroup(e.beat, e.groupId, BeatmapSaveData.FxEventType.Float, eventBoxesByEventBoxGroupId.Select((EventBoxEditorData eventBox) => (FxEventBoxEditorData)eventBox).Select(delegate (FxEventBoxEditorData eventBox)
						{
							BeatmapSaveData.IndexFilter indexFilter = BeatmapLevelDataModelSaver.CreateIndexFilter(eventBox.indexFilter);
							float beatDistributionParam = eventBox.beatDistributionParam;
							BeatmapSaveData.EventBox.DistributionParamType beatDistributionParamType = (BeatmapSaveData.EventBox.DistributionParamType)eventBox.beatDistributionParamType;
							float vfxDistributionParam = eventBox.vfxDistributionParam;
							BeatmapSaveData.EventBox.DistributionParamType vfxDistributionParamType = (BeatmapSaveData.EventBox.DistributionParamType)eventBox.vfxDistributionParamType;
							BeatmapSaveData.EaseType easeType3 = BeatmapLevelDataModelSaver.ConvertEaseType(eventBox.vfxDistributionEaseType);
							bool vfxDistributionShouldAffectFirstBaseEvent = eventBox.vfxDistributionShouldAffectFirstBaseEvent;
							IEnumerable<FloatFxBaseEditorData> enumerable = V3Saver._beatmapEventBoxGroupsDataModel.GetBaseEventsListByEventBoxId<FloatFxBaseEditorData>(eventBox.id).OrderBy((FloatFxBaseEditorData i) => i.beat);
							Func<FloatFxBaseEditorData, int> func = (FloatFxBaseEditorData data) => fxEventsCollection.AddEventAndGetIndex(new BeatmapSaveData.FloatFxEventBaseData(data.beat, data.usePreviousEventValue, data.value, BeatmapLevelDataModelSaver.ConvertEaseType(new ValueTuple<EaseLeadType, EaseCurveType>(data.easeLeadType, data.easeCurveType).ToEaseType())));
							return new BeatmapSaveData.FxEventBox(indexFilter, beatDistributionParam, beatDistributionParamType, vfxDistributionParam, vfxDistributionParamType, easeType3, vfxDistributionShouldAffectFirstBaseEvent, enumerable.Select(func).ToList());
						}).ToList());
						list.Add(fxEventBoxGroup);
					}
					break;
			}
			return list;
		}

		public CustomBeatmapSaveDataVersioned Save(Version version)
		{
			List<BeatmapSaveData.BpmChangeEventData> bpmChanges = V3Saver._beatmapDataModel.bpmData.regions.Select(new Func<BpmRegion, BeatmapSaveData.BpmChangeEventData>(CreateBpmChangeEventSaveData)).ToList();
			List<BeatmapSaveData.RotationEventData> rotationEvents = new List<BeatmapSaveData.RotationEventData>()
				.Concat(V3Saver._beatmapEventsDataModel.GetAllDataIn(BasicBeatmapEventType.Event14).Select(new Func<BasicEventEditorData, BeatmapSaveData.RotationEventData>(CreateRotationEventSaveData)))
				.Concat(V3Saver._beatmapEventsDataModel.GetAllDataIn(BasicBeatmapEventType.Event15).Select(new Func<BasicEventEditorData, BeatmapSaveData.RotationEventData>(CreateRotationEventSaveData))).ToList();
			List<BeatmapSaveData.BasicEventData> basicEvents = V3Saver._beatmapEventsDataModel.GetAllEventsAsList().Select(new Func<BasicEventEditorData, BeatmapSaveData.BasicEventData>(CreateBasicEventSaveData)).ToList();
			List<BeatmapSaveData.ColorBoostEventData> colorBoostEvents = V3Saver._beatmapEventsDataModel.GetAllDataIn(BasicBeatmapEventType.Event5).Select(new Func<BasicEventEditorData, BeatmapSaveData.ColorBoostEventData>(CreateColorBoostSaveEventData)).ToList();
			List<BeatmapSaveData.ColorNoteData> colorNotes = new List<BeatmapSaveData.ColorNoteData>();
			List<BeatmapSaveData.BombNoteData> bombNotes = new List<BeatmapSaveData.BombNoteData>();
			List<BeatmapSaveData.ObstacleData> obstacles = new List<BeatmapSaveData.ObstacleData>();
			List<BeatmapSaveData.SliderData> sliders = new List<BeatmapSaveData.SliderData>();
			List<BeatmapSaveData.BurstSliderData> burstSliders = new List<BeatmapSaveData.BurstSliderData>();
			List<BeatmapSaveData.WaypointData> waypoints = new List<BeatmapSaveData.WaypointData>();
			foreach (BaseBeatmapObjectEditorData baseBeatmapObjectEditorData in V3Saver._beatmapLevelDataModel.allBeatmapObjects)
			{
				NoteEditorData noteEditorData = baseBeatmapObjectEditorData as NoteEditorData;
				if (noteEditorData == null)
				{
					WaypointEditorData waypointEditorData = baseBeatmapObjectEditorData as WaypointEditorData;
					if (waypointEditorData == null)
					{
						ObstacleEditorData obstacleEditorData = baseBeatmapObjectEditorData as ObstacleEditorData;
						if (obstacleEditorData == null)
						{
							ChainEditorData chainEditorData = baseBeatmapObjectEditorData as ChainEditorData;
							if (chainEditorData == null)
							{
								ArcEditorData arcEditorData = baseBeatmapObjectEditorData as ArcEditorData;
								if (arcEditorData != null)
								{
									sliders.Add(CreateSliderSaveData(arcEditorData));
								}
							}
							else
							{
								colorNotes.Add(CreateColorNoteSaveDataFromChains(chainEditorData));
								burstSliders.Add(CreateBurstSliderSaveData(chainEditorData));
							}
						}
						else
						{
							obstacles.Add(CreateObstacleSaveData(obstacleEditorData));
						}
					}
					else
					{
						waypoints.Add(CreateWaypointSaveData(waypointEditorData));
					}
				}
				else if (noteEditorData.noteType == NoteType.Note)
				{
					colorNotes.Add(CreateColorNoteSaveData(noteEditorData));
				}
				else
				{
					bombNotes.Add(CreateBombNoteSaveData(noteEditorData));
				}
			}

			bpmChanges.Sort(new Comparison<BeatmapSaveData.BpmChangeEventData>(BeatmapLevelDataModelSaver.SortByBeat));
			rotationEvents.Sort(new Comparison<BeatmapSaveData.RotationEventData>(BeatmapLevelDataModelSaver.SortByRotationTypeAndBeat));
			basicEvents.Sort(new Comparison<BeatmapSaveData.BasicEventData>(BeatmapLevelDataModelSaver.SortByEventTypeAndBeat));
			colorBoostEvents.Sort(new Comparison<BeatmapSaveData.ColorBoostEventData>(BeatmapLevelDataModelSaver.SortByBeat));
			colorNotes.Sort(new Comparison<BeatmapSaveData.ColorNoteData>(BeatmapLevelDataModelSaver.SortByBeat));
			bombNotes.Sort(new Comparison<BeatmapSaveData.BombNoteData>(BeatmapLevelDataModelSaver.SortByBeat));
			waypoints.Sort(new Comparison<BeatmapSaveData.WaypointData>(BeatmapLevelDataModelSaver.SortByBeat));
			obstacles.Sort(new Comparison<BeatmapSaveData.ObstacleData>(BeatmapLevelDataModelSaver.SortByBeat));
			sliders.Sort(new Comparison<BeatmapSaveData.SliderData>(BeatmapLevelDataModelSaver.SortByBeat));
			burstSliders.Sort(new Comparison<BeatmapSaveData.BurstSliderData>(BeatmapLevelDataModelSaver.SortByBeat));

			var vfxCollection = new BeatmapSaveData.FxEventsCollection();
			ValueTuple<List<BeatmapSaveData.LightColorEventBoxGroup>, List<BeatmapSaveData.LightRotationEventBoxGroup>, List<BeatmapSaveData.LightTranslationEventBoxGroup>, List<BeatmapSaveData.FxEventBoxGroup>> valueTuple = V3Saver._beatmapEventBoxGroupsDataModel.GetAllEventBoxGroups().OrderBy((EventBoxGroupEditorData e) => e.beat).Select(new Func<EventBoxGroupEditorData, List<BeatmapSaveData.EventBoxGroup>>(x=> CreateEventBoxGroupSaveDataWithFxCollection(x, vfxCollection)))
				.Aggregate(new ValueTuple<List<BeatmapSaveData.LightColorEventBoxGroup>, List<BeatmapSaveData.LightRotationEventBoxGroup>, List<BeatmapSaveData.LightTranslationEventBoxGroup>, List<BeatmapSaveData.FxEventBoxGroup>>(new List<BeatmapSaveData.LightColorEventBoxGroup>(), new List<BeatmapSaveData.LightRotationEventBoxGroup>(), new List<BeatmapSaveData.LightTranslationEventBoxGroup>(), new List<BeatmapSaveData.FxEventBoxGroup>()), new Func<ValueTuple<List<BeatmapSaveData.LightColorEventBoxGroup>, List<BeatmapSaveData.LightRotationEventBoxGroup>, List<BeatmapSaveData.LightTranslationEventBoxGroup>, List<BeatmapSaveData.FxEventBoxGroup>>, List<BeatmapSaveData.EventBoxGroup>, ValueTuple<List<BeatmapSaveData.LightColorEventBoxGroup>, List<BeatmapSaveData.LightRotationEventBoxGroup>, List<BeatmapSaveData.LightTranslationEventBoxGroup>, List<BeatmapSaveData.FxEventBoxGroup>>>(BeatmapLevelDataModelSaver.SplitEventBoxGroupsSaveData));
			List<BeatmapSaveData.LightColorEventBoxGroup> item = valueTuple.Item1;
			List<BeatmapSaveData.LightRotationEventBoxGroup> item2 = valueTuple.Item2;
			List<BeatmapSaveData.LightTranslationEventBoxGroup> item3 = valueTuple.Item3;
			List<BeatmapSaveData.FxEventBoxGroup> item4 = valueTuple.Item4;
			BeatmapSaveData.BasicEventTypesWithKeywords basicEventTypesWithKeywords = new BeatmapSaveData.BasicEventTypesWithKeywords(V3Saver._beatmapEventsDataModel.GetBasicEventTypesForKeywordData().Select(new Func<BasicEventTypesForKeywordEditorData, BeatmapSaveData.BasicEventTypesWithKeywords.BasicEventTypesForKeyword>(V3Saver.CreateBasicEventTypesForKeywordSaveData)).ToList());
			
			return new CustomBeatmapSaveDataVersioned(version.ToString(), bpmChanges, rotationEvents, colorNotes, bombNotes, obstacles, sliders, burstSliders, waypoints, basicEvents, colorBoostEvents, item, item2, item3, item4, vfxCollection, basicEventTypesWithKeywords, V3Saver._beatmapEventsDataModel.GetUseNormalEventsAsCompatibleEvents(), CustomDataRepository.GetCustomBeatmapSaveData().beatmapCustomData);
		}
	}
}
