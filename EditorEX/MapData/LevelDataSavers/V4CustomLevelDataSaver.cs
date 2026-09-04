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

        private V4CustomLevelDataSaver(ICustomDataRepository customDataRepository)
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

        private CustomBeatmapSaveDataV4 SaveBeatmapObjects(BeatmapProjectManager projectManager)
        {
            var objects = projectManager._beatmapObjectsDataModel;
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

            foreach (BaseEditorData njs in objects.noteJumpSpeedEvents)
            {
                if (njs is not NoteJumpSpeedEditorData njsEditor)
                {
                    continue;
                }

                int index = njsEvents.GetIndex(NjsEventCodec.SaveV4Data(njsEditor));
                njsIndices.Add(
                    new CustomBeatIndex
                    {
                        b = njsEditor.beat,
                        i = index,
                        customData = PlacementCustomData(njsEditor),
                    }
                );
            }

            njsIndices.Sort((a, b) => a.b.CompareTo(b.b));

            HashSet<BeatmapEditorObjectId> seen = new HashSet<BeatmapEditorObjectId>();
            foreach (BaseEditorData allBeatmapObject in objects.allBeatmapObjects)
            {
                if (!seen.Add(allBeatmapObject.id))
                {
                    continue;
                }

                switch (allBeatmapObject)
                {
                    case NoteEditorData note when note.noteType == NoteType.Note:
                    {
                        int index = colorNotes.GetIndex(ColorNoteCodec.SaveV4Data(note));
                        colorNoteIndices.Add(Placement(note.beat, note.rotation, index, note));
                        break;
                    }
                    case NoteEditorData bomb when bomb.noteType == NoteType.Bomb:
                    {
                        int index = bombNotes.GetIndex(BombNoteCodec.SaveV4Data(bomb));
                        bombNoteIndices.Add(Placement(bomb.beat, bomb.rotation, index, bomb));
                        break;
                    }
                    case ObstacleEditorData obstacle when ObstacleCodec.CanSaveV4(obstacle):
                    {
                        int index = obstacles.GetIndex(ObstacleCodec.SaveV4Data(obstacle));
                        obstacleIndices.Add(
                            Placement(obstacle.beat, obstacle.rotation, index, obstacle)
                        );
                        break;
                    }
                    case ChainEditorData chain:
                    {
                        int noteIndex = colorNotes.GetIndex(
                            ColorNoteCodec.SaveV4DataFromChain(chain)
                        );
                        colorNoteIndices.Add(
                            Placement(chain.beat, chain.rotation, noteIndex, chain)
                        );
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
                                customData = PlacementCustomData(chain),
                            }
                        );
                        break;
                    }
                    case ArcEditorData arc:
                    {
                        int headIndex = colorNotes.GetIndex(
                            ColorNoteCodec.SaveV4DataFromArcHead(arc)
                        );
                        int tailIndex = colorNotes.GetIndex(
                            ColorNoteCodec.SaveV4DataFromArcTail(arc)
                        );
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
                                customData = PlacementCustomData(arc),
                            }
                        );
                        break;
                    }
                }
            }

            var customData = _customDataRepository.GetBeatmapData()?.customData ?? new CustomData();
            CustomEventCodec.Write(customData, _customDataRepository.GetCustomEvents(), v3: true);

            return new CustomBeatmapSaveDataV4
            {
                version = MapContext.Version?.ToString() ?? CustomBeatmapSaveDataV4.CurrentVersion,
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
                waypointIndices.Add(Placement(waypoint.beat, 0, index, waypoint));
            }

            foreach (BasicEventEditorData evt in basicEventsModel.GetAllEventsAsList())
            {
                int index = basicEvents.GetIndex(BasicEventCodec.SaveV4Data(evt));
                basicEventIndices.Add(
                    new CustomBeatIndex
                    {
                        b = evt.beat,
                        i = index,
                        customData = PlacementCustomData(evt),
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
                        customData = PlacementCustomData(evt),
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

        private CustomBeatmapBeatIndex Placement(
            float beat,
            int rotation,
            int index,
            BaseEditorData data
        )
        {
            return new CustomBeatmapBeatIndex
            {
                b = beat,
                r = rotation,
                i = index,
                customData = PlacementCustomData(data),
            };
        }

        private CustomData? PlacementCustomData(BaseEditorData data)
        {
            return CustomDataUtil.SaveCustom(data, _customDataRepository, out var customData)
                ? customData
                : null;
        }
    }
}
