using BeatmapEditor3D.DataModels;
using BetterEditor.CustomJSONData.Preview;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Zenject;

namespace BetterEditor.CustomJSONData.Patches
{
    [HarmonyPatch(typeof(BeatmapEditorDataModelsInstaller), nameof(BeatmapEditorDataModelsInstaller.Install))]
    public static class PreviewCustomBeatmap
    {
        private static readonly ConstructorInfo _beatmapDataCtor = AccessTools.FirstConstructor(typeof(BeatmapData), (ConstructorInfo _) => true);
        private static readonly MethodInfo _createCustomBeatmapData = AccessTools.Method(typeof(PreviewCustomBeatmap), "CreateBeatmapData", null, null);

        private static readonly MethodInfo _bindLivePreviewDataModel = AccessTools.Method(typeof(DiContainer), "BindInterfacesAndSelfTo", new Type[] { }, new Type[] { typeof(BeatmapLivePreviewDataModel) });
        private static readonly MethodInfo _bindCustomLivePreviewModel = AccessTools.Method(typeof(PreviewCustomBeatmap), "BindCustomLivePreviewModel", null, null);

        private static CustomBeatmapData CreateBeatmapData(int numberOfLines)
        {
            var beatmapData = new CustomBeatmapData(numberOfLines, false, null, null, null);
            CustomDataRepository.SetCustomLivePreviewBeatmapData(beatmapData);
            return beatmapData;
        }

        private static void BindCustomLivePreviewModel(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<CustomBeatmapLivePreviewDataModel>().AsSingle();
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null).MatchForward(false, new CodeMatch[]
            {
                new CodeMatch(new OpCode?(OpCodes.Newobj), _beatmapDataCtor, null)
            })
            .Set(OpCodes.Call, _createCustomBeatmapData)
            .MatchForward(false, new CodeMatch[]
            {
                new CodeMatch(new OpCode?(OpCodes.Callvirt), _bindLivePreviewDataModel, null)
            })
            .Advance(-1).RemoveInstructions(4)
            .Insert(new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, _bindCustomLivePreviewModel)
            }).InstructionEnumeration();

            return result;
        }
    }
}
