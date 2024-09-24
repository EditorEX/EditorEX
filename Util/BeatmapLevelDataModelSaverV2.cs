using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using BeatmapEditor3D.Types;
using BeatmapSaveDataVersion2_6_0AndEarlier;
using BetterEditor.CustomJSONData.VersionedSaveData;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using System.Linq;
using static BetterEditor.CustomJSONData.VersionedSaveData.Custom2_6_0AndEarlierBeatmapSaveDataVersioned;

namespace BetterEditor.CustomJSONData.Util
{
    public class BeatmapLevelDataModelSaverV2
    {
        public BeatmapLevelDataModelSaver V3Saver { get; set; }

        public BeatmapLevelDataModelSaverV2(BeatmapLevelDataModelSaver v3Saver)
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
            return SaveCustom(a) ? new Custom2_6_0AndEarlierBeatmapSaveData.SliderData((a.colorType == ColorType.ColorA) ? BeatmapSaveData.ColorType.ColorA : BeatmapSaveData.ColorType.ColorB, a.beat, a.column, (NoteLineLayer)a.row, a.controlPointLengthMultiplier, a.cutDirection, a.tailBeat, a.tailColumn, (NoteLineLayer)a.tailRow, a.tailControlPointLengthMultiplier, a.tailCutDirection, a.midAnchorMode, a.GetCustomData())
                                 : new BeatmapSaveData.SliderData((a.colorType == ColorType.ColorA) ? BeatmapSaveData.ColorType.ColorA : BeatmapSaveData.ColorType.ColorB, a.beat, a.column, (NoteLineLayer)a.row, a.controlPointLengthMultiplier, a.cutDirection, a.tailBeat, a.tailColumn, (NoteLineLayer)a.tailRow, a.tailControlPointLengthMultiplier, a.tailCutDirection, a.midAnchorMode);
        }

        public BeatmapSaveData.NoteData CreateNoteSaveData(NoteEditorData n)
        {
            BeatmapSaveData.NoteType noteType;
            switch (n.noteType)
            {
                case NoteType.Note:
                    noteType = (n.type == ColorType.ColorA) ? BeatmapSaveData.NoteType.NoteA : BeatmapSaveData.NoteType.NoteB;
                    break;
                case NoteType.Bomb:
                    noteType = BeatmapSaveData.NoteType.Bomb;
                    break;
                default:
                    noteType = BeatmapSaveData.NoteType.None;
                    break;
            }

            return SaveCustom(n) ? new Custom2_6_0AndEarlierBeatmapSaveData.NoteData(n.beat, n.column, (NoteLineLayer)n.row, noteType, n.cutDirection, n.GetCustomData())
                                 : new BeatmapSaveData.NoteData(n.beat, n.column, (NoteLineLayer)n.row, noteType, n.cutDirection);
        }

        public BeatmapSaveData.EventData CreateBasicEventSaveData(BasicEventEditorData e, bool supportsFloatValue)
        {
            return SaveCustom(e) ? new Custom2_6_0AndEarlierBeatmapSaveData.EventData(e.beat, (BeatmapSaveData.BeatmapEventType)e.type, e.value, e.floatValue, e.GetCustomData())
                                 : new BeatmapSaveData.EventData(e.beat, (BeatmapSaveData.BeatmapEventType)e.type, e.value, supportsFloatValue ? e.floatValue : 0f);
        }

        public List<BeatmapSaveData.SpecialEventsForKeyword> CreateSpecialEventSaveData(BasicEventTypesForKeywordEditorData e, bool supportsFloatValue)
        {
            return e.eventTypes.Select(x => new BeatmapSaveData.SpecialEventsForKeyword(e.keyword, e.eventTypes.Select(y => (BeatmapSaveData.BeatmapEventType)y).ToList())).ToList();
        }

        public CustomObstacleDataSerialized CreateObstacleSaveData(ObstacleEditorData o)
        {
            BeatmapSaveData.ObstacleType type;
            switch (o.row)
            {
                case 2:
                    type = BeatmapSaveData.ObstacleType.Top;
                    break;
                default:
                    type = BeatmapSaveData.ObstacleType.FullHeight;
                    break;
            }
            return new CustomObstacleDataSerialized(new Custom2_6_0AndEarlierBeatmapSaveData.ObstacleData(o.beat, o.column, type, o.duration, o.width, o.GetCustomData()));
        }

        public BeatmapSaveData.WaypointData CreateWaypointSaveData(WaypointEditorData w)
        {
            return SaveCustom(w) ? new Custom2_6_0AndEarlierBeatmapSaveData.WaypointData(w.beat, w.column, (NoteLineLayer)w.row, w.offsetDirection, w.GetCustomData())
                                 : new BeatmapSaveData.WaypointData(w.beat, w.column, (NoteLineLayer)w.row, w.offsetDirection);
        }

        public Custom2_6_0AndEarlierBeatmapSaveDataVersioned Save(Version version)
        {
            bool supportFloatValue = version >= new Version(2, 5, 0);
            List<BeatmapSaveData.EventData> events = V3Saver._beatmapEventsDataModel.GetAllEventsAsList().Select(x => CreateBasicEventSaveData(x, supportFloatValue)).ToList();
            List<BeatmapSaveData.SpecialEventsForKeyword> specialEvents = V3Saver._beatmapEventsDataModel.GetBasicEventTypesForKeywordData().Select(x => CreateSpecialEventSaveData(x, supportFloatValue)).SelectMany(x => x).ToList();
            List<BeatmapSaveData.NoteData> list1 = new List<BeatmapSaveData.NoteData>();
            List<CustomObstacleDataSerialized> list2 = new List<CustomObstacleDataSerialized>();
            List<BeatmapSaveData.SliderData> list3 = new List<BeatmapSaveData.SliderData>();
            List<BeatmapSaveData.WaypointData> list4 = new List<BeatmapSaveData.WaypointData>();
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
                                    list3.Add(CreateSliderSaveData(arcEditorData));
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            list2.Add(CreateObstacleSaveData(obstacleEditorData));
                        }
                    }
                    else
                    {
                        list4.Add(CreateWaypointSaveData(waypointEditorData));
                    }
                }
                else
                {
                    list1.Add(CreateNoteSaveData(noteEditorData));
                }
            }
            events.Sort(new Comparison<BeatmapSaveData.EventData>(SortByEventTypeAndBeat));
            list1.Sort(new Comparison<BeatmapSaveData.NoteData>(SortByBeat));
            list2.Sort(new Comparison<CustomObstacleDataSerialized>(SortByBeatObstacles));
            list3.Sort(new Comparison<BeatmapSaveData.SliderData>(SortByBeat));
            list4.Sort(new Comparison<BeatmapSaveData.WaypointData>(SortByBeat));

            var customData = CustomDataRepository.GetCustomBeatmapSaveData().beatmapCustomData;

            customData["_customEvents"] = CustomDataRepository.GetCustomEvents().Select(x => new CustomEventDataSerialized(x));

            Plugin.Log.Info($"Saving {events.Count} events");

            return new Custom2_6_0AndEarlierBeatmapSaveDataVersioned(version.ToString(), events, list1, list3, list4, list2, new BeatmapSaveData.SpecialEventKeywordFiltersData(specialEvents), CustomDataRepository.GetCustomBeatmapSaveData().beatmapCustomData);
        }

        int SortByBeat(BeatmapSaveDataItem itemA, BeatmapSaveDataItem itemB)
        {
            return itemA.time.CompareTo(itemB.time);
        }

        int SortByBeatObstacles(CustomObstacleDataSerialized itemA, CustomObstacleDataSerialized itemB)
        {
            return itemA._time.CompareTo(itemB._time);
        }

        int SortByEventTypeAndBeat(BeatmapSaveData.EventData itemA, BeatmapSaveData.EventData itemB)
        {
            int num = itemA.time.CompareTo(itemB.time);
            if (num != 0)
            {
                return num;
            }
            return itemA.type.CompareTo(itemB.type);
        }
    }
}
