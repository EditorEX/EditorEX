﻿using EditorEX.CustomDataModels;
using HarmonyLib;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static BeatmapEditor3D.DataModels.BeatmapDataModelSignals;

namespace EditorEX.UI.Patches
{
    internal class SaveLevelAuthorNamePatch : IAffinity
    {
        private readonly LevelCustomDataModel _levelCustomDataModel;

        private SaveLevelAuthorNamePatch(
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
            _levelCustomDataModel.UpdateWith(__instance._signal.levelAuthorName);
        }
    }
}