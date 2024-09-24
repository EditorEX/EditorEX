using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.Util;
using EditorEX.Heck.Deserializer;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EditorEX.Heck.Patches
{
    internal class DeserializationPatch : IAffinity
    {
        private readonly EditorDeserializerManager _editorDeserializerManager;

        private static readonly FieldInfo _customBeatmapDataV2 = BackingFieldUtil.GetBackingField<CustomBeatmapData>("version2_6_0AndEarlier");
        private static readonly FieldInfo _customBeatmapDataCustomData = BackingFieldUtil.GetBackingField<CustomBeatmapData>("customData");
        private static readonly FieldInfo _customBeatmapDataBeatmapCustomData = BackingFieldUtil.GetBackingField<CustomBeatmapData>("beatmapCustomData");
        private static readonly FieldInfo _customBeatmapDataLevelCustomData = BackingFieldUtil.GetBackingField<CustomBeatmapData>("levelCustomData");

        internal static Version beatmapVersion;

        private DeserializationPatch(EditorDeserializerManager editorDeserializerManager)
        {
            _editorDeserializerManager = editorDeserializerManager;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapLevelDataModelVersionedLoader), nameof(BeatmapLevelDataModelVersionedLoader.LoadToDataModel))]
        private void LoadToDataModelPatch(BeatmapLevelDataModelVersionedLoader __instance, string projectPath, string filename)
        {
            var customBeatmapSaveData = CustomDataRepository.GetCustomBeatmapSaveData();
            var beatmapData = CustomDataRepository.GetCustomLivePreviewBeatmapData();
            Plugin.Log.Info("Loading custom data into beatmap data " + (beatmapData == null));

            _customBeatmapDataV2?.SetValue(beatmapData, customBeatmapSaveData.version2_6_0AndEarlier);
            _customBeatmapDataCustomData?.SetValue(beatmapData, customBeatmapSaveData.customData);
            _customBeatmapDataBeatmapCustomData?.SetValue(beatmapData, customBeatmapSaveData.beatmapCustomData);
            _customBeatmapDataLevelCustomData?.SetValue(beatmapData, customBeatmapSaveData.levelCustomData);

            beatmapVersion = BeatmapProjectFileHelper.GetVersionedJSONVersion(__instance._saveData, projectPath, filename);
            bool v2 = beatmapVersion < __instance._version300;
            Plugin.Log.Info($"V2? {v2}");

            Dictionary<string, Track> beatmapTracks;
            HashSet<ValueTuple<object, EditorDeserializedData>> deserializedDatas;

            _editorDeserializerManager.DeserializeBeatmapData(v2, false, out beatmapTracks, out deserializedDatas);

            EditorDeserializedDataContainer.DeserializeDatas = deserializedDatas.ToDictionary(x => x.Item1, x => x.Item2);
            EditorDeserializedDataContainer.Tracks = beatmapTracks;
            EditorDeserializedDataContainer.Ready = true;
        }
    }
}
