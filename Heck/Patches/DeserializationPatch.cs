using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Heck.Deserialize;
using EditorEX.MapData.Contexts;
using EditorEX.Util;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EditorEX.Heck.Patches
{
    internal class DeserializationPatch : IAffinity
    {
        private readonly EditorDeserializerManager _editorDeserializerManager;

        private readonly SiraLog _siraLog;

        private static readonly FieldInfo _customBeatmapDataV2 = BackingFieldUtil.GetBackingField<CustomBeatmapData>("version");
        private static readonly FieldInfo _customBeatmapDataCustomData = BackingFieldUtil.GetBackingField<CustomBeatmapData>("customData");
        private static readonly FieldInfo _customBeatmapDataBeatmapCustomData = BackingFieldUtil.GetBackingField<CustomBeatmapData>("beatmapCustomData");

        private DeserializationPatch(EditorDeserializerManager editorDeserializerManager, SiraLog siraLog)
        {
            _siraLog = siraLog;
            _editorDeserializerManager = editorDeserializerManager;
        }

        [AffinityPatch(typeof(BeatmapDataModelsLoader), nameof(BeatmapDataModelsLoader.LoadToDataModel))]
        [AffinityPostfix]
        private void LoadToDataModelPatch(BeatmapDataModelsLoader __instance, string projectPath, string beatmapFilename, string lightshowFilename)
        {
            _siraLog.Info($"Loading beatmap data from {projectPath}");
            var beatmapVersion = BeatmapProjectFileHelper.GetVersionedJSONVersion(projectPath, beatmapFilename);

            MapContext.Version = beatmapVersion;

            if (beatmapVersion >= new Version(4, 0, 0)) return;

            var standardLevelInfoSaveData = CustomLevelInfoSaveData.Deserialize(File.ReadAllText(Path.Combine(projectPath, "Info.dat")));
            var customBeatmapSaveData = CustomDataRepository.GetCustomBeatmapSaveData();
            var beatmapData = CustomDataRepository.GetBeatmapData();

            _customBeatmapDataV2?.SetValue(beatmapData, beatmapVersion);
            _customBeatmapDataCustomData?.SetValue(beatmapData, customBeatmapSaveData.customData);
            var beatmapCustomData = (standardLevelInfoSaveData.difficultyBeatmapSets.SelectMany(x => x.difficultyBeatmaps).FirstOrDefault(x => x.beatmapFilename == beatmapFilename) as CustomLevelInfoSaveData.DifficultyBeatmap)?.customData;
            _customBeatmapDataBeatmapCustomData?.SetValue(beatmapData, beatmapCustomData);

            var v2 = beatmapVersion < __instance._version300;

            _editorDeserializerManager.DeserializeBeatmapData(v2, false, out var beatmapTracks, out HashSet<ValueTuple<object, EditorDeserializedData>> deserializedDatas);

            EditorDeserializedDataContainer.DeserializeDatas = deserializedDatas.ToDictionary(x => x.Item1, x => x.Item2);
            _siraLog.Info($"Deserialized {EditorDeserializedDataContainer.DeserializeDatas.Count} custom data objects.");
            EditorDeserializedDataContainer.Tracks = beatmapTracks;
            EditorDeserializedDataContainer.Ready = true;
        }
    }
}
