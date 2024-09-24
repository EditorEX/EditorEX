using BeatmapEditor3D.DataModels;
using EditorEX.CustomJSONData.Preview;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace EditorEX.CustomJSONData.Patches
{
    [HarmonyPatch(typeof(BeatmapLivePreviewDataModel), nameof(BeatmapLivePreviewDataModel.HandleBeatmapLevelDataModelLoaded))]
    public static class InjectCustomEvents
    {
        private static readonly MethodInfo _InjectEvents = AccessTools.Method(typeof(InjectCustomEvents), "InjectEvents", null, null);

        private static void InjectEvents(BeatmapLivePreviewDataModel self)
        {
            if (self is CustomBeatmapLivePreviewDataModel customBeatmapLivePreviewDataModel)
            {
                foreach (var evt in CustomDataRepository.GetCustomEvents())
                {
                    customBeatmapLivePreviewDataModel.AddLivePreviewCustomEvent(evt);
                }
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null).MatchForward(false, new CodeMatch[]
            {
                new CodeMatch(new OpCode?(OpCodes.Stloc_0), null, null),
            })
            .Advance(1)
            .Insert(new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, _InjectEvents)
            }).InstructionEnumeration();

            return result;
        }
    }
}
