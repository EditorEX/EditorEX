using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.DataModels.Events.Conversion;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.Patches;
using EditorEX.Essentials;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace EditorEX.Chroma.Patches
{
    [HarmonyPatch]
    public static class InjectCustomDataIntoLivePreview
    {
        //private static readonly MethodInfo _oldConvert = AccessTools.Method(typeof(BeatmapBasicEventConverter), "ConvertBasicEvent", null, null);
        //private static readonly MethodInfo _newConvert = AccessTools.Method(typeof(InjectCustomDataIntoLivePreview), "ConvertBasicEvent", null, null);

        //internal bool v2;

        //[Inject]
        //internal void Construct(VersionContext versionContext)
        //{
        //	Plugin.Log.Info($"HII {_oldConvert == null} {_newConvert == null}");
        //	v2 = versionContext.Version.Major < 3;
        //}

        static BeatmapEventData ConvertBasicEvent(BasicEventEditorData e, IBeatToTimeConverter timeConvertor)
        {
            BeatmapEventData result;
            if (e.type == BasicBeatmapEventType.Event5)
            {
                result = new CustomColorBoostBeatmapEventData(timeConvertor.ConvertBeatToTime(e.beat), e.value == 1, CustomDataRepository.GetCustomData(e), null);
            }
            else
            {
                result = new CustomBasicBeatmapEventData(timeConvertor.ConvertBeatToTime(e.beat), e.type, e.value, e.floatValue, CustomDataRepository.GetCustomData(e), null);
            }
            //Plugin.Log.Info("guh");
            CustomDataRepository.AddBasicEventConversion(e, result);
            return result;
        }

        [HarmonyPatch(typeof(BeatmapBasicEventConverter), nameof(BeatmapBasicEventConverter.ConvertBasicEvent))]
        [HarmonyPrefix]
        public static bool Prefix(BasicEventEditorData e, IBeatToTimeConverter timeConvertor, ref BeatmapEventData __result)
        {
            __result = ConvertBasicEvent(e, timeConvertor);
            return false;
        }
    }
}
