using System;
using System.IO;
using BeatmapEditor3D;
using BeatmapSaveDataCommon;
using V2 = BeatmapSaveDataVersion2_6_0AndEarlier;
using V3 = BeatmapSaveDataVersion3;

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

    public static int SortByRotationTypeAndBeat(
        V3.RotationEventData itemA,
        V3.RotationEventData itemB
    )
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
