using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using BeatmapSaveDataCommon;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.VersionedSaveData;
using EditorEX.MapData.Contexts;
using EditorEX.MapData.Objects;
using V4 = BeatmapSaveDataVersion4;

namespace EditorEX.MapData.LevelDataSavers
{
    public class V4CustomLevelDataSaver : ICustomLevelDataSaver
    {
        private readonly ICustomDataRepository _customDataRepository;

        internal V4CustomLevelDataSaver(ICustomDataRepository customDataRepository)
        {
            _customDataRepository = customDataRepository;
        }

        public bool IsVersion(Version version)
        {
            return version.Major == 4;
        }

        public void Save(
            BeatmapProjectManager projectManager,
            DifficultyBeatmapData difficultyBeatmapData,
            bool clearDirty
        )
        {
            if (
                !projectManager._beatmapDataModelsSaver.NeedsSaving()
                && !projectManager._bookmarkDataModelSaver.NeedsSaving()
                && !projectManager._recordingsDataModel.isDirty
            )
            {
                return;
            }

            if (projectManager._bookmarkDataModelSaver.NeedsSaving())
            {
                BeatmapProjectFileHelper.CreateSubdirectoryIfNotExists(
                    projectManager._workingBeatmapProject,
                    "Bookmarks"
                );
                var list = projectManager._bookmarkDataModelSaver.Save();
                foreach (var valueTuple in list)
                {
                    string item = valueTuple.Item1;
                    var item2 = valueTuple.Item2;
                    if (
                        item != null
                        && BeatmapProjectFileHelper.BookmarkFilenameChanged(item, item2)
                    )
                    {
                        BeatmapProjectFileHelper.TryDeleteBookmarkSet(
                            projectManager._workingBeatmapProject,
                            item
                        );
                    }
                }

                foreach (var valueTuple2 in list)
                {
                    string item3 = valueTuple2.Item1;
                    var item4 = valueTuple2.Item2;
                    BeatmapProjectFileHelper.SaveBookmarkSet(
                        projectManager._workingBeatmapProject,
                        item4
                    );
                    string bookmarkSetFilename = BeatmapProjectFileHelper.GetBookmarkSetFilename(
                        item4
                    );
                    projectManager._bookmarksDataModel.UpdateBookmarkSetFileName(
                        item3,
                        bookmarkSetFilename
                    );
                }

                if (clearDirty)
                {
                    projectManager._bookmarksDataModel.ClearDirty();
                }
            }

            if (projectManager._beatmapDataModelsSaver.BeatmapNeedSaving())
            {
                var beatmapSaveData = SaveBeatmapObjects(projectManager);
                LegacySavingUtil.SerializeAndSave(
                    projectManager._workingBeatmapProject,
                    difficultyBeatmapData.beatmapFilename,
                    beatmapSaveData
                );
                if (clearDirty)
                {
                    projectManager._beatmapObjectsDataModel.ClearDirty();
                }
            }

            if (projectManager._beatmapDataModelsSaver.LightshowNeedsSaving())
            {
                var lightshowSaveData = SaveLightshow(projectManager);
                LegacySavingUtil.SerializeAndSave(
                    projectManager._workingBeatmapProject,
                    difficultyBeatmapData.lightshowFilename,
                    lightshowSaveData
                );
                if (clearDirty)
                {
                    projectManager._beatmapBasicEventsDataModel.ClearDirty();
                    projectManager._beatmapEventBoxGroupsDataModel.ClearDirty();
                }
            }

            LevelDataSaveOps.BackupAndSaveTemp(projectManager, clearDirty);
        }

        internal static CustomBeatmapSaveDataV4 BuildBeatmap(
            DifficultySaveInput input,
            ICustomDataRepository repo
        )
        {
            var colorNotes = new V4IndexStore<V4.ColorNote>();
            var bombNotes = new V4IndexStore<V4.BombNote>();
            var obstacles = new V4IndexStore<V4.Obstacle>();
            var chains = new V4IndexStore<V4.Chain>();
            var arcs = new V4IndexStore<V4.Arc>();
            var njsEvents = new V4IndexStore<V4.NoteJumpMovementSpeedEvent>();
            var colorNoteIndices = new List<CustomBeatmapBeatIndex>();
            var bombNoteIndices = new List<CustomBeatmapBeatIndex>();
            var obstacleIndices = new List<CustomBeatmapBeatIndex>();
            var chainIndices = new List<CustomChainBeatIndex>();
            var arcIndices = new List<CustomArcBeatIndex>();
            var njsIndices = new List<CustomBeatIndex>();

            foreach (NoteJumpSpeedEditorData njsEditor in input.NjsEvents)
            {
                int index = njsEvents.GetIndex(NjsEventCodec.SaveV4Data(njsEditor));
                njsIndices.Add(
                    new CustomBeatIndex
                    {
                        b = njsEditor.beat,
                        i = index,
                        customData = PlacementCustomData(njsEditor, repo),
                    }
                );
            }

            njsIndices.Sort((a, b) => a.b.CompareTo(b.b));

            foreach (NoteEditorData note in input.Notes)
            {
                if (note.noteType == NoteType.Note)
                {
                    int index = colorNotes.GetIndex(ColorNoteCodec.SaveV4Data(note));
                    colorNoteIndices.Add(Placement(note.beat, note.rotation, index, note, repo));
                }
                else if (note.noteType == NoteType.Bomb)
                {
                    int index = bombNotes.GetIndex(BombNoteCodec.SaveV4Data(note));
                    bombNoteIndices.Add(Placement(note.beat, note.rotation, index, note, repo));
                }
            }

            foreach (ObstacleEditorData obstacle in input.Obstacles)
            {
                if (!ObstacleCodec.CanSaveV4(obstacle))
                {
                    continue;
                }

                int index = obstacles.GetIndex(ObstacleCodec.SaveV4Data(obstacle));
                obstacleIndices.Add(
                    Placement(obstacle.beat, obstacle.rotation, index, obstacle, repo)
                );
            }

            foreach (ChainEditorData chain in input.Chains)
            {
                int noteIndex = colorNotes.GetIndex(ColorNoteCodec.SaveV4DataFromChain(chain));
                int chainIndex = chains.GetIndex(ChainCodec.SaveV4Data(chain));
                chainIndices.Add(
                    new CustomChainBeatIndex
                    {
                        hb = chain.beat,
                        hr = chain.rotation,
                        i = noteIndex,
                        tb = chain.tailBeat,
                        tr = chain.tailRotation,
                        ci = chainIndex,
                        customData = PlacementCustomData(chain, repo),
                    }
                );
            }

            foreach (ArcEditorData arc in input.Arcs)
            {
                int headIndex = colorNotes.GetIndex(ColorNoteCodec.SaveV4DataFromArcHead(arc));
                int tailIndex = colorNotes.GetIndex(ColorNoteCodec.SaveV4DataFromArcTail(arc));
                int arcIndex = arcs.GetIndex(ArcCodec.SaveV4Data(arc));
                arcIndices.Add(
                    new CustomArcBeatIndex
                    {
                        hb = arc.beat,
                        hi = headIndex,
                        hr = arc.rotation,
                        tb = arc.tailBeat,
                        ti = tailIndex,
                        tr = arc.tailRotation,
                        ai = arcIndex,
                        customData = PlacementCustomData(arc, repo),
                    }
                );
            }

            var sourceCustomData =
                repo.GetBeatmapData()?.customData ?? repo.GetCustomBeatmapSaveData()?.customData;
            var customData =
                sourceCustomData == null ? new CustomData() : new CustomData(sourceCustomData);
            CustomEventCodec.Write(customData, repo.GetCustomEvents(), v3: true);

            return new CustomBeatmapSaveDataV4
            {
                version =
                    input.MapVersion?.ToString()
                    ?? MapContext.Version?.ToString()
                    ?? CustomBeatmapSaveDataV4.CurrentVersion,
                colorNotes = colorNoteIndices.ToArray(),
                bombNotes = bombNoteIndices.ToArray(),
                obstacles = obstacleIndices.ToArray(),
                chains = chainIndices.ToArray(),
                arcs = arcIndices.ToArray(),
                njsEvents = njsIndices.ToArray(),
                colorNotesData = colorNotes.Data.ToArray(),
                bombNotesData = bombNotes.Data.ToArray(),
                obstaclesData = obstacles.Data.ToArray(),
                chainsData = chains.Data.ToArray(),
                arcsData = arcs.Data.ToArray(),
                njsEventData = njsEvents.Data.ToArray(),
                customData = customData,
            };
        }

        internal static CustomLightshowSaveDataV4 BuildLightshow(
            DifficultySaveInput input,
            ICustomDataRepository repo
        )
        {
            var waypoints = new V4IndexStore<V4.Waypoint>();
            var basicEvents = new V4IndexStore<V4.BasicEvent>();
            var colorBoostEvents = new V4IndexStore<V4.ColorBoostEvent>();
            var waypointIndices = new List<CustomBeatmapBeatIndex>();
            var basicEventIndices = new List<CustomBeatIndex>();
            var colorBoostIndices = new List<CustomBeatIndex>();

            foreach (WaypointEditorData waypoint in input.Waypoints)
            {
                int index = waypoints.GetIndex(WaypointCodec.SaveV4Data(waypoint));
                waypointIndices.Add(Placement(waypoint.beat, 0, index, waypoint, repo));
            }

            foreach (BasicEventEditorData evt in input.BasicEvents)
            {
                if (evt.type == BasicBeatmapEventType.Event5)
                {
                    int index = colorBoostEvents.GetIndex(ColorBoostEventCodec.SaveV4Data(evt));
                    colorBoostIndices.Add(
                        new CustomBeatIndex
                        {
                            b = evt.beat,
                            i = index,
                            customData = PlacementCustomData(evt, repo),
                        }
                    );
                    continue;
                }

                int basicIndex = basicEvents.GetIndex(BasicEventCodec.SaveV4Data(evt));
                basicEventIndices.Add(
                    new CustomBeatIndex
                    {
                        b = evt.beat,
                        i = basicIndex,
                        customData = PlacementCustomData(evt, repo),
                    }
                );
            }

            var lightshow = new CustomLightshowSaveDataV4
            {
                version = CustomLightshowSaveDataV4.CurrentVersion,
                waypoints = waypointIndices.ToArray(),
                waypointsData = waypoints.Data.ToArray(),
                basicEvents = basicEventIndices.ToArray(),
                basicEventsData = basicEvents.Data.ToArray(),
                colorBoostEvents = colorBoostIndices.ToArray(),
                colorBoostEventsData = colorBoostEvents.Data.ToArray(),
                useNormalEventsAsCompatibleEvents = input.UseNormalEventsAsCompatibleEvents,
                basicEventTypesWithKeywords = new BasicEventTypesWithKeywords(
                    input.BasicEventTypesForKeyword.Select(BasicEventCodec.SaveKeywordV3).ToList()
                ),
            };

            var vanilla = lightshow.ToVanilla();
            EventBoxGroupCodec.SaveV4FromInput(input.EventBoxGroups, vanilla);
            lightshow.eventBoxGroups = vanilla.eventBoxGroups;
            lightshow.indexFilters = vanilla.indexFilters;
            lightshow.lightColorEventBoxes = vanilla.lightColorEventBoxes;
            lightshow.lightColorEvents = vanilla.lightColorEvents;
            lightshow.lightRotationEventBoxes = vanilla.lightRotationEventBoxes;
            lightshow.lightRotationEvents = vanilla.lightRotationEvents;
            lightshow.lightTranslationEventBoxes = vanilla.lightTranslationEventBoxes;
            lightshow.lightTranslationEvents = vanilla.lightTranslationEvents;
            lightshow.fxEventBoxes = vanilla.fxEventBoxes;
            lightshow.floatFxEvents = vanilla.floatFxEvents;
            return lightshow;
        }

        private CustomBeatmapSaveDataV4 SaveBeatmapObjects(BeatmapProjectManager projectManager)
        {
            var objects = projectManager._beatmapObjectsDataModel;
            var input = new DifficultySaveInput { MapVersion = MapContext.Version };
            foreach (BaseEditorData njs in objects.noteJumpSpeedEvents)
            {
                if (njs is NoteJumpSpeedEditorData njsEditor)
                {
                    input.NjsEvents.Add(njsEditor);
                }
            }

            HashSet<BeatmapEditorObjectId> seen = new HashSet<BeatmapEditorObjectId>();
            foreach (BaseEditorData allBeatmapObject in objects.allBeatmapObjects)
            {
                if (!seen.Add(allBeatmapObject.id))
                {
                    continue;
                }

                switch (allBeatmapObject)
                {
                    case NoteEditorData note:
                        input.Notes.Add(note);
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

            return BuildBeatmap(input, _customDataRepository);
        }

        private CustomLightshowSaveDataV4 SaveLightshow(BeatmapProjectManager projectManager)
        {
            var basicEventsModel = projectManager._beatmapBasicEventsDataModel;
            var waypoints = new V4IndexStore<V4.Waypoint>();
            var basicEvents = new V4IndexStore<V4.BasicEvent>();
            var colorBoostEvents = new V4IndexStore<V4.ColorBoostEvent>();
            var waypointIndices = new List<CustomBeatmapBeatIndex>();
            var basicEventIndices = new List<CustomBeatIndex>();
            var colorBoostIndices = new List<CustomBeatIndex>();

            foreach (WaypointEditorData waypoint in basicEventsModel.waypoints)
            {
                int index = waypoints.GetIndex(WaypointCodec.SaveV4Data(waypoint));
                waypointIndices.Add(
                    Placement(waypoint.beat, 0, index, waypoint, _customDataRepository)
                );
            }

            foreach (BasicEventEditorData evt in basicEventsModel.GetAllEventsAsList())
            {
                int index = basicEvents.GetIndex(BasicEventCodec.SaveV4Data(evt));
                basicEventIndices.Add(
                    new CustomBeatIndex
                    {
                        b = evt.beat,
                        i = index,
                        customData = PlacementCustomData(evt, _customDataRepository),
                    }
                );
            }

            foreach (
                BasicEventEditorData evt in basicEventsModel.GetAllDataIn(
                    BasicBeatmapEventType.Event5
                )
            )
            {
                int index = colorBoostEvents.GetIndex(ColorBoostEventCodec.SaveV4Data(evt));
                colorBoostIndices.Add(
                    new CustomBeatIndex
                    {
                        b = evt.beat,
                        i = index,
                        customData = PlacementCustomData(evt, _customDataRepository),
                    }
                );
            }

            var lightshow = new CustomLightshowSaveDataV4
            {
                version = CustomLightshowSaveDataV4.CurrentVersion,
                waypoints = waypointIndices.ToArray(),
                waypointsData = waypoints.Data.ToArray(),
                basicEvents = basicEventIndices.ToArray(),
                basicEventsData = basicEvents.Data.ToArray(),
                colorBoostEvents = colorBoostIndices.ToArray(),
                colorBoostEventsData = colorBoostEvents.Data.ToArray(),
                useNormalEventsAsCompatibleEvents =
                    basicEventsModel.GetUseNormalEventsAsCompatibleEvents(),
                basicEventTypesWithKeywords = new BasicEventTypesWithKeywords(
                    basicEventsModel
                        .GetBasicEventTypesForKeywordData()
                        .Select(BasicEventCodec.SaveKeywordV3)
                        .ToList()
                ),
            };

            var vanilla = lightshow.ToVanilla();
            EventBoxGroupCodec.SaveV4(projectManager._beatmapEventBoxGroupsDataModel, vanilla);
            lightshow.eventBoxGroups = vanilla.eventBoxGroups;
            lightshow.indexFilters = vanilla.indexFilters;
            lightshow.lightColorEventBoxes = vanilla.lightColorEventBoxes;
            lightshow.lightColorEvents = vanilla.lightColorEvents;
            lightshow.lightRotationEventBoxes = vanilla.lightRotationEventBoxes;
            lightshow.lightRotationEvents = vanilla.lightRotationEvents;
            lightshow.lightTranslationEventBoxes = vanilla.lightTranslationEventBoxes;
            lightshow.lightTranslationEvents = vanilla.lightTranslationEvents;
            lightshow.fxEventBoxes = vanilla.fxEventBoxes;
            lightshow.floatFxEvents = vanilla.floatFxEvents;
            return lightshow;
        }

        private static CustomBeatmapBeatIndex Placement(
            float beat,
            int rotation,
            int index,
            BaseEditorData data,
            ICustomDataRepository repo
        )
        {
            return new CustomBeatmapBeatIndex
            {
                b = beat,
                r = rotation,
                i = index,
                customData = PlacementCustomData(data, repo),
            };
        }

        private static CustomData? PlacementCustomData(
            BaseEditorData data,
            ICustomDataRepository repo
        )
        {
            return CustomDataUtil.SaveCustom(data, repo, out var customData) ? customData : null;
        }
    }
}
