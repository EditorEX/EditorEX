using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;

namespace EditorEX.CustomJSONData.CustomEvents
{
    public class CustomEventEditorData : BaseEditorData
    {
        public CustomEventEditorData(
            BeatmapEditorObjectId eventId,
            float time,
            string type,
            CustomData data,
            bool version260AndEarlier
        )
            : base(eventId, time)
        {
            eventType = type;
            customData = data;
            version2_6_0AndEarlier = version260AndEarlier;
        }

        public static CustomEventEditorData CreateNew(
            float time,
            string type,
            CustomData data,
            bool version260AndEarlier
        )
        {
            return new CustomEventEditorData(
                BeatmapEditorObjectId.NewId(),
                time,
                type,
                data,
                version260AndEarlier
            );
        }

        public string eventType { get; }

        public CustomData customData { get; }

        public bool version2_6_0AndEarlier { get; }
    }
}
