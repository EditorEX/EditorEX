using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using BeatmapSaveDataCommon;
using BeatmapSaveDataVersion3;
using BeatSaber.TrackDefinitions.DataModels;
using EditorEX.CustomJSONData;
using EditorEX.Util;
using CombinedEventBoxGroup = (
    System.Collections.Generic.List<BeatmapSaveDataVersion3.LightColorEventBoxGroup>,
    System.Collections.Generic.List<BeatmapSaveDataVersion3.LightRotationEventBoxGroup>,
    System.Collections.Generic.List<BeatmapSaveDataVersion3.LightTranslationEventBoxGroup>,
    System.Collections.Generic.List<BeatmapSaveDataVersion3.FxEventBoxGroup>
);
using V3 = BeatmapSaveDataVersion3;
using V3CustomSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;
using V4 = BeatmapSaveDataVersion4;

namespace EditorEX.MapData.Objects
{
    public static class EventBoxGroupCodec
    {
        public static TransitionType SaveLightColorTransition(LightColorBaseEditorData data)
        {
            if (data.usePreviousValue)
            {
                return TransitionType.Extend;
            }

            return data.easeLeadType == EaseLeadType.InOut
                ? TransitionType.Interpolate
                : TransitionType.Instant;
        }

        public static V3.IndexFilter CreateIndexFilter(IndexFilterEditorData f)
        {
            return new V3.IndexFilter(
                (IndexFilterType)f.type,
                f.param0,
                f.param1,
                f.reversed,
                (IndexFilterRandomType)f.randomType,
                f.seed,
                f.chunks,
                f.limit,
                (IndexFilterLimitAlsoAffectsType)f.limitAlsoAffectType
            );
        }

        public static List<EventBoxGroup> SaveV3(
            EventBoxGroupEditorData e,
            FxEventsCollection fxEventsCollection,
            BeatmapEventBoxGroupsDataModel beatmapEventBoxGroupsDataModel,
            ICustomDataRepository customDataRepository
        )
        {
            IReadOnlyList<EventBoxEditorData> boxes =
                beatmapEventBoxGroupsDataModel.GetEventBoxesByEventBoxGroupId(e.id);
            var baseLists = new List<(BeatmapEditorObjectId, List<BaseEditorData>)>();
            foreach (EventBoxEditorData box in boxes)
            {
                baseLists.Add(
                    (box.id, GetBaseEventsFromModel(beatmapEventBoxGroupsDataModel, e.type, box.id))
                );
            }

            return SaveV3FromInput(
                new BeatmapEditorEventBoxGroupInput
                {
                    eventBoxGroup = e,
                    eventBoxes = boxes,
                    baseLists = baseLists,
                },
                fxEventsCollection,
                customDataRepository
            );
        }

        public static List<BaseEditorData> GetBaseEventsFromModel(
            BeatmapEventBoxGroupsDataModel model,
            EventBoxGroupType type,
            BeatmapEditorObjectId boxId
        )
        {
            return type switch
            {
                EventBoxGroupType.Color => model
                    .GetBaseEventsListByEventBoxId<LightColorBaseEditorData>(boxId)
                    .Cast<BaseEditorData>()
                    .ToList(),
                EventBoxGroupType.Rotation => model
                    .GetBaseEventsListByEventBoxId<LightRotationBaseEditorData>(boxId)
                    .Cast<BaseEditorData>()
                    .ToList(),
                EventBoxGroupType.Translation => model
                    .GetBaseEventsListByEventBoxId<LightTranslationBaseEditorData>(boxId)
                    .Cast<BaseEditorData>()
                    .ToList(),
                EventBoxGroupType.FloatFx => model
                    .GetBaseEventsListByEventBoxId<FloatFxBaseEditorData>(boxId)
                    .Cast<BaseEditorData>()
                    .ToList(),
                _ => new List<BaseEditorData>(),
            };
        }

        public static List<EventBoxGroup> SaveV3FromInput(
            BeatmapEditorEventBoxGroupInput input,
            FxEventsCollection fxEventsCollection,
            ICustomDataRepository customDataRepository
        )
        {
            EventBoxGroupEditorData e = input.eventBoxGroup;
            IEnumerable<EventBoxEditorData> eventBoxesByEventBoxGroupId =
                input.eventBoxes ?? Array.Empty<EventBoxEditorData>();
            var baseById = new Dictionary<BeatmapEditorObjectId, List<BaseEditorData>>();
            if (input.baseLists != null)
            {
                foreach (var (id, events) in input.baseLists)
                {
                    baseById[id] = events;
                }
            }

            IEnumerable<T> Bases<T>(BeatmapEditorObjectId id)
                where T : BaseEditorData
            {
                return baseById.TryGetValue(id, out var events)
                    ? events.OfType<T>()
                    : Enumerable.Empty<T>();
            }

            List<EventBoxGroup> list = new List<EventBoxGroup>();
            EventBoxGroupType type = e.type;
            switch (type)
            {
                case EventBoxGroupType.Color:
                {
                    V3CustomSaveData.LightColorEventBoxGroupSaveData lightColorEventBoxGroup =
                        new V3CustomSaveData.LightColorEventBoxGroupSaveData(
                            e.beat,
                            e.groupId,
                            eventBoxesByEventBoxGroupId
                                .Select(
                                    (EventBoxEditorData eventBox) =>
                                        (LightColorEventBoxEditorData)eventBox
                                )
                                .Select(
                                    (LightColorEventBoxEditorData eventBox) =>
                                        new LightColorEventBox(
                                            CreateIndexFilter(eventBox.indexFilter),
                                            eventBox.beatDistributionParam,
                                            (DistributionParamType)
                                                eventBox.beatDistributionParamType,
                                            eventBox.brightnessDistributionParam,
                                            eventBox.brightnessDistributionShouldAffectFirstBaseEvent,
                                            (DistributionParamType)
                                                eventBox.brightnessDistributionParamType,
                                            ConvertEaseType(
                                                eventBox.brightnessDistributionEaseType
                                            ),
                                            Bases<LightColorBaseEditorData>(eventBox.id)
                                                .OrderBy((LightColorBaseEditorData i) => i.beat)
                                                .Select(
                                                    (LightColorBaseEditorData data) =>
                                                        new V3.LightColorBaseData(
                                                            data.beat,
                                                            SaveLightColorTransition(data),
                                                            (BeatmapSaveDataCommon.EnvironmentColorType)
                                                                data.colorType,
                                                            data.brightness,
                                                            data.strobeBeatFrequency,
                                                            data.strobeBrightness,
                                                            data.strobeFade
                                                        )
                                                )
                                                .ToList()
                                        )
                                )
                                .ToList(),
                            e.GetCustomData(customDataRepository)
                        );
                    list.Add(lightColorEventBoxGroup);
                    break;
                }
                case EventBoxGroupType.Rotation:
                {
                    V3CustomSaveData.LightRotationEventBoxGroupSaveData lightRotationEventBoxGroup =
                        new V3CustomSaveData.LightRotationEventBoxGroupSaveData(
                            e.beat,
                            e.groupId,
                            eventBoxesByEventBoxGroupId
                                .Select(
                                    (EventBoxEditorData eventBox) =>
                                        (LightRotationEventBoxEditorData)eventBox
                                )
                                .Select(
                                    (LightRotationEventBoxEditorData eventBox) =>
                                        new LightRotationEventBox(
                                            CreateIndexFilter(eventBox.indexFilter),
                                            eventBox.beatDistributionParam,
                                            (DistributionParamType)
                                                eventBox.beatDistributionParamType,
                                            eventBox.rotationDistributionParam,
                                            (DistributionParamType)
                                                eventBox.rotationDistributionParamType,
                                            eventBox.rotationDistributionShouldAffectFirstBaseEvent,
                                            ConvertEaseType(eventBox.rotationDistributionEaseType),
                                            (Axis)eventBox.axis,
                                            eventBox.flipRotation,
                                            Bases<LightRotationBaseEditorData>(eventBox.id)
                                                .OrderBy((LightRotationBaseEditorData i) => i.beat)
                                                .Select(
                                                    delegate(LightRotationBaseEditorData data)
                                                    {
                                                        float beat = data.beat;
                                                        BeatmapSaveDataCommon.EaseType easeType =
                                                            ConvertEaseType(
                                                                new ValueTuple<
                                                                    EaseLeadType,
                                                                    EaseCurveType
                                                                >(
                                                                    data.easeLeadType,
                                                                    data.easeCurveType
                                                                ).ToEaseType()
                                                            );
                                                        int loopsCount = data.loopsCount;
                                                        int rotation = (int)data.rotation;
                                                        return new V3.LightRotationBaseData(
                                                            beat,
                                                            data.usePreviousValue,
                                                            easeType,
                                                            loopsCount,
                                                            rotation,
                                                            (RotationDirection)
                                                                data.rotationDirection
                                                        );
                                                    }
                                                )
                                                .ToList()
                                        )
                                )
                                .ToList(),
                            e.GetCustomData(customDataRepository)
                        );
                    list.Add(lightRotationEventBoxGroup);
                    break;
                }
                case EventBoxGroupType.Translation:
                {
                    LightTranslationEventBoxGroup lightTranslationEventBoxGroup =
                        new LightTranslationEventBoxGroup(
                            e.beat,
                            e.groupId,
                            eventBoxesByEventBoxGroupId
                                .Select(
                                    (EventBoxEditorData eventBox) =>
                                        (LightTranslationEventBoxEditorData)eventBox
                                )
                                .Select(
                                    (LightTranslationEventBoxEditorData eventBox) =>
                                        new LightTranslationEventBox(
                                            CreateIndexFilter(eventBox.indexFilter),
                                            eventBox.beatDistributionParam,
                                            (DistributionParamType)
                                                eventBox.beatDistributionParamType,
                                            eventBox.gapDistributionParam,
                                            (DistributionParamType)
                                                eventBox.gapDistributionParamType,
                                            eventBox.gapDistributionShouldAffectFirstBaseEvent,
                                            ConvertEaseType(eventBox.gapDistributionEaseType),
                                            (Axis)eventBox.axis,
                                            eventBox.flipTranslation,
                                            Bases<LightTranslationBaseEditorData>(eventBox.id)
                                                .OrderBy(
                                                    (LightTranslationBaseEditorData i) => i.beat
                                                )
                                                .Select(
                                                    delegate(LightTranslationBaseEditorData data)
                                                    {
                                                        float beat2 = data.beat;
                                                        BeatmapSaveDataCommon.EaseType easeType2 =
                                                            ConvertEaseType(
                                                                new ValueTuple<
                                                                    EaseLeadType,
                                                                    EaseCurveType
                                                                >(
                                                                    data.easeLeadType,
                                                                    data.easeCurveType
                                                                ).ToEaseType()
                                                            );
                                                        float translation = data.translation;
                                                        return new V3.LightTranslationBaseData(
                                                            beat2,
                                                            data.usePreviousValue,
                                                            easeType2,
                                                            translation
                                                        );
                                                    }
                                                )
                                                .ToList()
                                        )
                                )
                                .ToList()
                        );
                    list.Add(lightTranslationEventBoxGroup);
                    break;
                }
                default:
                    if (type == EventBoxGroupType.FloatFx)
                    {
                        FxEventBoxGroup fxEventBoxGroup = new FxEventBoxGroup(
                            e.beat,
                            e.groupId,
                            FxEventType.Float,
                            eventBoxesByEventBoxGroupId
                                .Select(
                                    (EventBoxEditorData eventBox) => (FxEventBoxEditorData)eventBox
                                )
                                .Select(
                                    delegate(FxEventBoxEditorData eventBox)
                                    {
                                        V3.IndexFilter indexFilter = CreateIndexFilter(
                                            eventBox.indexFilter
                                        );
                                        float beatDistributionParam =
                                            eventBox.beatDistributionParam;
                                        DistributionParamType beatDistributionParamType =
                                            (DistributionParamType)
                                                eventBox.beatDistributionParamType;
                                        float vfxDistributionParam = eventBox.vfxDistributionParam;
                                        DistributionParamType vfxDistributionParamType =
                                            (DistributionParamType)
                                                eventBox.vfxDistributionParamType;
                                        BeatmapSaveDataCommon.EaseType easeType3 = ConvertEaseType(
                                            eventBox.vfxDistributionEaseType
                                        );
                                        bool vfxDistributionShouldAffectFirstBaseEvent =
                                            eventBox.vfxDistributionShouldAffectFirstBaseEvent;
                                        IEnumerable<FloatFxBaseEditorData> enumerable =
                                            Bases<FloatFxBaseEditorData>(eventBox.id)
                                                .OrderBy((FloatFxBaseEditorData i) => i.beat);
                                        Func<FloatFxBaseEditorData, int> func = (
                                            FloatFxBaseEditorData data
                                        ) =>
                                            fxEventsCollection.AddEventAndGetIndex(
                                                new FloatFxEventBaseData(
                                                    data.beat,
                                                    data.usePreviousValue,
                                                    data.value,
                                                    ConvertEaseType(
                                                        new ValueTuple<EaseLeadType, EaseCurveType>(
                                                            data.easeLeadType,
                                                            data.easeCurveType
                                                        ).ToEaseType()
                                                    )
                                                )
                                            );
                                        return new FxEventBox(
                                            indexFilter,
                                            beatDistributionParam,
                                            beatDistributionParamType,
                                            vfxDistributionParam,
                                            vfxDistributionParamType,
                                            easeType3,
                                            vfxDistributionShouldAffectFirstBaseEvent,
                                            enumerable.Select(func).ToList()
                                        );
                                    }
                                )
                                .ToList()
                        );
                        list.Add(fxEventBoxGroup);
                    }
                    break;
            }
            return list;
        }

        public static CombinedEventBoxGroup SplitV3(
            CombinedEventBoxGroup acc,
            IEnumerable<EventBoxGroup> eventBoxGroups
        )
        {
            foreach (EventBoxGroup eventBoxGroup in eventBoxGroups)
            {
                switch (eventBoxGroup)
                {
                    case LightColorEventBoxGroup item:
                        acc.Item1.Add(item);
                        break;
                    case LightRotationEventBoxGroup item2:
                        acc.Item2.Add(item2);
                        break;
                    case LightTranslationEventBoxGroup item3:
                        acc.Item3.Add(item3);
                        break;
                    case FxEventBoxGroup item4:
                        acc.Item4.Add(item4);
                        break;
                }
            }
            return acc;
        }

        public static List<BeatmapEditorEventBoxGroupInput> LoadV3(V3CustomSaveData beatmapSaveData)
        {
            return new List<BeatmapEditorEventBoxGroupInput>()
                .Concat(
                    beatmapSaveData.lightColorEventBoxGroups.Select(
                        BeatmapDataModelsLoader.CreateLightColorEventBoxGroup_v3
                    )
                )
                .Concat(
                    beatmapSaveData.lightRotationEventBoxGroups.Select(
                        BeatmapDataModelsLoader.CreateLightRotationEventBoxGroup_v3
                    )
                )
                .Concat(
                    beatmapSaveData.lightTranslationEventBoxGroups.Select(
                        BeatmapDataModelsLoader.CreateLightTranslationEventBoxGroup_v3
                    )
                )
                .Concat(
                    beatmapSaveData.vfxEventBoxGroups.Select(x =>
                        BeatmapDataModelsLoader.CreateFxEventBoxGroupWithFxEventsCollection_v3(
                            x,
                            beatmapSaveData._fxEventsCollection
                        )
                    )
                )
                .OrderBy(e => e.eventBoxGroup.beat)
                .ToList();
        }

        public static void SaveV4(
            BeatmapEventBoxGroupsDataModel beatmapEventBoxGroupsDataModel,
            V4.LightshowSaveData lightshow
        )
        {
            var inputs = new List<BeatmapEditorEventBoxGroupInput>();
            foreach (
                EventBoxGroupEditorData group in beatmapEventBoxGroupsDataModel.GetAllEventBoxGroups()
            )
            {
                IReadOnlyList<EventBoxEditorData> boxes =
                    beatmapEventBoxGroupsDataModel.GetEventBoxesByEventBoxGroupId(group.id);
                var baseLists = new List<(BeatmapEditorObjectId, List<BaseEditorData>)>();
                foreach (EventBoxEditorData box in boxes)
                {
                    baseLists.Add(
                        (
                            box.id,
                            GetBaseEventsFromModel(
                                beatmapEventBoxGroupsDataModel,
                                group.type,
                                box.id
                            )
                        )
                    );
                }

                inputs.Add(
                    new BeatmapEditorEventBoxGroupInput
                    {
                        eventBoxGroup = group,
                        eventBoxes = boxes,
                        baseLists = baseLists,
                    }
                );
            }

            SaveV4FromInput(inputs, lightshow);
        }

        public static void SaveV4FromInput(
            IEnumerable<BeatmapEditorEventBoxGroupInput> groups,
            V4.LightshowSaveData lightshow
        )
        {
            List<V4.EventBoxGroup> list = new List<V4.EventBoxGroup>();
            var (map, list2) = BeatmapSaverUtils.CreateEventsStorage<V4.IndexFilter>();
            var (lightColorEventBoxesMap, lightColorEventBoxesData) =
                BeatmapSaverUtils.CreateEventsStorage<V4.LightColorEventBox>();
            var (lightColorEventsMap, lightColorEventsData) =
                BeatmapSaverUtils.CreateEventsStorage<V4.LightColorEvent>();
            var (lightRotationEventBoxesMap, lightRotationEventBoxesData) =
                BeatmapSaverUtils.CreateEventsStorage<V4.LightRotationEventBox>();
            var (lightRotationEventsMap, lightRotationEventsData) =
                BeatmapSaverUtils.CreateEventsStorage<V4.LightRotationEvent>();
            var (lightTranslationEventBoxesMap, lightTranslationEventBoxesData) =
                BeatmapSaverUtils.CreateEventsStorage<V4.LightTranslationEventBox>();
            var (lightTranslationEventsMap, lightTranslationEventsData) =
                BeatmapSaverUtils.CreateEventsStorage<V4.LightTranslationEvent>();
            var (fxEventBoxesMap, fxEventBoxesData) =
                BeatmapSaverUtils.CreateEventsStorage<V4.FxEventBox>();
            var (floatFxEventsMap, floatFxEventsData) =
                BeatmapSaverUtils.CreateEventsStorage<V4.FloatFxEvent>();

            foreach (BeatmapEditorEventBoxGroupInput input in groups)
            {
                EventBoxGroupEditorData allEventBoxGroup = input.eventBoxGroup;
                var baseById = new Dictionary<BeatmapEditorObjectId, List<BaseEditorData>>();
                if (input.baseLists != null)
                {
                    foreach (var (id, events) in input.baseLists)
                    {
                        baseById[id] = events;
                    }
                }

                List<V4.EventBox> list3 = new List<V4.EventBox>();
                foreach (
                    EventBoxEditorData item in input.eventBoxes ?? Array.Empty<EventBoxEditorData>()
                )
                {
                    int index = BeatmapSaverUtils.GetIndex(
                        LightshowSaver.ConvertIndexFilter(item.indexFilter),
                        map,
                        list2
                    );
                    int e = GetEventBoxIndex(item);
                    V4.BeatIndex[] l = GetBaseEventsBeatIndexes(allEventBoxGroup.type, item.id);
                    list3.Add(
                        new V4.EventBox
                        {
                            f = index,
                            e = e,
                            l = l,
                        }
                    );
                }

                list.Add(
                    new V4.EventBoxGroup
                    {
                        b = allEventBoxGroup.beat,
                        g = allEventBoxGroup.groupId,
                        t = ConvertEventBoxGroupTypeToV4(allEventBoxGroup.type),
                        e = list3.ToArray(),
                    }
                );

                V4.BeatIndex[] GetBaseEventsBeatIndexes(
                    EventBoxGroupType type,
                    BeatmapEditorObjectId id
                )
                {
                    IEnumerable<BaseEditorData> bases = baseById.TryGetValue(id, out var events)
                        ? events
                        : Enumerable.Empty<BaseEditorData>();
                    return type switch
                    {
                        EventBoxGroupType.Color => bases
                            .OfType<LightColorBaseEditorData>()
                            .Select(lightColorBaseEditorData => new V4.BeatIndex
                            {
                                b = lightColorBaseEditorData.beat,
                                i = BeatmapSaverUtils.GetIndex(
                                    ConvertLightColorBaseEvent(lightColorBaseEditorData),
                                    lightColorEventsMap,
                                    lightColorEventsData
                                ),
                            })
                            .ToArray(),
                        EventBoxGroupType.Rotation => bases
                            .OfType<LightRotationBaseEditorData>()
                            .Select(lightRotationBaseEditorData => new V4.BeatIndex
                            {
                                b = lightRotationBaseEditorData.beat,
                                i = BeatmapSaverUtils.GetIndex(
                                    ConvertLightRotationBaseEvent(lightRotationBaseEditorData),
                                    lightRotationEventsMap,
                                    lightRotationEventsData
                                ),
                            })
                            .ToArray(),
                        EventBoxGroupType.Translation => bases
                            .OfType<LightTranslationBaseEditorData>()
                            .Select(lightTranslationBaseEditorData => new V4.BeatIndex
                            {
                                b = lightTranslationBaseEditorData.beat,
                                i = BeatmapSaverUtils.GetIndex(
                                    ConvertLightTranslationBaseEvent(
                                        lightTranslationBaseEditorData
                                    ),
                                    lightTranslationEventsMap,
                                    lightTranslationEventsData
                                ),
                            })
                            .ToArray(),
                        EventBoxGroupType.FloatFx => bases
                            .OfType<FloatFxBaseEditorData>()
                            .Select(floatFxBaseEditorData => new V4.BeatIndex
                            {
                                b = floatFxBaseEditorData.beat,
                                i = BeatmapSaverUtils.GetIndex(
                                    ConvertFloatFxBaseEvent(floatFxBaseEditorData),
                                    floatFxEventsMap,
                                    floatFxEventsData
                                ),
                            })
                            .ToArray(),
                        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
                    };
                }
            }

            lightshow.eventBoxGroups = list.ToArray();
            lightshow.indexFilters = list2.ToArray();
            lightshow.lightColorEventBoxes = lightColorEventBoxesData.ToArray();
            lightshow.lightColorEvents = lightColorEventsData.ToArray();
            lightshow.lightRotationEventBoxes = lightRotationEventBoxesData.ToArray();
            lightshow.lightRotationEvents = lightRotationEventsData.ToArray();
            lightshow.lightTranslationEventBoxes = lightTranslationEventBoxesData.ToArray();
            lightshow.lightTranslationEvents = lightTranslationEventsData.ToArray();
            lightshow.fxEventBoxes = fxEventBoxesData.ToArray();
            lightshow.floatFxEvents = floatFxEventsData.ToArray();

            int GetEventBoxIndex(EventBoxEditorData eventBox)
            {
                return eventBox switch
                {
                    LightColorEventBoxEditorData b => BeatmapSaverUtils.GetIndex(
                        ConvertLightColorEventBox(b),
                        lightColorEventBoxesMap,
                        lightColorEventBoxesData
                    ),
                    LightRotationEventBoxEditorData b2 => BeatmapSaverUtils.GetIndex(
                        ConvertLightRotationEventBox(b2),
                        lightRotationEventBoxesMap,
                        lightRotationEventBoxesData
                    ),
                    LightTranslationEventBoxEditorData b3 => BeatmapSaverUtils.GetIndex(
                        ConvertLightTranslationEventBox(b3),
                        lightTranslationEventBoxesMap,
                        lightTranslationEventBoxesData
                    ),
                    FxEventBoxEditorData b4 => BeatmapSaverUtils.GetIndex(
                        ConvertFxEventBox(b4),
                        fxEventBoxesMap,
                        fxEventBoxesData
                    ),
                    _ => throw new ArgumentOutOfRangeException(nameof(eventBox)),
                };
            }
        }

        public static List<BeatmapEditorEventBoxGroupInput> LoadV4(V4.LightshowSaveData lightshow)
        {
            return lightshow.eventBoxGroups.Select(CreateEventBoxGroupV4).ToList();

            BeatmapEditorEventBoxGroupInput CreateEventBoxGroupV4(V4.EventBoxGroup g)
            {
                var (eventBoxes, baseLists) = CreateEventBoxEditorDataV4(g.t, g.e);
                return new BeatmapEditorEventBoxGroupInput
                {
                    eventBoxGroup = EventBoxGroupEditorData.CreateNew(
                        g.b,
                        g.g,
                        ConvertEventBoxGroupTypeFromV4(g.t)
                    ),
                    eventBoxes = eventBoxes,
                    baseLists = baseLists,
                };
            }

            (
                IEnumerable<EventBoxEditorData> eventBoxes,
                IEnumerable<(BeatmapEditorObjectId, List<BaseEditorData>)> baseLists
            ) CreateEventBoxEditorDataV4(V4.EventBoxGroupType t, IEnumerable<V4.EventBox> boxes)
            {
                List<EventBoxEditorData> list = new List<EventBoxEditorData>();
                List<(BeatmapEditorObjectId, List<BaseEditorData>)> list2 = new();
                foreach (V4.EventBox box in boxes)
                {
                    IndexFilterEditorData indexFilter = CreateIndexFilterV4(
                        lightshow.indexFilters[box.f]
                    );
                    EventBoxEditorData eventBoxEditorData = t switch
                    {
                        V4.EventBoxGroupType.Color => CreateLightColorEventBox(
                            lightshow.lightColorEventBoxes[box.e],
                            indexFilter
                        ),
                        V4.EventBoxGroupType.Rotation => CreateLightRotationEventBox(
                            lightshow.lightRotationEventBoxes[box.e],
                            indexFilter
                        ),
                        V4.EventBoxGroupType.Translation => CreateLightTranslationEventBox(
                            lightshow.lightTranslationEventBoxes[box.e],
                            indexFilter
                        ),
                        V4.EventBoxGroupType.FloatFx => CreateFxEventBox(
                            lightshow.fxEventBoxes[box.e],
                            indexFilter
                        ),
                        _ => throw new ArgumentOutOfRangeException(nameof(t), t, null),
                    };
                    List<BaseEditorData> item = t switch
                    {
                        V4.EventBoxGroupType.Color => box
                            .l.Select(c =>
                                CreateLightColorBaseData(c.beat, lightshow.lightColorEvents[c.i])
                            )
                            .ToList(),
                        V4.EventBoxGroupType.Rotation => box
                            .l.Select(c =>
                                CreateLightRotationBaseData(
                                    c.beat,
                                    lightshow.lightRotationEvents[c.i]
                                )
                            )
                            .ToList(),
                        V4.EventBoxGroupType.Translation => box
                            .l.Select(c =>
                                CreateLightTranslationBaseData(
                                    c.beat,
                                    lightshow.lightTranslationEvents[c.i]
                                )
                            )
                            .ToList(),
                        V4.EventBoxGroupType.FloatFx => box
                            .l.Select(c =>
                                CreateFloatFxBaseData(c.beat, lightshow.floatFxEvents[c.i])
                            )
                            .ToList(),
                        _ => throw new ArgumentOutOfRangeException(nameof(t), t, null),
                    };
                    list.Add(eventBoxEditorData);
                    list2.Add((eventBoxEditorData.id, item));
                }

                return (eventBoxes: list, baseLists: list2);
            }
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
                EaseType.BeatSaberInOutElastic => BeatmapSaveDataCommon
                    .EaseType
                    .BeatSaberInOutElastic,
                EaseType.BeatSaberInOutBounce => BeatmapSaveDataCommon
                    .EaseType
                    .BeatSaberInOutBounce,
                _ => BeatmapSaveDataCommon.EaseType.None,
            };
        }

        private static int ConvertBool(bool v) => v ? 1 : 0;

        private static V4.EventBoxGroupType ConvertEventBoxGroupTypeToV4(EventBoxGroupType type)
        {
            return type switch
            {
                EventBoxGroupType.Color => V4.EventBoxGroupType.Color,
                EventBoxGroupType.Rotation => V4.EventBoxGroupType.Rotation,
                EventBoxGroupType.Translation => V4.EventBoxGroupType.Translation,
                EventBoxGroupType.FloatFx => V4.EventBoxGroupType.FloatFx,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
        }

        private static EventBoxGroupType ConvertEventBoxGroupTypeFromV4(V4.EventBoxGroupType t)
        {
            return t switch
            {
                V4.EventBoxGroupType.Color => EventBoxGroupType.Color,
                V4.EventBoxGroupType.Rotation => EventBoxGroupType.Rotation,
                V4.EventBoxGroupType.Translation => EventBoxGroupType.Translation,
                V4.EventBoxGroupType.FloatFx => EventBoxGroupType.FloatFx,
                _ => throw new ArgumentOutOfRangeException(nameof(t), t, null),
            };
        }

        private static BeatmapEventDataBox.DistributionParamType ConvertDistributionParam(
            DistributionParamType distributionParamType
        )
        {
            return distributionParamType switch
            {
                DistributionParamType.Wave => BeatmapEventDataBox.DistributionParamType.Wave,
                DistributionParamType.Step => BeatmapEventDataBox.DistributionParamType.Step,
                _ => BeatmapEventDataBox.DistributionParamType.Wave,
            };
        }

        private static IndexFilterEditorData CreateIndexFilterV4(V4.IndexFilter f)
        {
            return IndexFilterEditorData.CreateNew(
                (IndexFilterEditorData.IndexFilterType)f.f,
                f.p,
                f.t,
                f.r == 1,
                f.c,
                (IndexFilter.IndexFilterRandomType)f.n,
                f.s,
                f.l,
                (IndexFilter.IndexFilterLimitAlsoAffectType)f.d
            );
        }

        private static V4.LightColorEvent ConvertLightColorBaseEvent(LightColorBaseEditorData e)
        {
            return new V4.LightColorEvent
            {
                p = ConvertBool(e.usePreviousValue),
                e = EaseTypeConvertor.Convert(
                    (easeLeadType: e.easeLeadType, easeCurveType: e.easeCurveType).ToEaseType()
                ),
                c = (BeatmapSaveDataCommon.EnvironmentColorType)e.colorType,
                b = e.brightness,
                f = e.strobeBeatFrequency,
                sb = e.strobeBrightness,
                sf = ConvertBool(e.strobeFade),
            };
        }

        private static V4.LightColorEventBox ConvertLightColorEventBox(
            LightColorEventBoxEditorData b
        )
        {
            return new V4.LightColorEventBox
            {
                w = b.beatDistributionParam,
                d = (DistributionParamType)b.beatDistributionParamType,
                s = b.brightnessDistributionParam,
                t = (DistributionParamType)b.brightnessDistributionParamType,
                b = ConvertBool(b.brightnessDistributionShouldAffectFirstBaseEvent),
                e = EaseTypeConvertor.Convert(b.brightnessDistributionEaseType),
            };
        }

        private static V4.LightRotationEvent ConvertLightRotationBaseEvent(
            LightRotationBaseEditorData e
        )
        {
            return new V4.LightRotationEvent
            {
                p = ConvertBool(e.usePreviousValue),
                e = EaseTypeConvertor.Convert(
                    (easeLeadType: e.easeLeadType, easeCurveType: e.easeCurveType).ToEaseType()
                ),
                l = e.loopsCount,
                r = e.rotation,
                d = (RotationDirection)e.rotationDirection,
            };
        }

        private static V4.LightRotationEventBox ConvertLightRotationEventBox(
            LightRotationEventBoxEditorData b
        )
        {
            return new V4.LightRotationEventBox
            {
                w = b.beatDistributionParam,
                d = (DistributionParamType)b.beatDistributionParamType,
                s = b.rotationDistributionParam,
                t = (DistributionParamType)b.rotationDistributionParamType,
                b = ConvertBool(b.rotationDistributionShouldAffectFirstBaseEvent),
                a = (Axis)b.axis,
                f = ConvertBool(b.flipRotation),
                e = EaseTypeConvertor.Convert(b.rotationDistributionEaseType),
            };
        }

        private static V4.LightTranslationEvent ConvertLightTranslationBaseEvent(
            LightTranslationBaseEditorData e
        )
        {
            return new V4.LightTranslationEvent
            {
                p = ConvertBool(e.usePreviousValue),
                e = EaseTypeConvertor.Convert(
                    (easeLeadType: e.easeLeadType, easeCurveType: e.easeCurveType).ToEaseType()
                ),
                t = e.translation,
            };
        }

        private static V4.LightTranslationEventBox ConvertLightTranslationEventBox(
            LightTranslationEventBoxEditorData b
        )
        {
            return new V4.LightTranslationEventBox
            {
                w = b.beatDistributionParam,
                d = (DistributionParamType)b.beatDistributionParamType,
                s = b.gapDistributionParam,
                t = (DistributionParamType)b.gapDistributionParamType,
                b = ConvertBool(b.gapDistributionShouldAffectFirstBaseEvent),
                a = (Axis)b.axis,
                f = ConvertBool(b.flipTranslation),
                e = EaseTypeConvertor.Convert(b.gapDistributionEaseType),
            };
        }

        private static V4.FloatFxEvent ConvertFloatFxBaseEvent(FloatFxBaseEditorData e)
        {
            return new V4.FloatFxEvent
            {
                p = ConvertBool(e.usePreviousValue),
                e = EaseTypeConvertor.Convert(
                    (easeLeadType: e.easeLeadType, easeCurveType: e.easeCurveType).ToEaseType()
                ),
                v = e.value,
            };
        }

        private static V4.FxEventBox ConvertFxEventBox(FxEventBoxEditorData b)
        {
            return new V4.FxEventBox
            {
                w = b.beatDistributionParam,
                d = (DistributionParamType)b.beatDistributionParamType,
                s = b.vfxDistributionParam,
                t = (DistributionParamType)b.vfxDistributionParamType,
                b = ConvertBool(b.vfxDistributionShouldAffectFirstBaseEvent),
                e = EaseTypeConvertor.Convert(b.vfxDistributionEaseType),
            };
        }

        private static EventBoxEditorData CreateLightColorEventBox(
            V4.LightColorEventBox b,
            IndexFilterEditorData indexFilter
        )
        {
            return LightColorEventBoxEditorData.CreateNew(
                indexFilter,
                ConvertDistributionParam(b.d),
                b.w,
                ConvertDistributionParam(b.t),
                b.s,
                b.b == 1,
                BeatmapTypeConverters.ConvertEaseType(b.e)
            );
        }

        private static EventBoxEditorData CreateLightRotationEventBox(
            V4.LightRotationEventBox b,
            IndexFilterEditorData indexFilter
        )
        {
            return LightRotationEventBoxEditorData.CreateNew(
                indexFilter,
                ConvertDistributionParam(b.d),
                b.w,
                ConvertDistributionParam(b.t),
                b.s,
                b.b == 1,
                BeatmapTypeConverters.ConvertEaseType(b.e),
                BeatmapTypeConverters.ConvertLightAxis(b.a),
                b.f == 1
            );
        }

        private static EventBoxEditorData CreateLightTranslationEventBox(
            V4.LightTranslationEventBox b,
            IndexFilterEditorData indexFilter
        )
        {
            return LightTranslationEventBoxEditorData.CreateNew(
                indexFilter,
                ConvertDistributionParam(b.d),
                b.w,
                ConvertDistributionParam(b.t),
                b.s,
                b.b == 1,
                BeatmapTypeConverters.ConvertEaseType(b.e),
                BeatmapTypeConverters.ConvertLightAxis(b.a),
                b.f == 1
            );
        }

        private static EventBoxEditorData CreateFxEventBox(
            V4.FxEventBox b,
            IndexFilterEditorData indexFilter
        )
        {
            return FxEventBoxEditorData.CreateNew(
                indexFilter,
                ConvertDistributionParam(b.d),
                b.w,
                ConvertDistributionParam(b.t),
                b.s,
                b.b == 1,
                BeatmapTypeConverters.ConvertEaseType(b.e)
            );
        }

        private static BaseEditorData CreateLightColorBaseData(float beat, V4.LightColorEvent e)
        {
            return LightColorBaseEditorData.CreateNew(
                beat,
                e.b,
                BeatmapDataModelsLoader.ConvertEaseTypeToLead(e.e),
                BeatmapDataModelsLoader.ConvertEaseTypeToCurve(e.e),
                usePreviousValue: e.p == 1,
                colorType: (EnvironmentColorType)e.c,
                strobeFrequency: e.f,
                strobeBrightness: e.sb,
                strobeFade: e.sf == 1
            );
        }

        private static BaseEditorData CreateLightRotationBaseData(
            float beat,
            V4.LightRotationEvent e
        )
        {
            return LightRotationBaseEditorData.CreateNew(
                beat,
                rotation: e.r,
                easeLeadType: BeatmapDataModelsLoader.ConvertEaseTypeToLead(e.e),
                easeCurveType: BeatmapDataModelsLoader.ConvertEaseTypeToCurve(e.e),
                loopsCount: e.l,
                usePreviousEventRotationValue: e.p == 1,
                rotationDirection: BeatmapTypeConverters.ConvertLightRotationDirection(e.d)
            );
        }

        private static BaseEditorData CreateLightTranslationBaseData(
            float beat,
            V4.LightTranslationEvent e
        )
        {
            return LightTranslationBaseEditorData.CreateNew(
                beat,
                translation: e.t,
                easeLeadType: BeatmapDataModelsLoader.ConvertEaseTypeToLead(e.e),
                easeCurveType: BeatmapDataModelsLoader.ConvertEaseTypeToCurve(e.e),
                usePreviousEventTranslationValue: e.p == 1
            );
        }

        private static BaseEditorData CreateFloatFxBaseData(float beat, V4.FloatFxEvent e)
        {
            return FloatFxBaseEditorData.CreateNew(
                beat,
                e.v,
                BeatmapDataModelsLoader.ConvertEaseTypeToLead(e.e),
                BeatmapDataModelsLoader.ConvertEaseTypeToCurve(e.e),
                e.p == 1
            );
        }
    }
}
