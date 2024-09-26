using BeatmapEditor3D.DataModels;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.CustomJSONData.Patches
{
    [HarmonyPatch]
    public static class DisableConversion
    {
        [HarmonyPatch(typeof(BeatmapLevelToV4Convertor), nameof(BeatmapLevelToV4Convertor.DoesBeatmapLevelRequireConversionToV4))]
        [HarmonyPrefix]
        public static bool Patch(ref bool __result)
        {
            __result = false;
            Plugin.Log.Info("hi");
            return false;
        }
    }
}
