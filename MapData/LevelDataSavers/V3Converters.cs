using V3CustomSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;
using V3 = BeatmapSaveDataVersion3;
using BeatmapEditor3D.DataModels;
using BeatmapSaveDataCommon;
using System.Collections.Generic;
using BeatmapSaveDataVersion3;
using EditorEX.Util;
using System.Linq;
using BeatmapEditor3D;
using System;
using BeatSaber.TrackDefinitions.DataModels;
using CombinedEventBoxGroup = (System.Collections.Generic.List<BeatmapSaveDataVersion3.LightColorEventBoxGroup>, System.Collections.Generic.List<BeatmapSaveDataVersion3.LightRotationEventBoxGroup>, System.Collections.Generic.List<BeatmapSaveDataVersion3.LightTranslationEventBoxGroup>, System.Collections.Generic.List<BeatmapSaveDataVersion3.FxEventBoxGroup>);
using EditorEX.CustomJSONData.CustomEvents;

public static class V3Converters
{
    public static bool SaveCustom(BaseEditorData data)
    {
        var customData = data.GetCustomData();
        return customData != null && !customData.IsEmpty;
    }

    public static V3.SliderData CreateSliderSaveData(ArcEditorData a)
    {
        return SaveCustom(a) ? new V3CustomSaveData.SliderSaveData((a.colorType == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB, a.beat, a.column, a.row, a.controlPointLengthMultiplier, (BeatmapSaveDataCommon.NoteCutDirection)a.cutDirection, a.tailBeat, a.tailColumn, a.tailRow, a.tailControlPointLengthMultiplier, (BeatmapSaveDataCommon.NoteCutDirection)a.tailCutDirection, (BeatmapSaveDataCommon.SliderMidAnchorMode)a.midAnchorMode, a.GetCustomData())
                                : new V3.SliderData((a.colorType == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB, a.beat, a.column, a.row, a.controlPointLengthMultiplier, (BeatmapSaveDataCommon.NoteCutDirection)a.cutDirection, a.tailBeat, a.tailColumn, a.tailRow, a.tailControlPointLengthMultiplier, (BeatmapSaveDataCommon.NoteCutDirection)a.tailCutDirection, (BeatmapSaveDataCommon.SliderMidAnchorMode)a.midAnchorMode);
    }

    public static ColorNoteData CreateColorNoteSaveData(NoteEditorData n)
    {
        return SaveCustom(n) ? new V3CustomSaveData.ColorNoteSaveData(n.beat, n.column, n.row, (n.type == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB, (BeatmapSaveDataCommon.NoteCutDirection)n.cutDirection, n.angle, n.GetCustomData())
                                : new ColorNoteData(n.beat, n.column, n.row, (n.type == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB, (BeatmapSaveDataCommon.NoteCutDirection)n.cutDirection, n.angle);
    }

    public static BombNoteData CreateBombNoteSaveData(NoteEditorData n)
    {
        return SaveCustom(n) ? new V3CustomSaveData.BombNoteSaveData(n.beat, n.column, n.row, n.GetCustomData())
                                : new BombNoteData(n.beat, n.column, n.row);
    }

    public static BasicEventData CreateBasicEventSaveData(BasicEventEditorData e, bool supportsFloatValue)
    {
        return SaveCustom(e) ? new V3CustomSaveData.BasicEventSaveData(e.beat, (BeatmapEventType)e.type, e.value, e.floatValue, e.GetCustomData())
                                : new BasicEventData(e.beat, (BeatmapEventType)e.type, e.value, supportsFloatValue ? e.floatValue : 0f);
    }

    public static V3.ObstacleData CreateObstacleSaveData(ObstacleEditorData o)
    {
        return SaveCustom(o) ? new V3CustomSaveData.ObstacleSaveData(o.beat, o.column, o.row, o.duration, o.width, o.height, o.GetCustomData())
                                : new V3.ObstacleData(o.beat, o.column, o.row, o.duration, o.width, o.height);
    }

    public static V3.WaypointData CreateWaypointSaveData(WaypointEditorData w)
    {
        return SaveCustom(w) ? new V3CustomSaveData.WaypointSaveData(w.beat, w.column, w.row, (BeatmapSaveDataCommon.OffsetDirection)w.offsetDirection, w.GetCustomData())
                                : new V3.WaypointData(w.beat, w.column, w.row, (BeatmapSaveDataCommon.OffsetDirection)w.offsetDirection);
    }

    public static ColorNoteData CreateColorNoteSaveDataFromChains(ChainEditorData c)
    {
        return SaveCustom(c) ? new V3CustomSaveData.ColorNoteSaveData(c.beat, c.column, c.row, (c.colorType == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB, (BeatmapSaveDataCommon.NoteCutDirection)c.cutDirection, 0, c.GetCustomData())
                                : new ColorNoteData(c.beat, c.column, c.row, (c.colorType == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB, (BeatmapSaveDataCommon.NoteCutDirection)c.cutDirection, 0);
    }

    public static BurstSliderData CreateBurstSliderSaveData(ChainEditorData c)
    {
        return SaveCustom(c) ? new V3CustomSaveData.BurstSliderSaveData((c.colorType == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB, c.beat, c.column, c.row, (BeatmapSaveDataCommon.NoteCutDirection)c.cutDirection, c.tailBeat, c.tailColumn, c.tailRow, c.sliceCount, c.squishAmount, c.GetCustomData())
                                : new BurstSliderData((c.colorType == ColorType.ColorA) ? NoteColorType.ColorA : NoteColorType.ColorB, c.beat, c.column, c.row, (BeatmapSaveDataCommon.NoteCutDirection)c.cutDirection, c.tailBeat, c.tailColumn, c.tailRow, c.sliceCount, c.squishAmount);
    }

    public static BpmChangeEventData CreateBpmChangeEventSaveData(BpmRegion r)
    {
        return new BpmChangeEventData(r.startBeat, r.bpm);
    }

    public static RotationEventData CreateRotationEventSaveData(BasicEventEditorData e)
    {
        return SaveCustom(e) ? new V3CustomSaveData.RotationEventSaveData(e.beat, (e.type == BasicBeatmapEventType.Event14) ? ExecutionTime.Early : ExecutionTime.Late, (float)e.value, e.GetCustomData())
                                : new RotationEventData(e.beat, (e.type == BasicBeatmapEventType.Event14) ? ExecutionTime.Early : ExecutionTime.Late, (float)e.value);
    }

    public static ColorBoostEventData CreateColorBoostSaveEventData(BasicEventEditorData e)
    {
        return SaveCustom(e) ? new V3CustomSaveData.ColorBoostEventSaveData(e.beat, e.value == 1, e.GetCustomData())
                                : new ColorBoostEventData(e.beat, e.value == 1);
    }

    public static BasicEventData CreateBasicEventSaveData(BasicEventEditorData e)
    {
        return SaveCustom(e) ? new V3CustomSaveData.BasicEventSaveData(e.beat, (BeatmapEventType)e.type, e.value, e.floatValue, e.GetCustomData())
                                : new BasicEventData(e.beat, (BeatmapEventType)e.type, e.value, e.floatValue);
    }

    private static BeatmapSaveDataCommon.EaseType ConvertEaseType(EaseType easeType)
    {
        return easeType switch
        {
            EaseType.None => BeatmapSaveDataCommon.EaseType.None,
            EaseType.Linear => BeatmapSaveDataCommon.EaseType.Linear,
            EaseType.InSine => BeatmapSaveDataCommon.EaseType.InSine,
            EaseType.OutSine => BeatmapSaveDataCommon.EaseType.OutSine,
            EaseType.InOutSine => BeatmapSaveDataCommon.EaseType.InOutSine,
            EaseType.InQuad => BeatmapSaveDataCommon.EaseType.InQuad,
            EaseType.OutQuad => BeatmapSaveDataCommon.EaseType.OutQuad,
            EaseType.InOutQuad => BeatmapSaveDataCommon.EaseType.InOutQuad,
            EaseType.InCubic => BeatmapSaveDataCommon.EaseType.InCubic,
            EaseType.OutCubic => BeatmapSaveDataCommon.EaseType.OutCubic,
            EaseType.InOutCubic => BeatmapSaveDataCommon.EaseType.InOutCubic,
            EaseType.InQuart => BeatmapSaveDataCommon.EaseType.InQuart,
            EaseType.OutQuart => BeatmapSaveDataCommon.EaseType.OutQuart,
            EaseType.InOutQuart => BeatmapSaveDataCommon.EaseType.InOutQuart,
            EaseType.InQuint => BeatmapSaveDataCommon.EaseType.InQuint,
            EaseType.OutQuint => BeatmapSaveDataCommon.EaseType.OutQuint,
            EaseType.InOutQuint => BeatmapSaveDataCommon.EaseType.InOutQuint,
            EaseType.InExpo => BeatmapSaveDataCommon.EaseType.InExpo,
            EaseType.OutExpo => BeatmapSaveDataCommon.EaseType.OutExpo,
            EaseType.InOutExpo => BeatmapSaveDataCommon.EaseType.InOutExpo,
            EaseType.InCirc => BeatmapSaveDataCommon.EaseType.InCirc,
            EaseType.OutCirc => BeatmapSaveDataCommon.EaseType.OutCirc,
            EaseType.InOutCirc => BeatmapSaveDataCommon.EaseType.InOutCirc,
            EaseType.InBack => BeatmapSaveDataCommon.EaseType.InBack,
            EaseType.OutBack => BeatmapSaveDataCommon.EaseType.OutBack,
            EaseType.InOutBack => BeatmapSaveDataCommon.EaseType.InOutBack,
            EaseType.InElastic => BeatmapSaveDataCommon.EaseType.InElastic,
            EaseType.OutElastic => BeatmapSaveDataCommon.EaseType.OutElastic,
            EaseType.InOutElastic => BeatmapSaveDataCommon.EaseType.InOutElastic,
            EaseType.InBounce => BeatmapSaveDataCommon.EaseType.InBounce,
            EaseType.OutBounce => BeatmapSaveDataCommon.EaseType.OutBounce,
            EaseType.InOutBounce => BeatmapSaveDataCommon.EaseType.InOutBounce,
            EaseType.BeatSaberInOutBack => BeatmapSaveDataCommon.EaseType.BeatSaberInOutBack,
            EaseType.BeatSaberInOutElastic => BeatmapSaveDataCommon.EaseType.BeatSaberInOutElastic,
            EaseType.BeatSaberInOutBounce => BeatmapSaveDataCommon.EaseType.BeatSaberInOutBounce,
            _ => BeatmapSaveDataCommon.EaseType.None,
        };
    }

    public static V3.IndexFilter CreateIndexFilter(IndexFilterEditorData f)
    {
        return new V3.IndexFilter((IndexFilterType)f.type, f.param0, f.param1, f.reversed, (IndexFilterRandomType)f.randomType, f.seed, f.chunks, f.limit, (IndexFilterLimitAlsoAffectsType)f.limitAlsoAffectType);
    }

    public static List<EventBoxGroup> CreateEventBoxGroupSaveDataWithFxCollection(EventBoxGroupEditorData e, FxEventsCollection fxEventsCollection, BeatmapEventBoxGroupsDataModel beatmapEventBoxGroupsDataModel)
    {
        IReadOnlyList<EventBoxEditorData> eventBoxesByEventBoxGroupId = beatmapEventBoxGroupsDataModel.GetEventBoxesByEventBoxGroupId(e.id);
        List<EventBoxGroup> list = new List<EventBoxGroup>();
        EventBoxGroupType type = e.type;
        switch (type)
        {
            case EventBoxGroupType.Color:
                {
                    V3CustomSaveData.LightColorEventBoxGroupSaveData lightColorEventBoxGroup = new V3CustomSaveData.LightColorEventBoxGroupSaveData(e.beat, e.groupId, eventBoxesByEventBoxGroupId.Select((EventBoxEditorData eventBox) => (LightColorEventBoxEditorData)eventBox).Select((LightColorEventBoxEditorData eventBox) => new LightColorEventBox(CreateIndexFilter(eventBox.indexFilter), eventBox.beatDistributionParam, (DistributionParamType)eventBox.beatDistributionParamType, eventBox.brightnessDistributionParam, eventBox.brightnessDistributionShouldAffectFirstBaseEvent, (DistributionParamType)eventBox.brightnessDistributionParamType, ConvertEaseType(eventBox.brightnessDistributionEaseType), beatmapEventBoxGroupsDataModel.GetBaseEventsListByEventBoxId<LightColorBaseEditorData>(eventBox.id).OrderBy((LightColorBaseEditorData i) => i.beat).Select((LightColorBaseEditorData data) => new V3.LightColorBaseData(data.beat, data.easeLeadType == EaseLeadType.InOut ? TransitionType.Interpolate : TransitionType.Extend /* TODO: *sigh* */, (BeatmapSaveDataCommon.EnvironmentColorType)data.colorType, data.brightness, data.strobeBeatFrequency, data.strobeBrightness, data.strobeFade))
                        .ToList())).ToList(), e.GetCustomData());
                    list.Add(lightColorEventBoxGroup);
                    break;
                }
            case EventBoxGroupType.Rotation:
                {
                    V3CustomSaveData.LightRotationEventBoxGroupSaveData lightRotationEventBoxGroup = new V3CustomSaveData.LightRotationEventBoxGroupSaveData(e.beat, e.groupId, eventBoxesByEventBoxGroupId.Select((EventBoxEditorData eventBox) => (LightRotationEventBoxEditorData)eventBox).Select((LightRotationEventBoxEditorData eventBox) => new LightRotationEventBox(CreateIndexFilter(eventBox.indexFilter), eventBox.beatDistributionParam, (DistributionParamType)eventBox.beatDistributionParamType, eventBox.rotationDistributionParam, (DistributionParamType)eventBox.rotationDistributionParamType, eventBox.rotationDistributionShouldAffectFirstBaseEvent, ConvertEaseType(eventBox.rotationDistributionEaseType), (Axis)eventBox.axis, eventBox.flipRotation, beatmapEventBoxGroupsDataModel.GetBaseEventsListByEventBoxId<LightRotationBaseEditorData>(eventBox.id).OrderBy((LightRotationBaseEditorData i) => i.beat).Select(delegate (LightRotationBaseEditorData data)
                    {
                        float beat = data.beat;
                        BeatmapSaveDataCommon.EaseType easeType = ConvertEaseType(new ValueTuple<EaseLeadType, EaseCurveType>(data.easeLeadType, data.easeCurveType).ToEaseType());
                        int loopsCount = data.loopsCount;
                        int rotation = (int)data.rotation; //TODO: Investigate float->int
                        return new V3.LightRotationBaseData(beat, data.usePreviousValue, easeType, loopsCount, rotation, (RotationDirection)data.rotationDirection);
                    }).ToList())).ToList(), e.GetCustomData());
                    list.Add(lightRotationEventBoxGroup);
                    break;
                }
            case EventBoxGroupType.Translation:
                {
                    LightTranslationEventBoxGroup lightTranslationEventBoxGroup = new LightTranslationEventBoxGroup(e.beat, e.groupId, eventBoxesByEventBoxGroupId.Select((EventBoxEditorData eventBox) => (LightTranslationEventBoxEditorData)eventBox).Select((LightTranslationEventBoxEditorData eventBox) => new LightTranslationEventBox(CreateIndexFilter(eventBox.indexFilter), eventBox.beatDistributionParam, (DistributionParamType)eventBox.beatDistributionParamType, eventBox.gapDistributionParam, (DistributionParamType)eventBox.gapDistributionParamType, eventBox.gapDistributionShouldAffectFirstBaseEvent, ConvertEaseType(eventBox.gapDistributionEaseType), (Axis)eventBox.axis, eventBox.flipTranslation, beatmapEventBoxGroupsDataModel.GetBaseEventsListByEventBoxId<LightTranslationBaseEditorData>(eventBox.id).OrderBy((LightTranslationBaseEditorData i) => i.beat).Select(delegate (LightTranslationBaseEditorData data)
                    {
                        float beat2 = data.beat;
                        BeatmapSaveDataCommon.EaseType easeType2 = ConvertEaseType(new ValueTuple<EaseLeadType, EaseCurveType>(data.easeLeadType, data.easeCurveType).ToEaseType());
                        float translation = data.translation;
                        return new V3.LightTranslationBaseData(beat2, data.usePreviousValue, easeType2, translation);
                    })
                        .ToList())).ToList());
                    list.Add(lightTranslationEventBoxGroup);
                    break;
                }
            default:
                if (type == EventBoxGroupType.FloatFx)
                {
                    FxEventBoxGroup fxEventBoxGroup = new FxEventBoxGroup(e.beat, e.groupId, FxEventType.Float, eventBoxesByEventBoxGroupId.Select((EventBoxEditorData eventBox) => (FxEventBoxEditorData)eventBox).Select(delegate (FxEventBoxEditorData eventBox)
                    {
                        V3.IndexFilter indexFilter = CreateIndexFilter(eventBox.indexFilter);
                        float beatDistributionParam = eventBox.beatDistributionParam;
                        DistributionParamType beatDistributionParamType = (DistributionParamType)eventBox.beatDistributionParamType;
                        float vfxDistributionParam = eventBox.vfxDistributionParam;
                        DistributionParamType vfxDistributionParamType = (DistributionParamType)eventBox.vfxDistributionParamType;
                        BeatmapSaveDataCommon.EaseType easeType3 = ConvertEaseType(eventBox.vfxDistributionEaseType);
                        bool vfxDistributionShouldAffectFirstBaseEvent = eventBox.vfxDistributionShouldAffectFirstBaseEvent;
                        IEnumerable<FloatFxBaseEditorData> enumerable = beatmapEventBoxGroupsDataModel.GetBaseEventsListByEventBoxId<FloatFxBaseEditorData>(eventBox.id).OrderBy((FloatFxBaseEditorData i) => i.beat);
                        Func<FloatFxBaseEditorData, int> func = (FloatFxBaseEditorData data) => fxEventsCollection.AddEventAndGetIndex(new FloatFxEventBaseData(data.beat, data.usePreviousValue, data.value, ConvertEaseType(new ValueTuple<EaseLeadType, EaseCurveType>(data.easeLeadType, data.easeCurveType).ToEaseType())));
                        return new FxEventBox(indexFilter, beatDistributionParam, beatDistributionParamType, vfxDistributionParam, vfxDistributionParamType, easeType3, vfxDistributionShouldAffectFirstBaseEvent, enumerable.Select(func).ToList());
                    }).ToList());
                    list.Add(fxEventBoxGroup);
                }
                break;
        }
        return list;
    }

    public static BasicEventTypesWithKeywords.BasicEventTypesForKeyword CreateBasicEventTypesForKeywordSaveData(BasicEventTypesForKeywordEditorData data)
    {
        return new BasicEventTypesWithKeywords.BasicEventTypesForKeyword(data.keyword, data.eventTypes.Select((BasicBeatmapEventType e) => (BeatmapEventType)e).ToList());
    }
    public static CombinedEventBoxGroup SplitEventBoxGroupsSaveData((List<LightColorEventBoxGroup> colors, List<LightRotationEventBoxGroup> rotations, List<LightTranslationEventBoxGroup> translations, List<FxEventBoxGroup> vfxEventBoxGroups) acc, IEnumerable<EventBoxGroup> eventBoxGroups)
    {
        foreach (EventBoxGroup eventBoxGroup in eventBoxGroups)
        {
            if (!(eventBoxGroup is LightColorEventBoxGroup item))
            {
                if (!(eventBoxGroup is LightRotationEventBoxGroup item2))
                {
                    if (!(eventBoxGroup is LightTranslationEventBoxGroup item3))
                    {
                        if (eventBoxGroup is FxEventBoxGroup item4)
                        {
                            acc.vfxEventBoxGroups.Add(item4);
                        }
                    }
                    else
                    {
                        acc.translations.Add(item3);
                    }
                }
                else
                {
                    acc.rotations.Add(item2);
                }
            }
            else
            {
                acc.colors.Add(item);
            }
        }
        return acc;
    }

    public static V3CustomSaveData.CustomEventSaveData CreateCustomEventSaveData(CustomEventEditorData data)
    {
        return new V3CustomSaveData.CustomEventSaveData(data.beat, data.eventType, data.customData);
    }
}