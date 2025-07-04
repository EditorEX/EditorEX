﻿using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using SiraUtil.Affinity;

namespace EditorEX.CustomJSONData.Patches
{
    //A Much faster implementation of clearing the BeatmapData events that doesn't take 15 years.
    internal class BetterClearEvents : IAffinity
    {
        private static readonly MethodInfo _ClearEvents = AccessTools.Method(
            typeof(BetterClearEvents),
            "InjectEvents"
        );
        private static readonly FieldInfo _BeatmapDataItemsPerTypeAndId = AccessTools.Field(
            typeof(BeatmapData),
            "_beatmapDataItemsPerTypeAndId"
        );

        private static void InjectEvents(BeatmapLivePreviewDataModel self)
        {
            _BeatmapDataItemsPerTypeAndId.SetValue(
                self._livePreviewBeatmapData,
                new CustomBeatmapDataSortedListForTypeAndIds(
                    new BeatmapDataSortedListForTypeAndIds<BeatmapDataItem>()
                )
            );
            self._livePreviewBeatmapData._allBeatmapData.items.Clear();
            self._livePreviewBeatmapData._allBeatmapDataItemToNodeMap.Clear();
            (self._livePreviewBeatmapData as CustomBeatmapData)._beatmapEventDatas.Clear();
            (self._livePreviewBeatmapData as CustomBeatmapData)._customEventDatas.Clear();
        }

        [AffinityPatch(
            typeof(BeatmapLivePreviewDataModel),
            nameof(BeatmapLivePreviewDataModel.HandleBeatmapLevelWillClose)
        )]
        [AffinityTranspiler]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new CodeMatcher(instructions)
                .Advance(63)
                .RemoveInstructions(5)
                .End()
                .Insert(new(OpCodes.Ldarg_0), new(OpCodes.Call, _ClearEvents))
                .InstructionEnumeration();
            return result;
        }
    }
}
