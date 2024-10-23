using EditorEX.CustomDataModels;
using HarmonyLib;
using SiraUtil.Affinity;
using System.Collections.Generic;
using static BeatmapEditor3D.DataModels.BeatmapDataModelSignals;

namespace EditorEX.UI.Patches
{
    internal class SaveCustomDataPatch : IAffinity
    {
        private readonly LevelCustomDataModel _levelCustomDataModel;

        private SaveCustomDataPatch(
            LevelCustomDataModel levelCustomDataModel)
        {
            _levelCustomDataModel = levelCustomDataModel;
        }

        [AffinityPatch(typeof(UpdateBeatmapDataCommand), nameof(UpdateBeatmapDataCommand.Execute))]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions, null)
                .End()
                .Advance(-3)
                .RemoveInstructions(3)
                .InstructionEnumeration();
            return result;
        }

        [AffinityPatch(typeof(UpdateBeatmapDataCommand), nameof(UpdateBeatmapDataCommand.Execute))]
        [AffinityPostfix]
        private void Execute(UpdateBeatmapDataCommand __instance)
        {
            _levelCustomDataModel.UpdateWith(__instance._signal.levelAuthorName, __instance._signal.allDirectionsEnvironmentName, __instance._signal.environmentName);
        }
    }
}
