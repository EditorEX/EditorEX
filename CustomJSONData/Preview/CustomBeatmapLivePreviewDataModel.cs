using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BetterEditor.CustomJSONData.CustomEvents;
using BetterEditor.CustomJSONData.Util;
using CustomJSONData.CustomBeatmap;
using System.Collections.Generic;

namespace BetterEditor.CustomJSONData.Preview
{
    public class CustomBeatmapLivePreviewDataModel : BeatmapLivePreviewDataModel
    {
        private readonly Dictionary<BeatmapEditorObjectId, CustomEventData> _livePreviewCustomEvents = new Dictionary<BeatmapEditorObjectId, CustomEventData>();

        public void AddLivePreviewCustomEvent(CustomEventEditorData evt)
        {
            CustomEventData customEventData = new CustomEventData(_timeConvertor.ConvertBeatToTime(evt.beat), evt.eventType, evt.customData);
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
