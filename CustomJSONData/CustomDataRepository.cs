using BeatmapEditor3D.DataModels;
using EditorEX.CustomJSONData.CustomEvents;
using CustomJSONData.CustomBeatmap;
using System.Collections.Generic;
using EditorEX.Util;

namespace EditorEX.CustomJSONData
{
    public class RepoData
    {
        public Dictionary<BaseEditorData?, CustomData> CustomData = new();

        public List<CustomEventEditorData> CustomEvents = new();

        public ReversibleDictionary<CustomEventEditorData, CustomEventData> CustomEventConversions = new();

        public ReversibleDictionary<BasicEventEditorData?, BeatmapEventData> ChromaBasicEventConversions = new();

        public Version3CustomBeatmapSaveData customBeatmapSaveData;

        public CustomBeatmapData customLivePreviewBeatmapData;

        public RepoData(RepoData original)
        {
            customLivePreviewBeatmapData = original.customLivePreviewBeatmapData;
        }

        public RepoData()
        {
        }
    }

    // This is a workaround to provide custom data as the EditorData clases are sealed...
    public static class CustomDataRepository
    {
        private static RepoData _repoData = new();

        public static void ClearAll()
        {
            _repoData = new RepoData(_repoData);
        }

        public static void AddCustomData(BaseEditorData? data, CustomData customData)
        {
            _repoData.CustomData[data] = customData;
        }

        public static CustomData GetCustomData(BaseEditorData? data)
        {
            _repoData.CustomData.TryGetValue(data, out var result);
            return result;
        }

        public static void AddCustomEventConversion(CustomEventEditorData editorData, CustomEventData customData)
        {
            _repoData.CustomEventConversions.Add(editorData, customData);
        }

        public static void RemoveCustomEventConversion(CustomEventEditorData editorData)
        {
            _repoData.CustomEventConversions.Remove(editorData);
        }

        public static void RemoveCustomEventConversion(CustomEventData customEventData)
        {
            _repoData.CustomEventConversions.Remove(customEventData);
        }

        public static CustomEventData GetCustomEventConversion(CustomEventEditorData editorData)
        {
            CustomEventData result = null;
            _repoData.CustomEventConversions.TryGetValue(editorData, out result);
            return result;
        }

        public static CustomEventEditorData GetCustomEventConversion(CustomEventData data)
        {
            CustomEventEditorData result = null;
            _repoData.CustomEventConversions.TryGetKey(data, out result);
            return result;
        }

        public static void AddBasicEventConversion(BasicEventEditorData? editorData, BeatmapEventData customData)
        {
            _repoData.ChromaBasicEventConversions.Add(editorData, customData);
        }

        public static void RemoveBasicEventConversion(BasicEventEditorData? editorData)
        {
            _repoData.ChromaBasicEventConversions.Remove(editorData);
        }

        public static void RemoveBasicEventConversion(BeatmapEventData data)
        {
            _repoData.ChromaBasicEventConversions.Remove(data);
        }

        public static BeatmapEventData GetBasicEventConversion(BasicEventEditorData? editorData)
        {
            BeatmapEventData result = null;
            _repoData.ChromaBasicEventConversions.TryGetValue(editorData, out result);
            return result;
        }

        public static BasicEventEditorData GetBasicEventConversion(BeatmapEventData data)
        {
            BasicEventEditorData result = null;
            _repoData.ChromaBasicEventConversions.TryGetKey(data, out result);
            return result;
        }

        public static void SetCustomBeatmapSaveData(Version3CustomBeatmapSaveData customData)
        {
            _repoData.customBeatmapSaveData = customData;
        }

        public static Version3CustomBeatmapSaveData GetCustomBeatmapSaveData()
        {
            return _repoData.customBeatmapSaveData;
        }

        public static void SetBeatmapData(CustomBeatmapData customData)
        {
            _repoData.customLivePreviewBeatmapData = customData;
        }

        public static CustomBeatmapData GetBeatmapData()
        {
            return _repoData.customLivePreviewBeatmapData;
        }

        public static void SetCustomEvents(List<CustomEventEditorData> events)
        {
            _repoData.CustomEvents = events;
        }

        public static List<CustomEventEditorData> GetCustomEvents()
        {
            return _repoData.CustomEvents;
        }
    }
}