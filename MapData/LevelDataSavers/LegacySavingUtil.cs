using V3CustomSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;
using V3SaveData = BeatmapSaveDataVersion3.BeatmapSaveData;
using V3 = BeatmapSaveDataVersion3;
using V2 = BeatmapSaveDataVersion2_6_0AndEarlier;
using V2CustomSaveData = CustomJSONData.CustomBeatmap.Version2_6_0AndEarlierCustomBeatmapSaveData;
using BeatmapSaveDataCommon;
using BeatmapEditor3D;
using System.IO;
using System;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;

public static class LegacySavingUtil
{
    public static int SortByBeat(IBeat itemA, IBeat itemB)
    {
        return itemA.beat.CompareTo(itemB.beat);
    }

    public static int SortByEventTypeAndBeat(V3.BasicEventData itemA, V3.BasicEventData itemB)
    {
        int num = itemA.beat.CompareTo(itemB.beat);
        if (num != 0)
        {
            return num;
        }
        return itemA.eventType.CompareTo(itemB.eventType);
    }

    public static int SortByEventTypeAndBeat(V2.EventData itemA, V2.EventData itemB)
    {
        int num = itemA.beat.CompareTo(itemB.beat);
        if (num != 0)
        {
            return num;
        }
        return itemA.type.CompareTo(itemB.type);
    }

    public static int SortByRotationTypeAndBeat(V3.RotationEventData itemA, V3.RotationEventData itemB)
    {
        int num = itemA.beat.CompareTo(itemB.beat);
        if (num != 0)
        {
            return num;
        }
        return itemA.executionTime.CompareTo(itemB.executionTime);
    }

    public static void SerializeAndSave(string projectPath, string filename, object toSerialize)
    {
        string filePath = Path.Combine(projectPath, filename);
        BeatmapFileUtils.SaveToJSONFileCompact(toSerialize, filePath, false);
        Directory.SetLastWriteTime(projectPath, DateTime.Now);
    }
}