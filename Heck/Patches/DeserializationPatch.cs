using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Heck.Deserialize;
using EditorEX.MapData.Contexts;
using EditorEX.Util;
using Heck.Animation;
using SiraUtil.Affinity;
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

        private static readonly FieldInfo _customBeatmapDataV2 = BackingFieldUtil.GetBackingField<CustomBeatmapData>("version");
        private static readonly FieldInfo _customBeatmapDataCustomData = BackingFieldUtil.GetBackingField<CustomBeatmapData>("customData");
        private static readonly FieldInfo _customBeatmapDataBeatmapCustomData = BackingFieldUtil.GetBackingField<CustomBeatmapData>("beatmapCustomData");

        private DeserializationPatch(EditorDeserializerManager editorDeserializerManager)
        {
            _editorDeserializerManager = editorDeserializerManager;
        }

        [AffinityPatch(typeof(BeatmapDataModelsLoader), nameof(BeatmapDataModelsLoader.LoadToDataModel))]
        [AffinityPostfix]
        private void LoadToDataModelPatch(BeatmapDataModelsLoader __instance, string projectPath, string beatmapFilename, string lightshowFilename)
        {
            var beatmapVersion = BeatmapProjectFileHelper.GetVersionedJSONVersion(projectPath, beatmapFilename);

            MapContext.Version = beatmapVersion;

            if (lightshowFilename != "") return;

            var standardLevelInfoSaveData = CustomLevelInfoSaveData.Deserialize(File.ReadAllText(Path.Combine(projectPath, "Info.dat")));
            var customBeatmapSaveData = CustomDataRepository.GetCustomBeatmapSaveData();
            var beatmapData = CustomDataRepository.GetBeatmapData();

            _customBeatmapDataV2?.SetValue(beatmapData, beatmapVersion);
            _customBeatmapDataCustomData?.SetValue(beatmapData, customBeatmapSaveData.customData);
            var beatmapCustomData = (standardLevelInfoSaveData.difficultyBeatmapSets.SelectMany(x => x.difficultyBeatmaps).FirstOrDefault(x => x.beatmapFilename == beatmapFilename) as CustomLevelInfoSaveData.DifficultyBeatmap).customData;
            _customBeatmapDataBeatmapCustomData?.SetValue(beatmapData, beatmapCustomData);

            bool v2 = beatmapVersion < __instance._version300;

            Dictionary<string, Track> beatmapTracks;
            HashSet<ValueTuple<object, EditorDeserializedData>> deserializedDatas;

            _editorDeserializerManager.DeserializeBeatmapData(v2, false, out beatmapTracks, out deserializedDatas);

            EditorDeserializedDataContainer.DeserializeDatas = deserializedDatas.ToDictionary(x => x.Item1, x => x.Item2);
            EditorDeserializedDataContainer.Tracks = beatmapTracks;
            EditorDeserializedDataContainer.Ready = true;
        }
    }
}
