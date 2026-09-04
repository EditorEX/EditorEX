using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using BeatmapSaveDataCommon;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.CustomJSONData.VersionedSaveData;
using EditorEX.MapData.LevelDataSavers;
using EditorEX.MapData.Objects;
using V3 = BeatmapSaveDataVersion3;
using V4 = BeatmapSaveDataVersion4;

namespace EditorEX.MapData.LevelDataLoaders
{
    public class LevelDataLoaderV4 : ICustomLevelDataLoader
    {
        private readonly ICustomDataRepository _customDataRepository;

        private LevelDataLoaderV4(ICustomDataRepository customDataRepository)
        {
            _customDataRepository = customDataRepository;
        }

        public bool IsVersion(Version version)
        {
            return version.Major == 4;
        }

        public DifficultyLoadResult Load(
            BeatmapDataModelsLoader loader,
            string projectPath,
            string beatmapFilename,
            string? lightshowFilename
        )
        {
            _customDataRepository.ClearAll();

            var result = new DifficultyLoadResult();
            var saveData =
                LegacySavingUtil.Deserialize<CustomBeatmapSaveDataV4>(
                    File.ReadAllText(Path.Combine(projectPath, beatmapFilename))
                ) ?? new CustomBeatmapSaveDataV4();

            saveData.colorNotes ??= Array.Empty<CustomBeatmapBeatIndex>();
            saveData.bombNotes ??= Array.Empty<CustomBeatmapBeatIndex>();
            saveData.obstacles ??= Array.Empty<CustomBeatmapBeatIndex>();
            saveData.chains ??= Array.Empty<CustomChainBeatIndex>();
            saveData.arcs ??= Array.Empty<CustomArcBeatIndex>();
            saveData.njsEvents ??= Array.Empty<CustomBeatIndex>();
            saveData.colorNotesData ??= Array.Empty<V4.ColorNote>();
            saveData.bombNotesData ??= Array.Empty<V4.BombNote>();
            saveData.obstaclesData ??= Array.Empty<V4.Obstacle>();
            saveData.chainsData ??= Array.Empty<V4.Chain>();
            saveData.arcsData ??= Array.Empty<V4.Arc>();
            saveData.njsEventData ??= Array.Empty<V4.NoteJumpMovementSpeedEvent>();
            saveData.customData ??= new CustomData();

            foreach (var n in saveData.colorNotes)
            {
                var editor = ColorNoteCodec.LoadV4(n.b, n.r, saveData.colorNotesData[n.i]);
                _customDataRepository.AddCustomData(editor, n.customData ?? new CustomData());
                result.Notes.Add(editor);
            }

            foreach (var b in saveData.bombNotes)
            {
                var editor = BombNoteCodec.LoadV4(b.b, b.r, saveData.bombNotesData[b.i]);
                _customDataRepository.AddCustomData(editor, b.customData ?? new CustomData());
                result.Notes.Add(editor);
            }

            foreach (var o in saveData.obstacles)
            {
                var editor = ObstacleCodec.LoadV4(o.b, o.r, saveData.obstaclesData[o.i]);
                _customDataRepository.AddCustomData(editor, o.customData ?? new CustomData());
                result.Obstacles.Add(editor);
            }

            foreach (var c in saveData.chains)
            {
                var editor = ChainCodec.LoadV4(
                    c.hb,
                    c.hr,
                    saveData.colorNotesData[c.i],
                    c.tb,
                    c.tr,
                    saveData.chainsData[c.ci]
                );
                _customDataRepository.AddCustomData(editor, c.customData ?? new CustomData());
                result.Chains.Add(editor);
            }

            foreach (var a in saveData.arcs)
            {
                var editor = ArcCodec.LoadV4(
                    a.hb,
                    a.hr,
                    saveData.colorNotesData[a.hi],
                    a.tb,
                    a.tr,
                    saveData.colorNotesData[a.ti],
                    saveData.arcsData[a.ai]
                );
                _customDataRepository.AddCustomData(editor, a.customData ?? new CustomData());
                result.Arcs.Add(editor);
            }

            foreach (var e in saveData.njsEvents)
            {
                var editor = NjsEventCodec.LoadV4(e.b, saveData.njsEventData[e.i]);
                _customDataRepository.AddCustomData(editor, e.customData ?? new CustomData());
                result.NjsEvents.Add(editor);
            }

            var customEvents = CustomEventCodec.Read(saveData.customData, v3: true);
            foreach (CustomEventEditorData evt in customEvents)
            {
                _customDataRepository.AddCustomData(evt, evt.customData);
            }

            _customDataRepository.SetCustomEvents(customEvents);
            _customDataRepository.SetCustomBeatmapSaveData(
                CreateStubV3Snapshot(saveData.customData, customEvents)
            );

            if (
                string.IsNullOrEmpty(lightshowFilename)
                || !BeatmapProjectFileHelper.FileExists(projectPath, lightshowFilename)
            )
            {
                return result;
            }

            var lightshow =
                LegacySavingUtil.Deserialize<CustomLightshowSaveDataV4>(
                    File.ReadAllText(Path.Combine(projectPath, lightshowFilename))
                ) ?? new CustomLightshowSaveDataV4();

            lightshow.waypoints ??= Array.Empty<CustomBeatmapBeatIndex>();
            lightshow.waypointsData ??= Array.Empty<V4.Waypoint>();
            lightshow.basicEvents ??= Array.Empty<CustomBeatIndex>();
            lightshow.basicEventsData ??= Array.Empty<V4.BasicEvent>();
            lightshow.colorBoostEvents ??= Array.Empty<CustomBeatIndex>();
            lightshow.colorBoostEventsData ??= Array.Empty<V4.ColorBoostEvent>();
            lightshow.eventBoxGroups ??= Array.Empty<V4.EventBoxGroup>();
            lightshow.indexFilters ??= Array.Empty<V4.IndexFilter>();
            lightshow.lightColorEventBoxes ??= Array.Empty<V4.LightColorEventBox>();
            lightshow.lightColorEvents ??= Array.Empty<V4.LightColorEvent>();
            lightshow.lightRotationEventBoxes ??= Array.Empty<V4.LightRotationEventBox>();
            lightshow.lightRotationEvents ??= Array.Empty<V4.LightRotationEvent>();
            lightshow.lightTranslationEventBoxes ??= Array.Empty<V4.LightTranslationEventBox>();
            lightshow.lightTranslationEvents ??= Array.Empty<V4.LightTranslationEvent>();
            lightshow.fxEventBoxes ??= Array.Empty<V4.FxEventBox>();
            lightshow.floatFxEvents ??= Array.Empty<V4.FloatFxEvent>();
            lightshow.basicEventTypesWithKeywords ??= new BasicEventTypesWithKeywords(
                new List<BasicEventTypesWithKeywords.BasicEventTypesForKeyword>()
            );

            foreach (var w in lightshow.waypoints)
            {
                var editor = WaypointCodec.LoadV4(w.b, lightshow.waypointsData[w.i]);
                _customDataRepository.AddCustomData(editor, w.customData ?? new CustomData());
                result.Waypoints.Add(editor);
            }

            foreach (var e in lightshow.basicEvents)
            {
                var editor = BasicEventCodec.LoadV4(e.b, lightshow.basicEventsData[e.i]);
                _customDataRepository.AddCustomData(editor, e.customData ?? new CustomData());
                result.BasicEvents.Add(editor);
            }

            foreach (var e in lightshow.colorBoostEvents)
            {
                var editor = ColorBoostEventCodec.LoadV4(e.b, lightshow.colorBoostEventsData[e.i]);
                _customDataRepository.AddCustomData(editor, e.customData ?? new CustomData());
                result.BasicEvents.Add(editor);
            }

            result.BasicEventTypesForKeyword = lightshow
                .basicEventTypesWithKeywords.data.Select(d =>
                    BasicEventTypesForKeywordEditorData.CreateNew(
                        d.keyword,
                        d.eventTypes.Select(t => (BasicBeatmapEventType)t).ToList()
                    )
                )
                .ToList();
            result.UseNormalEventsAsCompatibleEvents = lightshow.useNormalEventsAsCompatibleEvents;
            result.EventBoxGroups = EventBoxGroupCodec.LoadV4(lightshow.ToVanilla());

            return result;
        }

        private static Version3CustomBeatmapSaveData CreateStubV3Snapshot(
            CustomData customData,
            List<CustomEventEditorData> customEvents
        )
        {
            var customEventSaveData = customEvents
                .Select(CustomEventCodec.SaveV3)
                .Cast<Version3CustomBeatmapSaveData.CustomEventSaveData>()
                .ToList();

            return new Version3CustomBeatmapSaveData(
                "4.1.0",
                new List<BpmChangeEventData>(),
                new List<RotationEventData>(),
                new List<ColorNoteData>(),
                new List<BombNoteData>(),
                new List<V3.ObstacleData>(),
                new List<V3.SliderData>(),
                new List<BurstSliderData>(),
                new List<V3.WaypointData>(),
                new List<BasicEventData>(),
                new List<ColorBoostEventData>(),
                new List<LightColorEventBoxGroup>(),
                new List<LightRotationEventBoxGroup>(),
                new List<LightTranslationEventBoxGroup>(),
                new List<FxEventBoxGroup>(),
                new FxEventsCollection(),
                new BasicEventTypesWithKeywords(
                    new List<BasicEventTypesWithKeywords.BasicEventTypesForKeyword>()
                ),
                false,
                customEventSaveData,
                customData
            );
        }
    }
}
