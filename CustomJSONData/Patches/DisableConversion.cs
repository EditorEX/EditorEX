using BeatmapEditor3D.DataModels;
using HarmonyLib;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
