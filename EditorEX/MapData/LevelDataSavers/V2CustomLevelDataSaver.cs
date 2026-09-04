using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using BeatmapSaveDataVersion2_6_0AndEarlier;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.VersionedSaveData;
using EditorEX.MapData.Bookmarks;
using EditorEX.MapData.Contexts;
using static EditorEX.CustomJSONData.VersionedSaveData.Custom2_6_0AndEarlierBeatmapSaveDataVersioned;
using V2 = BeatmapSaveDataVersion2_6_0AndEarlier;

namespace EditorEX.MapData.LevelDataSavers
{
    public class V2CustomLevelDataSaver : ICustomLevelDataSaver
    {
        private readonly ICustomDataRepository _customDataRepository;

        private V2CustomLevelDataSaver(ICustomDataRepository customDataRepository)
        {
            _customDataRepository = customDataRepository;
        }

        public bool IsVersion(Version version)
        {
            return version.Major == 2;
        }

        private Custom2_6_0AndEarlierBeatmapSaveDataVersioned GetSaveData(
            BeatmapProjectManager projectManager,
            DifficultyBeatmapData difficultyBeatmapData
        )
        {
            var basicEventsModel = projectManager._beatmapBasicEventsDataModel;
            var levelDataModel = projectManager._beatmapObjectsDataModel;
            var eventBoxGroupsDataModel = projectManager._beatmapEventBoxGroupsDataModel;

            bool supportFloatValue = MapContext.Version >= new Version(2, 5, 0);
            List<V2.EventData> events = basicEventsModel
                .GetAllEventsAsList()
                .Select(x =>
                    V2Converters.CreateBasicEventSaveData(
                        x,
                        supportFloatValue,
                        _customDataRepository
                    )
                )
                .ToList();
            List<V2.SpecialEventsForKeyword> specialEvents = basicEventsModel
                .GetBasicEventTypesForKeywordData()
                .Select(x => V2Converters.CreateSpecialEventSaveData(x, supportFloatValue))
                .SelectMany(x => x)
                .ToList();
            List<V2.NoteData> notes = new List<V2.NoteData>();
            List<V2.ObstacleData> obstacles = new List<V2.ObstacleData>();
            List<V2.SliderData> sliders = new List<V2.SliderData>();
            List<V2.WaypointData> waypoints = new List<V2.WaypointData>();
            foreach (
                BaseBeatmapObjectEditorData allBeatmapObject in levelDataModel.allBeatmapObjects
            )
            {
                if (!(allBeatmapObject is NoteEditorData noteEditorData))
                {
                    if (!(allBeatmapObject is WaypointEditorData w))
                    {
                        if (!(allBeatmapObject is ObstacleEditorData o))
                        {
                            if (!(allBeatmapObject is ChainEditorData c))
                            {
                                if (allBeatmapObject is ArcEditorData a)
                                {
                                    sliders.Add(
                                        V2Converters.CreateSliderSaveData(a, _customDataRepository)
                                    );
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            obstacles.Add(
                                V2Converters.CreateObstacleSaveData(o, _customDataRepository)
                            );
                        }
                    }
                    else
                    {
                        waypoints.Add(
                            V2Converters.CreateWaypointSaveData(w, _customDataRepository)
                        );
                    }
                }
                else if (noteEditorData.noteType == BeatmapEditor3D.Types.NoteType.Note)
                {
                    notes.Add(
                        V2Converters.CreateNoteSaveData(noteEditorData, _customDataRepository)
                    );
                }
            }
            events.Sort(LegacySavingUtil.SortByEventTypeAndBeat);
            notes.Sort(LegacySavingUtil.SortByBeat);
            waypoints.Sort(LegacySavingUtil.SortByBeat);
            obstacles.Sort(LegacySavingUtil.SortByBeat);
            sliders.Sort(LegacySavingUtil.SortByBeat);

            var customData = _customDataRepository.GetBeatmapData().customData;

            /*customData["_customEvents"] = _customDataRepository
                .GetCustomEvents()
                .Select(x => new CustomEventDataSerialized(x));

            CustomDataBookmarkCodec.Write(
                customData,
                projectManager._bookmarksDataModel,
                v3: false
            );
            var liveCustomData = _customDataRepository.GetBeatmapData()?.customData;
            if (liveCustomData != null && !ReferenceEquals(liveCustomData, customData))
            {
                CustomDataBookmarkCodec.Write(
                    liveCustomData,
                    projectManager._bookmarksDataModel,
                    v3: false
                );
            }*/

            Plugin.Logger.Info($"Note count: {notes.Count}");
            Plugin.Logger.Info($"Slider count: {sliders.Count}");
            Plugin.Logger.Info($"Waypoint count: {waypoints.Count}");
            Plugin.Logger.Info($"Obstacle count: {obstacles.Count}");
            Plugin.Logger.Info($"Event count: {events.Count}");
            Plugin.Logger.Info($"Special event count: {specialEvents.Count}");
            Plugin.Logger.Info($"Custom data count: {customData.Count}");

            return new Custom2_6_0AndEarlierBeatmapSaveDataVersioned(
                MapContext.Version.ToString(),
                events,
                notes,
                sliders,
                waypoints,
                obstacles,
                new SpecialEventKeywordFiltersData(specialEvents),
                customData
            );
        }

        public void Save(
            BeatmapProjectManager projectManager,
            DifficultyBeatmapData difficultyBeatmapData,
            bool clearDirty
        )
        {
            bool bookmarksDirty = projectManager._bookmarkDataModelSaver.NeedsSaving();
            if (!projectManager._beatmapDataModelsSaver.NeedsSaving() && !bookmarksDirty)
            {
                return;
            }

            if (
                projectManager._beatmapDataModelsSaver.BeatmapNeedSaving()
                || projectManager._beatmapDataModelsSaver.LightshowNeedsSaving()
                || bookmarksDirty
            )
            {
                var beatmapSaveData = GetSaveData(projectManager, difficultyBeatmapData);
                LegacySavingUtil.SerializeAndSave(
                    projectManager._workingBeatmapProject,
                    difficultyBeatmapData.beatmapFilename,
                    beatmapSaveData
                );
                if (clearDirty)
                {
                    projectManager._beatmapObjectsDataModel.ClearDirty();
                    projectManager._beatmapBasicEventsDataModel.ClearDirty();
                    projectManager._beatmapEventBoxGroupsDataModel.ClearDirty();
                    projectManager._bookmarksDataModel.ClearDirty();
                }
            }
            if (clearDirty)
            {
                projectManager.BackupProject();
                projectManager.SaveTempProject();
            }
        }
    }
}
