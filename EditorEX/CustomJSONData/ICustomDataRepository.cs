using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData.CustomEvents;

namespace EditorEX.CustomJSONData
{
    public interface ICustomDataRepository
    {
        void ClearAll();

        void AddCustomData(BaseEditorData? data, CustomData customData);

        CustomData GetCustomData(BaseEditorData? data);

        void AddCustomEventConversion(CustomEventEditorData editorData, CustomEventData customData);

        void RemoveCustomEventConversion(CustomEventEditorData editorData);

        void RemoveCustomEventConversion(CustomEventData customEventData);

        CustomEventData GetCustomEventConversion(CustomEventEditorData editorData);

        CustomEventEditorData GetCustomEventConversion(CustomEventData data);

        void AddBasicEventConversion(BasicEventEditorData? editorData, BeatmapEventData customData);

        void RemoveBasicEventConversion(BasicEventEditorData? editorData);

        void RemoveBasicEventConversion(BeatmapEventData data);

        BeatmapEventData GetBasicEventConversion(BasicEventEditorData? editorData);

        BasicEventEditorData GetBasicEventConversion(BeatmapEventData data);

        void SetCustomBeatmapSaveData(Version3CustomBeatmapSaveData customData);

        Version3CustomBeatmapSaveData GetCustomBeatmapSaveData();

        void SetBeatmapData(CustomBeatmapData customData);

        CustomBeatmapData GetBeatmapData();

        void SetCustomEvents(List<CustomEventEditorData> events);

        List<CustomEventEditorData> GetCustomEvents();
    }
}
