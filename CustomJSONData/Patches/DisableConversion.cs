using BeatmapEditor3D.DataModels;
using SiraUtil.Affinity;

namespace EditorEX.CustomJSONData.Patches
{
    [AffinityPatch]
    public class DisableConversion : IAffinity
    {
        [AffinityPatch(typeof(BeatmapLevelToV4Convertor), nameof(BeatmapLevelToV4Convertor.DoesBeatmapLevelRequireConversionToV4))]
        [AffinityPrefix]
        public bool Patch(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
