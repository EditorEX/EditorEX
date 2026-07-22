using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using BeatmapSaveDataVersion2_6_0AndEarlier;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.VersionedSaveData;
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
            List<CustomObstacleDataSerialized> obstacles = new List<CustomObstacleDataSerialized>();
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

            var customData = _customDataRepository.GetBeatmapData().beatmapCustomData;

            customData["_customEvents"] = _customDataRepository
                .GetCustomEvents()
                .Select(x => new CustomEventDataSerialized(x));

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
            if (!projectManager._beatmapDataModelsSaver.NeedsSaving())
            {
                return;
            }

            if (
                projectManager._beatmapDataModelsSaver.BeatmapNeedSaving()
                || projectManager._beatmapDataModelsSaver.LightshowNeedsSaving()
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
