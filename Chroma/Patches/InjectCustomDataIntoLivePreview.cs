using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.DataModels.Events.Conversion;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.MapData.Contexts;
using HarmonyLib;
using SiraUtil.Affinity;
using System.Collections.Generic;
using System.Reflection;

namespace EditorEX.Chroma.Patches
{
    [AffinityPatch]
    public class InjectCustomDataIntoLivePreview : IAffinity
    {
        private static readonly MethodInfo _oldConvert = AccessTools.Method(typeof(BeatmapBasicEventConverter), "ConvertBasicEvent");
        private static readonly MethodInfo _newConvert = AccessTools.Method(typeof(InjectCustomDataIntoLivePreview), "ConvertBasicEvent");

        BeatmapEventData ConvertBasicEvent(BasicEventEditorData? e)
        {
            BeatmapEventData result;
            if (e.type == BasicBeatmapEventType.Event5)
            {
                result = new CustomColorBoostBeatmapEventData(e.beat, e.value == 1, CustomDataRepository.GetCustomData(e), MapContext.Version);
            }
            else
            {
                result = new CustomBasicBeatmapEventData(e.beat, e.type, e.value, e.floatValue, CustomDataRepository.GetCustomData(e), MapContext.Version);
            }
            CustomDataRepository.AddBasicEventConversion(e, result);
            return result;
        }

        [AffinityPatch(typeof(BeatmapBasicEventConverter), nameof(BeatmapBasicEventConverter.ConvertBasicEvent))]
        [AffinityPrefix]
        public bool Prefix(BasicEventEditorData? e, ref BeatmapEventData __result)
        {
            __result = ConvertBasicEvent(e);
            return false;
        }

        private static readonly Dictionary<BasicBeatmapEventType, CustomBasicBeatmapEventData> _defaultsForTypeCustom = new();

        [AffinityPatch(typeof(BasicBeatmapEventData), nameof(BasicBeatmapEventData.GetDefault))]
        [AffinityPrefix]
        public bool Prefix2(BasicBeatmapEventData __instance, ref BeatmapEventData __result)
        {
            if (__instance is CustomBasicBeatmapEventData customBasicBeatmapEventData)
            {
                CustomBasicBeatmapEventData basicBeatmapEventData;
                if (_defaultsForTypeCustom.TryGetValue(__instance.basicBeatmapEventType, out basicBeatmapEventData))
                {
                    __result = basicBeatmapEventData;
                    return false;
                }
                __result = _defaultsForTypeCustom[__instance.basicBeatmapEventType] = new CustomBasicBeatmapEventData(0f, __instance.basicBeatmapEventType, 0, 0f, new CustomData(), customBasicBeatmapEventData.version);
                return false;
            }
            return true;
        }
    }
}
