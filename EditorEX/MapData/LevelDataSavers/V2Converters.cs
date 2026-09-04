using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using BeatmapSaveDataCommon;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Util;
using static EditorEX.CustomJSONData.VersionedSaveData.Custom2_6_0AndEarlierBeatmapSaveDataVersioned;
using V2 = BeatmapSaveDataVersion2_6_0AndEarlier;
using V2CustomSaveData = CustomJSONData.CustomBeatmap.Version2_6_0AndEarlierCustomBeatmapSaveData;

namespace EditorEX.MapData.LevelDataSavers
{
    public static class V2Converters
    {
        public static bool SaveCustom(
            BaseEditorData data,
            ICustomDataRepository customDataRepository,
            out CustomData customData
        )
        {
            customData = data.GetCustomData(customDataRepository);
            if (customData != null)
            {
                customData = new CustomData(
                    customData
                        .Where(x => !x.Key.StartsWith("NE_"))
                        .ToDictionary(x => x.Key, x => x.Value)
                );
            }
            return (customData != null && !customData.IsEmpty);
        }

        public static V2.SliderData CreateSliderSaveData(
            ArcEditorData a,
            ICustomDataRepository customDataRepository
        )
        {
            return SaveCustom(a, customDataRepository, out var customData)
                ? new V2CustomSaveData.SliderSaveData(
                    (a.colorType == ColorType.ColorA) ? V2.ColorType.ColorA : V2.ColorType.ColorB,
                    a.beat,
                    a.column,
                    (BeatmapSaveDataCommon.NoteLineLayer)a.row,
                    a.controlPointLengthMultiplier,
                    (BeatmapSaveDataCommon.NoteCutDirection)a.cutDirection,
                    a.tailBeat,
                    a.tailColumn,
                    (BeatmapSaveDataCommon.NoteLineLayer)a.tailRow,
                    a.tailControlPointLengthMultiplier,
                    (BeatmapSaveDataCommon.NoteCutDirection)a.tailCutDirection,
                    (BeatmapSaveDataCommon.SliderMidAnchorMode)a.midAnchorMode,
                    customData
                )
                : new V2.SliderData(
                    (a.colorType == ColorType.ColorA) ? V2.ColorType.ColorA : V2.ColorType.ColorB,
                    a.beat,
                    a.column,
                    (BeatmapSaveDataCommon.NoteLineLayer)a.row,
                    a.controlPointLengthMultiplier,
                    (BeatmapSaveDataCommon.NoteCutDirection)a.cutDirection,
                    a.tailBeat,
                    a.tailColumn,
                    (BeatmapSaveDataCommon.NoteLineLayer)a.tailRow,
                    a.tailControlPointLengthMultiplier,
                    (BeatmapSaveDataCommon.NoteCutDirection)a.tailCutDirection,
                    (BeatmapSaveDataCommon.SliderMidAnchorMode)a.midAnchorMode
                );
        }

        public static V2.NoteData CreateNoteSaveData(
            NoteEditorData n,
            ICustomDataRepository customDataRepository
        )
        {
            V2.NoteType noteType;
            switch (n.noteType)
            {
                case NoteType.Note:
                    noteType = (n.type == ColorType.ColorA) ? V2.NoteType.NoteA : V2.NoteType.NoteB;
                    break;
                case NoteType.Bomb:
                    noteType = V2.NoteType.Bomb;
                    break;
                default:
                    noteType = V2.NoteType.None;
                    break;
            }

            return SaveCustom(n, customDataRepository, out var customData)
                ? new V2CustomSaveData.NoteSaveData(
                    n.beat,
                    n.column,
                    (BeatmapSaveDataCommon.NoteLineLayer)n.row,
                    noteType,
                    (BeatmapSaveDataCommon.NoteCutDirection)n.cutDirection,
                    customData
                )
                : new V2.NoteData(
                    n.beat,
                    n.column,
                    (BeatmapSaveDataCommon.NoteLineLayer)n.row,
                    noteType,
                    (BeatmapSaveDataCommon.NoteCutDirection)n.cutDirection
                );
        }

        public static V2.EventData CreateBasicEventSaveData(
            BasicEventEditorData e,
            bool supportsFloatValue,
            ICustomDataRepository customDataRepository
        )
        {
            return SaveCustom(e, customDataRepository, out var customData)
                ? new V2CustomSaveData.EventSaveData(
                    e.beat,
                    (BeatmapEventType)e.type,
                    e.value,
                    e.floatValue,
                    customData
                )
                : new V2.EventData(
                    e.beat,
                    (BeatmapEventType)e.type,
                    e.value,
                    supportsFloatValue ? e.floatValue : 0f
                );
        }

        public static List<V2.SpecialEventsForKeyword> CreateSpecialEventSaveData(
            BasicEventTypesForKeywordEditorData e,
            bool supportsFloatValue
        )
        {
            return e
                .eventTypes.Select(x => new V2.SpecialEventsForKeyword(
                    e.keyword,
                    e.eventTypes.Select(y => (BeatmapEventType)y).ToList()
                ))
                .ToList();
        }

        public static V2.ObstacleData CreateObstacleSaveData(
            ObstacleEditorData o,
            ICustomDataRepository customDataRepository
        )
        {
            V2.ObstacleType type;
            switch (o.row)
            {
                case 2:
                    type = V2.ObstacleType.Top;
                    break;
                default:
                    type = V2.ObstacleType.FullHeight;
                    break;
            }
            return SaveCustom(o, customDataRepository, out var customData)
                ? new V2CustomSaveData.ObstacleSaveData(
                    o.beat,
                    o.column,
                    type,
                    o.duration,
                    o.width,
                    customData
                )
                : new V2.ObstacleData(o.beat, o.column, type, o.duration, o.width);
        }

        public static V2.WaypointData CreateWaypointSaveData(
            WaypointEditorData w,
            ICustomDataRepository customDataRepository
        )
        {
            return SaveCustom(w, customDataRepository, out var customData)
                ? new V2CustomSaveData.WaypointSaveData(
                    w.beat,
                    w.column,
                    (BeatmapSaveDataCommon.NoteLineLayer)w.row,
                    (BeatmapSaveDataCommon.OffsetDirection)w.offsetDirection,
                    customData
                )
                : new V2.WaypointData(
                    w.beat,
                    w.column,
                    (BeatmapSaveDataCommon.NoteLineLayer)w.row,
                    (BeatmapSaveDataCommon.OffsetDirection)w.offsetDirection
                );
        }
    }
}
