using System.Linq;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Util;

namespace EditorEX.MapData.Objects
{
    public static class CustomDataUtil
    {
        public static bool SaveCustom(
            BaseEditorData data,
            ICustomDataRepository customDataRepository,
            out CustomData customData
        )
        {
            customData = data.GetCustomData(customDataRepository);
            if (customData != null)
            {
                customData = Filter(customData);
            }

            return customData != null && !customData.IsEmpty;
        }

        public static CustomData Filter(CustomData customData)
        {
            return new CustomData(
                customData
                    .Where(x => !x.Key.StartsWith("NE_"))
                    .ToDictionary(x => x.Key, x => x.Value)
            );
        }
    }
}
