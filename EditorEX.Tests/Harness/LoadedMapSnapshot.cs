using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.MapData.Bookmarks;
using EditorEX.MapData.LevelDataLoaders;
using EditorEX.MapData.Objects;
using Newtonsoft.Json.Linq;

namespace EditorEX.Tests.Harness
{
    public sealed class LoadedMapSnapshot
    {
        public List<NoteRecord> Notes { get; } = new();

        public List<ObstacleRecord> Obstacles { get; } = new();

        public List<ArcRecord> Arcs { get; } = new();

        public List<ChainRecord> Chains { get; } = new();

        public List<WaypointRecord> Waypoints { get; } = new();

        public List<EventRecord> Events { get; } = new();

        public List<CustomEventRecord> CustomEvents { get; } = new();

        public List<BookmarkRecord> Bookmarks { get; } = new();

        public List<BpmRecord> BpmChanges { get; } = new();

        public List<EventBoxGroupRecord> EventBoxGroups { get; } = new();

        public List<KeywordRecord> Keywords { get; } = new();

        public bool UseNormalEventsAsCompatibleEvents { get; set; }

        public static LoadedMapSnapshot Capture(
            DifficultyLoadResult loaded,
            ICustomDataRepository repo
        )
        {
            var snapshot = new LoadedMapSnapshot
            {
                UseNormalEventsAsCompatibleEvents = loaded.UseNormalEventsAsCompatibleEvents,
            };

            foreach (NoteEditorData note in loaded.Notes)
            {
                snapshot.Notes.Add(
                    new NoteRecord
                    {
                        Beat = note.beat,
                        Column = note.column,
                        Row = note.row,
                        Rotation = note.rotation,
                        NoteType = note.noteType.ToString(),
                        ColorType = note.type.ToString(),
                        CutDirection = note.cutDirection.ToString(),
                        Angle = note.angle,
                        CustomData = CanonCustomData(repo.GetCustomData(note)),
                    }
                );
            }

            foreach (ObstacleEditorData obstacle in loaded.Obstacles)
            {
                snapshot.Obstacles.Add(
                    new ObstacleRecord
                    {
                        Beat = obstacle.beat,
                        Column = obstacle.column,
                        Row = obstacle.row,
                        Rotation = obstacle.rotation,
                        Duration = obstacle.duration,
                        Width = obstacle.width,
                        Height = obstacle.height,
                        CustomData = CanonCustomData(repo.GetCustomData(obstacle)),
                    }
                );
            }

            foreach (ArcEditorData arc in loaded.Arcs)
            {
                snapshot.Arcs.Add(
                    new ArcRecord
                    {
                        Beat = arc.beat,
                        Column = arc.column,
                        Row = arc.row,
                        Rotation = arc.rotation,
                        TailBeat = arc.tailBeat,
                        TailColumn = arc.tailColumn,
                        TailRow = arc.tailRow,
                        TailRotation = arc.tailRotation,
                        ColorType = arc.colorType.ToString(),
                        CutDirection = arc.cutDirection.ToString(),
                        TailCutDirection = arc.tailCutDirection.ToString(),
                        ControlPoint = arc.controlPointLengthMultiplier,
                        TailControlPoint = arc.tailControlPointLengthMultiplier,
                        MidAnchorMode = arc.midAnchorMode.ToString(),
                        CustomData = CanonCustomData(repo.GetCustomData(arc)),
                    }
                );
            }

            foreach (ChainEditorData chain in loaded.Chains)
            {
                snapshot.Chains.Add(
                    new ChainRecord
                    {
                        Beat = chain.beat,
                        Column = chain.column,
                        Row = chain.row,
                        Rotation = chain.rotation,
                        TailBeat = chain.tailBeat,
                        TailColumn = chain.tailColumn,
                        TailRow = chain.tailRow,
                        TailRotation = chain.tailRotation,
                        ColorType = chain.colorType.ToString(),
                        CutDirection = chain.cutDirection.ToString(),
                        SliceCount = chain.sliceCount,
                        SquishAmount = chain.squishAmount,
                        CustomData = CanonCustomData(repo.GetCustomData(chain)),
                    }
                );
            }

            foreach (WaypointEditorData waypoint in loaded.Waypoints)
            {
                snapshot.Waypoints.Add(
                    new WaypointRecord
                    {
                        Beat = waypoint.beat,
                        Column = waypoint.column,
                        Row = waypoint.row,
                        Rotation = waypoint.rotation,
                        OffsetDirection = waypoint.offsetDirection.ToString(),
                        CustomData = CanonCustomData(repo.GetCustomData(waypoint)),
                    }
                );
            }

            foreach (BasicEventEditorData evt in loaded.BasicEvents)
            {
                snapshot.Events.Add(
                    new EventRecord
                    {
                        Beat = evt.beat,
                        Type = evt.type.ToString(),
                        Value = evt.value,
                        FloatValue = evt.floatValue,
                        CustomData = CanonCustomData(repo.GetCustomData(evt)),
                    }
                );
            }

            foreach (CustomEventEditorData evt in repo.GetCustomEvents() ?? new())
            {
                snapshot.CustomEvents.Add(
                    new CustomEventRecord
                    {
                        Beat = evt.beat,
                        Type = evt.eventType,
                        Data = CanonCustomData(evt.customData),
                    }
                );
            }

            CustomData? beatmapCustom =
                repo.GetBeatmapData()?.customData ?? repo.GetCustomBeatmapSaveData()?.customData;
            foreach (CustomDataBookmark bookmark in CustomDataBookmarkCodec.Read(beatmapCustom, true))
            {
                snapshot.Bookmarks.Add(
                    new BookmarkRecord
                    {
                        Beat = bookmark.Beat,
                        Name = bookmark.Name ?? "",
                        R = bookmark.Color.r,
                        G = bookmark.Color.g,
                        B = bookmark.Color.b,
                        HasColor = bookmark.HasColor,
                    }
                );
            }

            foreach (BpmChangeEventData bpm in loaded.BpmChanges)
            {
                snapshot.BpmChanges.Add(new BpmRecord { Beat = bpm.beat, Bpm = bpm.bpm });
            }

            foreach (BeatmapEditorEventBoxGroupInput group in loaded.EventBoxGroups)
            {
                snapshot.EventBoxGroups.Add(CaptureEventBoxGroup(group));
            }

            foreach (BasicEventTypesForKeywordEditorData keyword in loaded.BasicEventTypesForKeyword)
            {
                snapshot.Keywords.Add(
                    new KeywordRecord
                    {
                        Keyword = keyword.keyword,
                        EventTypes = keyword
                            .eventTypes.Select(t => t.ToString())
                            .OrderBy(t => t, StringComparer.Ordinal)
                            .ToList(),
                    }
                );
            }

            snapshot.Sort();
            return snapshot;
        }

        public void Sort()
        {
            Notes.Sort((a, b) => CompareRecord(a.Beat, a.Key, b.Beat, b.Key));
            Obstacles.Sort((a, b) => CompareRecord(a.Beat, a.Key, b.Beat, b.Key));
            Arcs.Sort((a, b) => CompareRecord(a.Beat, a.Key, b.Beat, b.Key));
            Chains.Sort((a, b) => CompareRecord(a.Beat, a.Key, b.Beat, b.Key));
            Waypoints.Sort((a, b) => CompareRecord(a.Beat, a.Key, b.Beat, b.Key));
            Events.Sort((a, b) => CompareRecord(a.Beat, a.Key, b.Beat, b.Key));
            CustomEvents.Sort((a, b) => CompareRecord(a.Beat, a.Key, b.Beat, b.Key));
            Bookmarks.Sort((a, b) => CompareRecord(a.Beat, a.Key, b.Beat, b.Key));
            BpmChanges.Sort((a, b) => CompareRecord(a.Beat, a.Key, b.Beat, b.Key));
            EventBoxGroups.Sort((a, b) => CompareRecord(a.Beat, a.Key, b.Beat, b.Key));
            Keywords.Sort((a, b) => string.CompareOrdinal(a.Keyword, b.Keyword));
        }

        public static JToken CanonCustomData(CustomData? data)
        {
            if (data == null)
            {
                return new JObject();
            }

            CustomData filtered = CustomDataUtil.Filter(data);
            if (filtered == null || filtered.IsEmpty)
            {
                return new JObject();
            }

            var obj = new JObject();
            foreach (var kv in filtered.OrderBy(x => x.Key, StringComparer.Ordinal))
            {
                obj[kv.Key] = kv.Value == null ? JValue.CreateNull() : JToken.FromObject(kv.Value);
            }

            return obj;
        }

        private static EventBoxGroupRecord CaptureEventBoxGroup(
            BeatmapEditorEventBoxGroupInput input
        )
        {
            var bases = new Dictionary<BeatmapEditorObjectId, List<BaseEditorData>>();
            if (input.baseLists != null)
            {
                foreach (var (id, list) in input.baseLists)
                {
                    bases[id] = list;
                }
            }

            var boxes = new List<EventBoxRecord>();
            foreach (EventBoxEditorData box in input.eventBoxes ?? Array.Empty<EventBoxEditorData>())
            {
                IEnumerable<BaseEditorData> boxBases = bases.TryGetValue(box.id, out var list)
                    ? list
                    : Array.Empty<BaseEditorData>();
                boxes.Add(
                    new EventBoxRecord
                    {
                        Kind = box.GetType().Name,
                        Filter = DescribeFilter(box.indexFilter),
                        BeatDistribution = box.beatDistributionParam,
                        BeatDistributionType = box.beatDistributionParamType.ToString(),
                        Extra = DescribeBox(box),
                        BaseEvents = boxBases
                            .Select(DescribeBase)
                            .OrderBy(x => x, StringComparer.Ordinal)
                            .ToList(),
                    }
                );
            }

            boxes.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return new EventBoxGroupRecord
            {
                Beat = input.eventBoxGroup.beat,
                GroupId = input.eventBoxGroup.groupId,
                Type = input.eventBoxGroup.type.ToString(),
                Boxes = boxes,
            };
        }

        private static string DescribeFilter(IndexFilterEditorData? filter)
        {
            if (filter == null)
            {
                return "";
            }

            return string.Join(
                ",",
                filter.type,
                filter.param0,
                filter.param1,
                filter.reversed,
                filter.randomType,
                filter.seed,
                filter.chunks,
                filter.limit.ToString("G9", CultureInfo.InvariantCulture),
                filter.limitAlsoAffectType
            );
        }

        private static string DescribeBox(EventBoxEditorData box)
        {
            return box switch
            {
                LightColorEventBoxEditorData c => string.Join(
                    ",",
                    c.brightnessDistributionParam.ToString("G9", CultureInfo.InvariantCulture),
                    c.brightnessDistributionParamType,
                    c.brightnessDistributionShouldAffectFirstBaseEvent,
                    c.brightnessDistributionEaseType
                ),
                LightRotationEventBoxEditorData r => string.Join(
                    ",",
                    r.rotationDistributionParam.ToString("G9", CultureInfo.InvariantCulture),
                    r.rotationDistributionParamType,
                    r.rotationDistributionShouldAffectFirstBaseEvent,
                    r.rotationDistributionEaseType,
                    r.axis,
                    r.flipRotation
                ),
                LightTranslationEventBoxEditorData t => string.Join(
                    ",",
                    t.gapDistributionParam.ToString("G9", CultureInfo.InvariantCulture),
                    t.gapDistributionParamType,
                    t.gapDistributionShouldAffectFirstBaseEvent,
                    t.gapDistributionEaseType,
                    t.axis,
                    t.flipTranslation
                ),
                FxEventBoxEditorData f => string.Join(
                    ",",
                    f.vfxDistributionParam.ToString("G9", CultureInfo.InvariantCulture),
                    f.vfxDistributionParamType,
                    f.vfxDistributionShouldAffectFirstBaseEvent,
                    f.vfxDistributionEaseType
                ),
                _ => box.GetType().Name,
            };
        }

        private static string DescribeBase(BaseEditorData data)
        {
            return data switch
            {
                LightColorBaseEditorData c => string.Join(
                    "|",
                    c.beat.ToString("G9", CultureInfo.InvariantCulture),
                    c.brightness.ToString("G9", CultureInfo.InvariantCulture),
                    c.colorType,
                    c.strobeBeatFrequency,
                    c.strobeBrightness.ToString("G9", CultureInfo.InvariantCulture),
                    c.strobeFade
                ),
                LightRotationBaseEditorData r => string.Join(
                    "|",
                    r.beat.ToString("G9", CultureInfo.InvariantCulture),
                    r.rotation.ToString("G9", CultureInfo.InvariantCulture),
                    r.loopsCount,
                    r.usePreviousValue,
                    r.rotationDirection
                ),
                LightTranslationBaseEditorData t => string.Join(
                    "|",
                    t.beat.ToString("G9", CultureInfo.InvariantCulture),
                    t.translation.ToString("G9", CultureInfo.InvariantCulture),
                    t.usePreviousValue
                ),
                FloatFxBaseEditorData f => string.Join(
                    "|",
                    f.beat.ToString("G9", CultureInfo.InvariantCulture),
                    f.value.ToString("G9", CultureInfo.InvariantCulture),
                    f.usePreviousValue
                ),
                _ => data.beat.ToString("G9", CultureInfo.InvariantCulture) + "|" + data.GetType().Name,
            };
        }

        private static int CompareRecord(float beatA, string keyA, float beatB, string keyB)
        {
            int beat = beatA.CompareTo(beatB);
            return beat != 0 ? beat : string.CompareOrdinal(keyA, keyB);
        }

        public sealed class NoteRecord
        {
            public float Beat { get; set; }

            public int Column { get; set; }

            public int Row { get; set; }

            public int Rotation { get; set; }

            public string NoteType { get; set; } = "";

            public string ColorType { get; set; } = "";

            public string CutDirection { get; set; } = "";

            public int Angle { get; set; }

            public JToken CustomData { get; set; } = new JObject();

            public string Key =>
                string.Join(
                    "|",
                    Beat.ToString("0.###", CultureInfo.InvariantCulture),
                    Column,
                    Row,
                    Rotation,
                    NoteType,
                    ColorType,
                    CutDirection,
                    Angle
                );
        }

        public sealed class ObstacleRecord
        {
            public float Beat { get; set; }

            public int Column { get; set; }

            public int Row { get; set; }

            public int Rotation { get; set; }

            public float Duration { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }

            public JToken CustomData { get; set; } = new JObject();

            public string Key =>
                string.Join(
                    "|",
                    Beat.ToString("0.###", CultureInfo.InvariantCulture),
                    Column,
                    Row,
                    Rotation,
                    Duration.ToString("0.###", CultureInfo.InvariantCulture),
                    Width,
                    Height
                );
        }

        public sealed class ArcRecord
        {
            public float Beat { get; set; }

            public int Column { get; set; }

            public int Row { get; set; }

            public int Rotation { get; set; }

            public float TailBeat { get; set; }

            public int TailColumn { get; set; }

            public int TailRow { get; set; }

            public int TailRotation { get; set; }

            public string ColorType { get; set; } = "";

            public string CutDirection { get; set; } = "";

            public string TailCutDirection { get; set; } = "";

            public float ControlPoint { get; set; }

            public float TailControlPoint { get; set; }

            public string MidAnchorMode { get; set; } = "";

            public JToken CustomData { get; set; } = new JObject();

            public string Key =>
                string.Join(
                    "|",
                    Beat.ToString("0.###", CultureInfo.InvariantCulture),
                    TailBeat.ToString("0.###", CultureInfo.InvariantCulture),
                    Column,
                    Row,
                    TailColumn,
                    TailRow,
                    ColorType
                );
        }

        public sealed class ChainRecord
        {
            public float Beat { get; set; }

            public int Column { get; set; }

            public int Row { get; set; }

            public int Rotation { get; set; }

            public float TailBeat { get; set; }

            public int TailColumn { get; set; }

            public int TailRow { get; set; }

            public int TailRotation { get; set; }

            public string ColorType { get; set; } = "";

            public string CutDirection { get; set; } = "";

            public int SliceCount { get; set; }

            public float SquishAmount { get; set; }

            public JToken CustomData { get; set; } = new JObject();

            public string Key =>
                string.Join(
                    "|",
                    Beat.ToString("0.###", CultureInfo.InvariantCulture),
                    TailBeat.ToString("0.###", CultureInfo.InvariantCulture),
                    Column,
                    Row,
                    SliceCount
                );
        }

        public sealed class WaypointRecord
        {
            public float Beat { get; set; }

            public int Column { get; set; }

            public int Row { get; set; }

            public int Rotation { get; set; }

            public string OffsetDirection { get; set; } = "";

            public JToken CustomData { get; set; } = new JObject();

            public string Key =>
                string.Join(
                    "|",
                    Beat.ToString("0.###", CultureInfo.InvariantCulture),
                    Column,
                    Row,
                    OffsetDirection
                );
        }

        public sealed class EventRecord
        {
            public float Beat { get; set; }

            public string Type { get; set; } = "";

            public int Value { get; set; }

            public float FloatValue { get; set; }

            public JToken CustomData { get; set; } = new JObject();

            public string Key =>
                string.Join(
                    "|",
                    Beat.ToString("0.###", CultureInfo.InvariantCulture),
                    Type,
                    Value,
                    FloatValue.ToString("0.###", CultureInfo.InvariantCulture)
                );
        }

        public sealed class CustomEventRecord
        {
            public float Beat { get; set; }

            public string Type { get; set; } = "";

            public JToken Data { get; set; } = new JObject();

            public string Key =>
                string.Join("|", Beat.ToString("0.###", CultureInfo.InvariantCulture), Type);
        }

        public sealed class BookmarkRecord
        {
            public float Beat { get; set; }

            public string Name { get; set; } = "";

            public float R { get; set; }

            public float G { get; set; }

            public float B { get; set; }

            public bool HasColor { get; set; }

            public string Key =>
                string.Join("|", Beat.ToString("0.###", CultureInfo.InvariantCulture), Name);
        }

        public sealed class BpmRecord
        {
            public float Beat { get; set; }

            public float Bpm { get; set; }

            public string Key =>
                string.Join(
                    "|",
                    Beat.ToString("0.###", CultureInfo.InvariantCulture),
                    Bpm.ToString("0.###", CultureInfo.InvariantCulture)
                );
        }

        public sealed class EventBoxGroupRecord
        {
            public float Beat { get; set; }

            public int GroupId { get; set; }

            public string Type { get; set; } = "";

            public List<EventBoxRecord> Boxes { get; set; } = new();

            public string Key =>
                string.Join(
                    "|",
                    Beat.ToString("0.###", CultureInfo.InvariantCulture),
                    GroupId,
                    Type,
                    Boxes.Count
                );
        }

        public sealed class EventBoxRecord
        {
            public string Kind { get; set; } = "";

            public string Filter { get; set; } = "";

            public float BeatDistribution { get; set; }

            public string BeatDistributionType { get; set; } = "";

            public string Extra { get; set; } = "";

            public List<string> BaseEvents { get; set; } = new();

            public string Key => string.Join("|", Kind, Filter, Extra, BaseEvents.Count);
        }

        public sealed class KeywordRecord
        {
            public string Keyword { get; set; } = "";

            public List<string> EventTypes { get; set; } = new();
        }
    }
}
