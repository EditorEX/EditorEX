using System;
using System.IO;
using System.Text;
using BeatmapEditor3D;
using BeatmapSaveDataCommon;
using BGLib.JsonExtension;
using Newtonsoft.Json;
using UnityEngine;
using V2 = BeatmapSaveDataVersion2_6_0AndEarlier;
using V3 = BeatmapSaveDataVersion3;

namespace EditorEX.MapData.LevelDataSavers
{
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

        private static JsonSerializerSettings _serializerSettings = new()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = JsonSettings.jsonConverters,
            ContractResolver = CustomDataContractResolver.Instance,
        };

        public static void SaveToJSONFileCompact(object obj, string filePath, bool compressed)
        {
            try
            {
                string s = JsonConvert.SerializeObject(obj, _serializerSettings);
                File.WriteAllBytes(filePath, Encoding.UTF8.GetBytes(s));
            }
            catch (Exception ex)
            {
                Debug.LogWarning((object)ex);
            }
        }

        public static void SerializeAndSave(string projectPath, string filename, object toSerialize)
        {
            string filePath = Path.Combine(projectPath, filename);
            SaveToJSONFileCompact(toSerialize, filePath, false);
            Directory.SetLastWriteTime(projectPath, DateTime.Now);
        }
    }
}
