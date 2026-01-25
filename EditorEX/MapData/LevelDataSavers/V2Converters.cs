using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using BeatmapSaveDataCommon;
using EditorEX.Util;
using static EditorEX.CustomJSONData.VersionedSaveData.Custom2_6_0AndEarlierBeatmapSaveDataVersioned;
using V2 = BeatmapSaveDataVersion2_6_0AndEarlier;
using V2CustomSaveData = CustomJSONData.CustomBeatmap.Version2_6_0AndEarlierCustomBeatmapSaveData;

public static class V2Converters
{
    public static bool SaveCustom(BaseEditorData data)
    {
        var customData = data.GetCustomData();
        return (customData != null && !customData.IsEmpty);
    }

    public static V2.SliderData CreateSliderSaveData(ArcEditorData a)
    {
        return SaveCustom(a)
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
                a.GetCustomData()
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

    public static V2.NoteData CreateNoteSaveData(NoteEditorData n)
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

        return SaveCustom(n)
            ? new V2CustomSaveData.NoteSaveData(
                n.beat,
                n.column,
                (BeatmapSaveDataCommon.NoteLineLayer)n.row,
                noteType,
                (BeatmapSaveDataCommon.NoteCutDirection)n.cutDirection,
                n.GetCustomData()
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
        bool supportsFloatValue
    )
    {
        return SaveCustom(e)
            ? new V2CustomSaveData.EventSaveData(
                e.beat,
                (BeatmapEventType)e.type,
                e.value,
                e.floatValue,
                e.GetCustomData()
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

    public static CustomObstacleDataSerialized CreateObstacleSaveData(ObstacleEditorData o)
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
        return new CustomObstacleDataSerialized(
            new V2CustomSaveData.ObstacleSaveData(
                o.beat,
                o.column,
                type,
                o.duration,
                o.width,
                o.GetCustomData()
            )
        );
    }

    public static V2.WaypointData CreateWaypointSaveData(WaypointEditorData w)
    {
        return SaveCustom(w)
            ? new V2CustomSaveData.WaypointSaveData(
                w.beat,
                w.column,
                (BeatmapSaveDataCommon.NoteLineLayer)w.row,
                (BeatmapSaveDataCommon.OffsetDirection)w.offsetDirection,
                w.GetCustomData()
            )
            : new V2.WaypointData(
                w.beat,
                w.column,
                (BeatmapSaveDataCommon.NoteLineLayer)w.row,
                (BeatmapSaveDataCommon.OffsetDirection)w.offsetDirection
            );
    }
}
