using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using BeatmapSaveDataCommon;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.VersionedSaveData;
using EditorEX.MapData.Bookmarks;
using EditorEX.MapData.Contexts;
using EditorEX.MapData.Objects;
using V3 = BeatmapSaveDataVersion3;

namespace EditorEX.MapData.LevelDataSavers
{
    public class V3CustomLevelDataSaver : ICustomLevelDataSaver
    {
        private readonly ICustomDataRepository _customDataRepository;

        internal V3CustomLevelDataSaver(ICustomDataRepository customDataRepository)
        {
            _customDataRepository = customDataRepository;
        }

        public bool IsVersion(Version version)
        {
            return version.Major == 3;
        }

        internal static CustomBeatmapSaveDataVersioned Build(
            DifficultySaveInput input,
            ICustomDataRepository repo
        )
        {
            var rotationEvents = new List<RotationEventData>();
            var basicEvents = new List<V3.BasicEventData>();
            var colorBoostEvents = new List<ColorBoostEventData>();
            foreach (BasicEventEditorData evt in input.BasicEvents)
            {
                switch (evt.type)
                {
                    case BasicBeatmapEventType.Event14:
                    case BasicBeatmapEventType.Event15:
                        rotationEvents.Add(RotationEventCodec.SaveV3(evt, repo));
                        break;
                    case BasicBeatmapEventType.Event5:
                        colorBoostEvents.Add(ColorBoostEventCodec.SaveV3(evt, repo));
                        break;
                    default:
                        basicEvents.Add(BasicEventCodec.SaveV3(evt, repo));
                        break;
                }
            }

            var colorNotes = new List<ColorNoteData>();
            var bombNotes = new List<BombNoteData>();
            var obstacles = new List<V3.ObstacleData>();
            var sliders = new List<V3.SliderData>();
            var burstSliders = new List<BurstSliderData>();
            var waypoints = new List<V3.WaypointData>();
            foreach (NoteEditorData note in input.Notes)
            {
                if (note.noteType == NoteType.Note)
                {
                    colorNotes.Add(ColorNoteCodec.SaveV3(note, repo));
                }
                else
                {
                    bombNotes.Add(BombNoteCodec.SaveV3(note, repo));
                }
            }

            foreach (WaypointEditorData waypoint in input.Waypoints)
            {
                waypoints.Add(WaypointCodec.SaveV3(waypoint, repo));
            }

            foreach (ObstacleEditorData obstacle in input.Obstacles)
            {
                obstacles.Add(ObstacleCodec.SaveV3(obstacle, repo));
            }

            foreach (ChainEditorData chain in input.Chains)
            {
                colorNotes.Add(ColorNoteCodec.SaveV3FromChain(chain, repo));
                burstSliders.Add(ChainCodec.SaveV3(chain, repo));
            }

            foreach (ArcEditorData arc in input.Arcs)
            {
                sliders.Add(ArcCodec.SaveV3(arc, repo));
            }

            var bpmChanges = input.BpmChanges.ToList();
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
            var tuple = input
                .EventBoxGroups.OrderBy(e => e.eventBoxGroup.beat)
                .Select(x => EventBoxGroupCodec.SaveV3FromInput(x, vfxCollection, repo))
                .Aggregate(
                    (
                        new List<LightColorEventBoxGroup>(),
                        new List<LightRotationEventBoxGroup>(),
                        new List<LightTranslationEventBoxGroup>(),
                        new List<FxEventBoxGroup>()
                    ),
                    EventBoxGroupCodec.SplitV3
                );

            BasicEventTypesWithKeywords basicEventTypesWithKeywords =
                new BasicEventTypesWithKeywords(
                    input.BasicEventTypesForKeyword.Select(BasicEventCodec.SaveKeywordV3).ToList()
                );

            var sourceCustomData =
                repo.GetBeatmapData()?.customData ?? repo.GetCustomBeatmapSaveData()?.customData;
            var customData =
                sourceCustomData == null ? new CustomData() : new CustomData(sourceCustomData);
            CustomEventCodec.Write(customData, repo.GetCustomEvents(), v3: true);
            CustomDataBookmarkCodec.Write(customData, input.Bookmarks, v3: true);

            Version version = input.MapVersion ?? MapContext.Version ?? new Version(3, 3, 0);
            return new CustomBeatmapSaveDataVersioned(
                version.ToString(),
                bpmChanges,
                rotationEvents,
                colorNotes,
                bombNotes,
                obstacles,
                sliders,
                burstSliders,
                waypoints,
                basicEvents,
                colorBoostEvents,
                tuple.Item1,
                tuple.Item2,
                tuple.Item3,
                tuple.Item4,
                vfxCollection,
                basicEventTypesWithKeywords,
                input.UseNormalEventsAsCompatibleEvents,
                customData
            );
        }

        private CustomBeatmapSaveDataVersioned GetSaveData(BeatmapProjectManager projectManager)
        {
            var basicEventsModel = projectManager._beatmapBasicEventsDataModel;
            var levelDataModel = projectManager._beatmapObjectsDataModel;
            var eventBoxGroupsDataModel = projectManager._beatmapEventBoxGroupsDataModel;

            var input = new DifficultySaveInput
            {
                BpmChanges = projectManager
                    ._audioDataModel.bpmData.regions.Select(BpmChangeCodec.SaveV3)
                    .ToList(),
                BasicEvents = basicEventsModel
                    .GetAllEventsAsList()
                    .Concat(basicEventsModel.GetAllDataIn(BasicBeatmapEventType.Event14))
                    .Concat(basicEventsModel.GetAllDataIn(BasicBeatmapEventType.Event15))
                    .Concat(basicEventsModel.GetAllDataIn(BasicBeatmapEventType.Event5))
                    .ToList(),
                BasicEventTypesForKeyword = basicEventsModel
                    .GetBasicEventTypesForKeywordData()
                    .ToList(),
                UseNormalEventsAsCompatibleEvents =
                    basicEventsModel.GetUseNormalEventsAsCompatibleEvents(),
                Bookmarks = CustomDataBookmarkCodec.Flatten(projectManager._bookmarksDataModel),
                MapVersion = MapContext.Version,
            };

            foreach (
                BaseBeatmapObjectEditorData allBeatmapObject in levelDataModel.allBeatmapObjects
            )
            {
                switch (allBeatmapObject)
                {
                    case NoteEditorData note when note.noteType == NoteType.Note:
                        input.Notes.Add(note);
                        break;
                    case NoteEditorData bomb:
                        input.Notes.Add(bomb);
                        break;
                    case WaypointEditorData waypoint:
                        input.Waypoints.Add(waypoint);
                        break;
                    case ObstacleEditorData obstacle:
                        input.Obstacles.Add(obstacle);
                        break;
                    case ChainEditorData chain:
                        input.Chains.Add(chain);
                        break;
                    case ArcEditorData arc:
                        input.Arcs.Add(arc);
                        break;
                }
            }

            foreach (
                EventBoxGroupEditorData group in eventBoxGroupsDataModel
                    .GetAllEventBoxGroups()
                    .OrderBy(e => e.beat)
            )
            {
                IReadOnlyList<EventBoxEditorData> boxes =
                    eventBoxGroupsDataModel.GetEventBoxesByEventBoxGroupId(group.id);
                var baseLists = new List<(BeatmapEditorObjectId, List<BaseEditorData>)>();
                foreach (EventBoxEditorData box in boxes)
                {
                    baseLists.Add(
                        (
                            box.id,
                            EventBoxGroupCodec.GetBaseEventsFromModel(
                                eventBoxGroupsDataModel,
                                group.type,
                                box.id
                            )
                        )
                    );
                }

                input.EventBoxGroups.Add(
                    new BeatmapEditorEventBoxGroupInput
                    {
                        eventBoxGroup = group,
                        eventBoxes = boxes,
                        baseLists = baseLists,
                    }
                );
            }

            return Build(input, _customDataRepository);
        }

        public void Save(
            BeatmapProjectManager projectManager,
            DifficultyBeatmapData difficultyBeatmapData,
            bool clearDirty
        )
        {
            if (LevelDataSaveOps.ShouldSkipSave(projectManager))
            {
                return;
            }

            if (LevelDataSaveOps.BeatmapOrLightshowOrBookmarksNeedSaving(projectManager))
            {
                var beatmapSaveData = GetSaveData(projectManager);
                LegacySavingUtil.SerializeAndSave(
                    projectManager._workingBeatmapProject,
                    difficultyBeatmapData.beatmapFilename,
                    beatmapSaveData
                );
                if (clearDirty)
                {
                    LevelDataSaveOps.ClearDifficultyDirty(projectManager);
                }
            }

            LevelDataSaveOps.BackupAndSaveTemp(projectManager, clearDirty);
        }
    }
}
