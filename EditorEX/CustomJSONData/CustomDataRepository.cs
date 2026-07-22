using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.SDK.Util;

namespace EditorEX.CustomJSONData
{
    public class RepoData
    {
        public Dictionary<BaseEditorData?, CustomData> CustomData = new();

        public List<CustomEventEditorData> CustomEvents = new();

        public ReversibleDictionary<CustomEventEditorData, CustomEventData> CustomEventConversions =
            new();

        public ReversibleDictionary<
            BasicEventEditorData?,
            BeatmapEventData
        > ChromaBasicEventConversions = new();

        public Version3CustomBeatmapSaveData customBeatmapSaveData;

        public CustomBeatmapData customLivePreviewBeatmapData;

        public RepoData(RepoData original)
        {
            customLivePreviewBeatmapData = original.customLivePreviewBeatmapData;
        }

        public RepoData() { }
    }

    // This is a workaround to provide custom data as the EditorData clases are sealed...
    public class CustomDataRepository : ICustomDataRepository
    {
        private RepoData _repoData = new();

        public void ClearAll()
        {
            _repoData = new RepoData(_repoData);
        }

        public void AddCustomData(BaseEditorData? data, CustomData customData)
        {
            _repoData.CustomData[data] = customData;
        }

        public CustomData GetCustomData(BaseEditorData? data)
        {
            _repoData.CustomData.TryGetValue(data, out var result);
            return result;
        }

        public void AddCustomEventConversion(
            CustomEventEditorData editorData,
            CustomEventData customData
        )
        {
            _repoData.CustomEventConversions.Add(editorData, customData);
        }

        public void RemoveCustomEventConversion(CustomEventEditorData editorData)
        {
            _repoData.CustomEventConversions.Remove(editorData);
        }

        public void RemoveCustomEventConversion(CustomEventData customEventData)
        {
            _repoData.CustomEventConversions.Remove(customEventData);
        }

        public CustomEventData GetCustomEventConversion(CustomEventEditorData editorData)
        {
            CustomEventData result = null;
            _repoData.CustomEventConversions.TryGetValue(editorData, out result);
            return result;
        }

        public CustomEventEditorData GetCustomEventConversion(CustomEventData data)
        {
            CustomEventEditorData result = null;
            _repoData.CustomEventConversions.TryGetKey(data, out result);
            return result;
        }

        public void AddBasicEventConversion(
            BasicEventEditorData? editorData,
            BeatmapEventData customData
        )
        {
            _repoData.ChromaBasicEventConversions.Add(editorData, customData);
        }

        public void RemoveBasicEventConversion(BasicEventEditorData? editorData)
        {
            _repoData.ChromaBasicEventConversions.Remove(editorData);
        }

        public void RemoveBasicEventConversion(BeatmapEventData data)
        {
            _repoData.ChromaBasicEventConversions.Remove(data);
        }

        public BeatmapEventData GetBasicEventConversion(BasicEventEditorData? editorData)
        {
            BeatmapEventData result = null;
            _repoData.ChromaBasicEventConversions.TryGetValue(editorData, out result);
            return result;
        }

        public BasicEventEditorData GetBasicEventConversion(BeatmapEventData data)
        {
            BasicEventEditorData result = null;
            _repoData.ChromaBasicEventConversions.TryGetKey(data, out result);
            return result;
        }

        public void SetCustomBeatmapSaveData(Version3CustomBeatmapSaveData customData)
        {
            _repoData.customBeatmapSaveData = customData;
        }

        public Version3CustomBeatmapSaveData GetCustomBeatmapSaveData()
        {
            return _repoData.customBeatmapSaveData;
        }

        public void SetBeatmapData(CustomBeatmapData customData)
        {
            _repoData.customLivePreviewBeatmapData = customData;
        }

        public CustomBeatmapData GetBeatmapData()
        {
            if (_repoData.customLivePreviewBeatmapData == null)
            {
                var pending = LivePreviewBeatmapDataBuffer.Consume();
                if (pending != null)
                {
                    _repoData.customLivePreviewBeatmapData = pending;
                }
            }

            return _repoData.customLivePreviewBeatmapData;
        }

        public void SetCustomEvents(List<CustomEventEditorData> events)
        {
            _repoData.CustomEvents = events;
        }

        public List<CustomEventEditorData> GetCustomEvents()
        {
            return _repoData.CustomEvents;
        }
    }
}
