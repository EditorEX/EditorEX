using System;
using Newtonsoft.Json.Linq;

namespace EditorEX.UI.Patches
{
    public static class NewMapEmptyBeatmap
    {
        public static string Write(Version beatmapVersion)
        {
            if (beatmapVersion.Major == 2)
            {
                return new JObject
                {
                    ["_version"] = beatmapVersion.ToString(),
                    ["_events"] = new JArray(),
                    ["_notes"] = new JArray(),
                    ["_obstacles"] = new JArray(),
                    ["_waypoints"] = new JArray(),
                    ["_sliders"] = new JArray(),
                    ["_specialEventsKeywordFilters"] = new JObject { ["_keywords"] = new JArray() },
                    ["_customData"] = new JObject(),
                }.ToString();
            }

            if (beatmapVersion.Major == 3)
            {
                return new JObject
                {
                    ["version"] = beatmapVersion.ToString(),
                    ["bpmEvents"] = new JArray(),
                    ["rotationEvents"] = new JArray(),
                    ["colorNotes"] = new JArray(),
                    ["bombNotes"] = new JArray(),
                    ["obstacles"] = new JArray(),
                    ["sliders"] = new JArray(),
                    ["burstSliders"] = new JArray(),
                    ["waypoints"] = new JArray(),
                    ["basicBeatmapEvents"] = new JArray(),
                    ["colorBoostBeatmapEvents"] = new JArray(),
                    ["lightColorEventBoxGroups"] = new JArray(),
                    ["lightRotationEventBoxGroups"] = new JArray(),
                    ["lightTranslationEventBoxGroups"] = new JArray(),
                    ["vfxEventBoxGroups"] = new JArray(),
                    ["_fxEventsCollection"] = new JObject
                    {
                        ["_il"] = new JArray(),
                        ["_fl"] = new JArray(),
                    },
                    ["basicEventTypesWithKeywords"] = new JObject { ["d"] = new JArray() },
                    ["useNormalEventsAsCompatibleEvents"] = false,
                    ["customData"] = new JObject(),
                }.ToString();
            }

            throw new ArgumentOutOfRangeException(
                nameof(beatmapVersion),
                beatmapVersion,
                "Empty difficulty JSON is only generated for v2 and v3."
            );
        }
    }
}
