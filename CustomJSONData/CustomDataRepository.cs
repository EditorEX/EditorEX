using BeatmapEditor3D.DataModels;
using BetterEditor.CustomJSONData.CustomEvents;
using CustomJSONData.CustomBeatmap;
using ModestTree.Util;
using System.Collections.Generic;
using System.Linq;

namespace BetterEditor.CustomJSONData
{
    public class RepoData
    {
        public Dictionary<BaseEditorData, CustomData> CustomData = new Dictionary<BaseEditorData, CustomData>();

        public List<CustomEventEditorData> CustomEvents = new List<CustomEventEditorData>();

        public HashSet<ValuePair<CustomEventEditorData, CustomEventData>> CustomEventConversions = new HashSet<ValuePair<CustomEventEditorData, CustomEventData>>();

        public HashSet<ValuePair<BasicEventEditorData, BeatmapEventData>> ChromaBasicEventConversions = new HashSet<ValuePair<BasicEventEditorData, BeatmapEventData>>();

        public CustomBeatmapSaveData customBeatmapSaveData;

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
        private static RepoData _repoData = new RepoData();

        public static void ClearAll()
        {
            _repoData = new RepoData(_repoData);
        }

        public static void AddCustomData(BaseEditorData data, CustomData customData)
        {
            _repoData.CustomData[data] = customData;
        }

        public static CustomData GetCustomData(BaseEditorData data)
        {
            if (_repoData.CustomData.ContainsKey(data))
            {
                return _repoData.CustomData[data];
            }
            else
            {
                return null;
            }
        }

        public static void AddCustomEventConversion(CustomEventEditorData editorData, CustomEventData customData)
        {
            var existing = _repoData.CustomEventConversions.FirstOrDefault(x => x.First == editorData);
            if (existing != null)
            {
                _repoData.CustomEventConversions.Remove(existing);
            }
            _repoData.CustomEventConversions.Add(new ValuePair<CustomEventEditorData, CustomEventData>(editorData, customData));
        }

        public static void RemoveCustomEventConversion(CustomEventEditorData editorData)
        {
            var existing = _repoData.CustomEventConversions.FirstOrDefault(x => x.First == editorData);
            if (existing != null)
            {
                _repoData.CustomEventConversions.Remove(existing);
            }
        }

        public static CustomEventData GetCustomEventConversion(CustomEventEditorData editorData)
        {
            var existing = _repoData.CustomEventConversions.FirstOrDefault(x => x.First == editorData);
            if (existing != null)
            {
                return existing.Second;
            }
            else
            {
                return null;
            }
        }

        public static CustomEventEditorData GetCustomEventConversion(CustomEventData data)
        {
            var existing = _repoData.CustomEventConversions.FirstOrDefault(x => x.Second == data);
            if (existing != null)
            {
                return existing.First;
            }
            else
            {
                return null;
            }
        }

        public static void AddBasicEventConversion(BasicEventEditorData editorData, BeatmapEventData customData)
        {
            var existing = _repoData.ChromaBasicEventConversions.FirstOrDefault(x => x.First == editorData);
            if (existing != null)
            {
                _repoData.ChromaBasicEventConversions.Remove(existing);
            }
            _repoData.ChromaBasicEventConversions.Add(new ValuePair<BasicEventEditorData, BeatmapEventData>(editorData, customData));
        }

        public static void RemoveBasicEventConversion(BasicEventEditorData editorData)
        {
            var existing = _repoData.ChromaBasicEventConversions.FirstOrDefault(x => x.First == editorData);
            if (existing != null)
            {
                _repoData.ChromaBasicEventConversions.Remove(existing);
            }
        }

        public static BeatmapEventData GetBasicEventConversion(BasicEventEditorData editorData)
        {
            var existing = _repoData.ChromaBasicEventConversions.FirstOrDefault(x => x.First == editorData);
            if (existing != null)
            {
                return existing.Second;
            }
            else
            {
                return null;
            }
        }

        public static BasicEventEditorData GetBasicEventConversion(BeatmapEventData data)
        {
            var existing = _repoData.ChromaBasicEventConversions.FirstOrDefault(x => x.Second == data);
            return existing?.First;
        }

        public static void SetCustomBeatmapSaveData(CustomBeatmapSaveData customData)
        {
            _repoData.customBeatmapSaveData = customData;
        }

        public static CustomBeatmapSaveData GetCustomBeatmapSaveData()
        {
            return _repoData.customBeatmapSaveData;
        }

        public static void SetCustomLivePreviewBeatmapData(CustomBeatmapData customData)
        {
            _repoData.customLivePreviewBeatmapData = customData;
        }

        public static CustomBeatmapData GetCustomLivePreviewBeatmapData()
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