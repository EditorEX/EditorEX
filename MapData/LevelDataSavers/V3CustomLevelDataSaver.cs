using EditorEX.MapData.SaveDataLoaders;
using System;
using BeatmapEditor3D.DataModels;

using V3CustomSaveData = CustomJSONData.CustomBeatmap.Version3CustomBeatmapSaveData;
using V3SaveData = BeatmapSaveDataVersion3.BeatmapSaveData;
using V3 = BeatmapSaveDataVersion3;
using System.Collections.Generic;
using System.Linq;
using BeatmapSaveDataVersion3;
using BeatmapSaveDataCommon;
using EditorEX.CustomJSONData;
using BeatmapEditor3D.Types;
using EditorEX.MapData.Contexts;
using EditorEX.CustomJSONData.VersionedSaveData;
using static EditorEX.CustomJSONData.VersionedSaveData.Custom2_6_0AndEarlierBeatmapSaveDataVersioned;
using Zenject;
using SiraUtil.Logging;

namespace EditorEX.MapData.LevelDataSavers
{
    public class V3CustomLevelDataSaver : ICustomLevelDataSaver
    {
        [Inject]
        private SiraLog? _siraLog;
        public bool IsVersion(Version version)
        {
            return version.Major == 3;
        }

        private CustomBeatmapSaveDataVersioned GetSaveData(BeatmapProjectManager projectManager, DifficultyBeatmapData difficultyBeatmapData)
        {
            var basicEventsModel = projectManager._beatmapBasicEventsDataModel;
            var levelDataModel = projectManager._beatmapObjectsDataModel;
            var eventBoxGroupsDataModel = projectManager._beatmapEventBoxGroupsDataModel;

            var bpmChanges = projectManager._audioDataModel.bpmData.regions.Select(V3Converters.CreateBpmChangeEventSaveData).ToList();
            var rotationEvents = new List<RotationEventData>().Concat(basicEventsModel.GetAllDataIn(BasicBeatmapEventType.Event14).Select(V3Converters.CreateRotationEventSaveData)).Concat(basicEventsModel.GetAllDataIn(BasicBeatmapEventType.Event15).Select(V3Converters.CreateRotationEventSaveData)).ToList();
            var basicEvents = basicEventsModel.GetAllEventsAsList().Select(V3Converters.CreateBasicEventSaveData).ToList();
            var customEvents = CustomDataRepository.GetCustomEvents().Select(V3Converters.CreateCustomEventSaveData).ToList();
            var colorBoostEvents = basicEventsModel.GetAllDataIn(BasicBeatmapEventType.Event5).Select(V3Converters.CreateColorBoostSaveEventData).ToList();
            var colorNotes = new List<ColorNoteData>();
            var bombNotes = new List<BombNoteData>();
            var obstacles = new List<V3.ObstacleData>();
            var sliders = new List<V3.SliderData>();
            var burstSliders = new List<BurstSliderData>();
            var waypoints = new List<V3.WaypointData>();
            foreach (BaseBeatmapObjectEditorData allBeatmapObject in levelDataModel.allBeatmapObjects)
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
                                    sliders.Add(V3Converters.CreateSliderSaveData(a));
                                }
                            }
                            else
                            {
                                colorNotes.Add(V3Converters.CreateColorNoteSaveDataFromChains(c));
                                burstSliders.Add(V3Converters.CreateBurstSliderSaveData(c));
                            }
                        }
                        else
                        {
                            obstacles.Add(V3Converters.CreateObstacleSaveData(o));
                        }
                    }
                    else
                    {
                        waypoints.Add(V3Converters.CreateWaypointSaveData(w));
                    }
                }
                else if (noteEditorData.noteType == NoteType.Note)
                {
                    colorNotes.Add(V3Converters.CreateColorNoteSaveData(noteEditorData));
                }
                else
                {
                    bombNotes.Add(V3Converters.CreateBombNoteSaveData(noteEditorData));
                }
            }
            bpmChanges.Sort(LegacySavingUtil.SortByBeat);
            rotationEvents.Sort(LegacySavingUtil.SortByRotationTypeAndBeat);
            basicEvents.Sort(LegacySavingUtil.SortByEventTypeAndBeat);
            colorBoostEvents.Sort(LegacySavingUtil.SortByBeat);
            colorNotes.Sort(LegacySavingUtil.SortByBeat);
            bombNotes.Sort(LegacySavingUtil.SortByBeat);
            waypoints.Sort(LegacySavingUtil.SortByBeat);
            obstacles.Sort(LegacySavingUtil.SortByBeat);
            sliders.Sort(LegacySavingUtil.SortByBeat);
            burstSliders.Sort(LegacySavingUtil.SortByBeat);
            var vfxCollection = new FxEventsCollection();
            var tuple = (from e in eventBoxGroupsDataModel.GetAllEventBoxGroups()
                orderby e.beat
                select e).Select(x => V3Converters.CreateEventBoxGroupSaveDataWithFxCollection(x, vfxCollection, eventBoxGroupsDataModel)).Aggregate((new List<LightColorEventBoxGroup>(), new List<LightRotationEventBoxGroup>(), new List<LightTranslationEventBoxGroup>(), new List<FxEventBoxGroup>()), V3Converters.SplitEventBoxGroupsSaveData);
            List<LightColorEventBoxGroup> lightColorEventBoxGroups = tuple.Item1;
            List<LightRotationEventBoxGroup> lightRotationEventBoxGroups = tuple.Item2;
            List<LightTranslationEventBoxGroup> lightTranslationEventBoxGroups = tuple.Item3;
            List<FxEventBoxGroup> fxEventBoxGroups = tuple.Item4;
            BasicEventTypesWithKeywords basicEventTypesWithKeywords = new BasicEventTypesWithKeywords(basicEventsModel.GetBasicEventTypesForKeywordData().Select(V3Converters.CreateBasicEventTypesForKeywordSaveData).ToList());

            _siraLog.Info($"Saving 1: {CustomDataRepository.GetBeatmapData() == null}");
            _siraLog.Info($"Saving 2: {CustomDataRepository.GetBeatmapData().customData == null}");

            var customData = CustomDataRepository.GetBeatmapData().customData;

            _siraLog.Info($"Saving 3: {CustomDataRepository.GetCustomEvents() == null}");
            _siraLog.Info($"Saving 3: {CustomDataRepository.GetCustomEvents().Select(x => new CustomEventDataSerialized(x)) == null}");

            customData["customEvents"] = CustomDataRepository.GetCustomEvents().Select(x => new CustomEventDataSerialized(x));
            
            return new CustomBeatmapSaveDataVersioned(MapContext.Version.ToString(), bpmChanges, rotationEvents, colorNotes, bombNotes, obstacles, sliders, burstSliders, waypoints, basicEvents, colorBoostEvents, lightColorEventBoxGroups, lightRotationEventBoxGroups, lightTranslationEventBoxGroups, fxEventBoxGroups, vfxCollection, basicEventTypesWithKeywords, basicEventsModel.GetUseNormalEventsAsCompatibleEvents(), customData);
        }

        public void Save(BeatmapProjectManager projectManager, DifficultyBeatmapData difficultyBeatmapData, bool clearDirty)
        {
            if (!projectManager._beatmapDataModelsSaver.NeedsSaving())
            {
                return;
            }

            if (projectManager._beatmapDataModelsSaver.BeatmapNeedSaving() || projectManager._beatmapDataModelsSaver.LightshowNeedsSaving())
            {
                var beatmapSaveData = GetSaveData(projectManager, difficultyBeatmapData);
                LegacySavingUtil.SerializeAndSave(projectManager._workingBeatmapProject, difficultyBeatmapData.beatmapFilename, beatmapSaveData);
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
