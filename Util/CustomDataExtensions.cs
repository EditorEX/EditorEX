using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;

namespace EditorEX.Util
{
    public static class CustomDataExtensions
    {
        public static CustomData GetCustomData(this BaseEditorData? data)
        {
            return CustomDataRepository.GetCustomData(data);
        }
    }
}
