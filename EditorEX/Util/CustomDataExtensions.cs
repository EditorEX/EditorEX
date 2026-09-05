using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;

namespace EditorEX.Util
{
    public static class CustomDataExtensions
    {
        public static CustomData GetCustomData(
            this BaseEditorData? data,
            ICustomDataRepository repo
        )
        {
            return repo.GetCustomData(data);
        }

        public static CustomData GetOrCreateCustomData(
            this BaseEditorData? data,
            ICustomDataRepository repo
        )
        {
            CustomData customData = repo.GetCustomData(data);
            if (customData == null)
            {
                customData = new CustomData();
                repo.AddCustomData(data, customData);
            }

            return customData;
        }
    }
}
