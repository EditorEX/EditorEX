using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Heck.Codecs;
using EditorEX.MapData.Contexts;
using EditorEX.SDK.Util;
using Heck.Animation;
using SiraUtil.Affinity;
using SiraUtil.Logging;

namespace EditorEX.Heck.Patches
{
    internal class DeserializationPatch : IAffinity
    {
        private readonly CustomDataCodecRegistry _registry;
        private readonly SiraLog _siraLog;
        private readonly ICustomDataRepository _customDataRepository;
        private readonly BeatmapObjectsDataModel _objectsModel;
        private readonly BeatmapBasicEventsDataModel _eventsModel;
        private readonly Dictionary<string, Track> _tracks;

        private static readonly FieldInfo _customBeatmapDataV2 =
            BackingFieldUtil.GetBackingField<CustomBeatmapData>("version");
        private static readonly FieldInfo _customBeatmapDataCustomData =
            BackingFieldUtil.GetBackingField<CustomBeatmapData>("customData");
        private static readonly FieldInfo _customBeatmapDataBeatmapCustomData =
            BackingFieldUtil.GetBackingField<CustomBeatmapData>("beatmapCustomData");

        private DeserializationPatch(
            CustomDataCodecRegistry registry,
            SiraLog siraLog,
            ICustomDataRepository customDataRepository,
            BeatmapObjectsDataModel objectsModel,
            BeatmapBasicEventsDataModel eventsModel,
            Dictionary<string, Track> tracks
        )
        {
            _registry = registry;
            _siraLog = siraLog;
            _customDataRepository = customDataRepository;
            _objectsModel = objectsModel;
            _eventsModel = eventsModel;
            _tracks = tracks;
        }

        [AffinityPatch(
            typeof(BeatmapDataModelsLoader),
            nameof(BeatmapDataModelsLoader.LoadToDataModel)
        )]
        [AffinityPostfix]
        private void LoadToDataModelPatch(
            BeatmapDataModelsLoader __instance,
            string projectPath,
            string beatmapFilename,
            string lightshowFilename
        )
        {
            _siraLog.Info($"Loading beatmap data from {projectPath}");
            var beatmapVersion = BeatmapProjectFileHelper.GetVersionedJSONVersion(
                projectPath,
                beatmapFilename
            );

            MapContext.Version = beatmapVersion;

            if (beatmapVersion >= new Version(4, 0, 0))
            {
                _registry.Clear();
                return;
            }

            var standardLevelInfoSaveData = CustomLevelInfoSaveData.Deserialize(
                File.ReadAllText(Path.Combine(projectPath, "Info.dat"))
            );
            var customBeatmapSaveData = _customDataRepository.GetCustomBeatmapSaveData();
            var beatmapData = _customDataRepository.GetBeatmapData();

            _customBeatmapDataV2?.SetValue(beatmapData, beatmapVersion);
            _customBeatmapDataCustomData?.SetValue(beatmapData, customBeatmapSaveData.customData);
            var beatmapCustomData = (
                standardLevelInfoSaveData
                    .difficultyBeatmapSets.SelectMany(x => x.difficultyBeatmaps)
                    .FirstOrDefault(x => x.beatmapFilename == beatmapFilename)
                as CustomLevelInfoSaveData.DifficultyBeatmap
            )?.customData;
            _customBeatmapDataBeatmapCustomData?.SetValue(beatmapData, beatmapCustomData);

            var ctx = new CustomDataCodecContext
            {
                SourceVersion = beatmapVersion,
                TargetVersion = beatmapVersion,
                Tracks = _tracks,
                TrackBuilder = new TrackBuilder(),
                Repository = _customDataRepository,
            };
            _registry.LoadMap(_objectsModel, _eventsModel, ctx);
            _siraLog.Info("Loaded custom data codecs into the App preview cache.");
        }
    }
}
