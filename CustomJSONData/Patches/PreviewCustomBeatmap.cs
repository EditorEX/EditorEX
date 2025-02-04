using BeatmapEditor3D.DataModels;
using EditorEX.CustomJSONData.Preview;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Zenject;
using SiraUtil.Affinity;

namespace EditorEX.CustomJSONData.Patches
{
    [AffinityPatch]
    public class PreviewCustomBeatmap : IAffinity
    {
        private readonly ConstructorInfo _beatmapDataCtor = AccessTools.FirstConstructor(typeof(BeatmapData), (ConstructorInfo _) => true);
        private readonly MethodInfo _createCustomBeatmapData = AccessTools.Method(typeof(PreviewCustomBeatmap), "CreateBeatmapData");

        private readonly MethodInfo _bindLivePreviewDataModel = AccessTools.Method(typeof(DiContainer), "BindInterfacesAndSelfTo", new Type[] { }, new Type[] { typeof(BeatmapLivePreviewDataModel) });
        private readonly MethodInfo _bindCustomLivePreviewModel = AccessTools.Method(typeof(PreviewCustomBeatmap), "BindCustomLivePreviewModel");

        [AffinityPatch(typeof(BeatmapEditorDataModelsInstaller), nameof(BeatmapEditorDataModelsInstaller.Install))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions).MatchForward(false, new CodeMatch[]
            {
                new(new OpCode?(OpCodes.Newobj), _beatmapDataCtor)
            })
            .Set(OpCodes.Call, _createCustomBeatmapData)
            .MatchForward(false, new CodeMatch[]
            {
                new(new OpCode?(OpCodes.Callvirt), _bindLivePreviewDataModel)
            })
            .Advance(-1).RemoveInstructions(4)
            .Insert(new(OpCodes.Ldarg_0), new(OpCodes.Call, _bindCustomLivePreviewModel)).InstructionEnumeration();
            return result;
        }

        private static CustomBeatmapData CreateBeatmapData(int numberOfLines)
        {
            var beatmapData = new CustomBeatmapData(numberOfLines, null, null, null, null);
            CustomDataRepository.SetBeatmapData(beatmapData);
            return beatmapData;
        }

        private static void BindCustomLivePreviewModel(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<CustomBeatmapLivePreviewDataModel>().AsSingle();
        }
    }
}
