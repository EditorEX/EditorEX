using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Util;
using System.Collections.Generic;

namespace EditorEX.CustomJSONData.Preview
{
    public class CustomBeatmapLivePreviewDataModel : BeatmapLivePreviewDataModel
    {
        public readonly Dictionary<BeatmapEditorObjectId, CustomEventData> _livePreviewCustomEvents = new Dictionary<BeatmapEditorObjectId, CustomEventData>();

        public void AddLivePreviewCustomEvent(CustomEventEditorData evt)
        {
            CustomEventData customEventData = new CustomEventData(evt.beat, evt.eventType, evt.customData, null);
            CustomDataRepository.AddCustomEventConversion(evt, customEventData);
            _livePreviewCustomEvents[evt.id] = customEventData;
            (_livePreviewBeatmapData as CustomBeatmapData).InsertCustomEventData(customEventData);
        }

        public void RemoveLivePreviewCustomEvent(CustomEventEditorData evt)
        {
            var beatmapData = _livePreviewBeatmapData as CustomBeatmapData;
            CustomDataRepository.RemoveCustomEventConversion(evt);
            beatmapData.RemoveBeatmapCustomEventData(_livePreviewCustomEvents[evt.id]);
            _livePreviewEvents.Remove(evt.id);
        }
    }
}
