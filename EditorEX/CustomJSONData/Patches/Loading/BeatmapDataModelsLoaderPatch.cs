using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using EditorEX.MapData.LevelDataLoaders;
using SiraUtil.Affinity;

namespace EditorEX.CustomJSONData.Patches.Loading
{
    internal class BeatmapDataModelsLoaderPatch : IAffinity
    {
        private readonly LevelDataLoaderV2 _levelDataLoaderV2;
        private readonly LevelDataLoaderV3 _levelDataLoaderV3;
        private readonly LevelDataLoaderV4 _levelDataLoaderV4;

        private BeatmapDataModelsLoaderPatch(
            LevelDataLoaderV2 levelDataLoaderV2,
            LevelDataLoaderV3 levelDataLoaderV3,
            LevelDataLoaderV4 levelDataLoaderV4
        )
        {
            _levelDataLoaderV2 = levelDataLoaderV2;
            _levelDataLoaderV3 = levelDataLoaderV3;
            _levelDataLoaderV4 = levelDataLoaderV4;
        }

        [AffinityPatch(typeof(BeatmapDataModelsLoader), nameof(BeatmapDataModelsLoader.Load_v2Raw))]
        [AffinityPrefix]
        private bool Loadv2(
            BeatmapDataModelsLoader __instance,
            string projectPath,
            string filename,
            out List<NoteEditorData> notes,
            out List<WaypointEditorData> waypoints,
            out List<ObstacleEditorData> obstacles,
            out List<ArcEditorData> sliders,
            out List<BasicEventEditorData> events,
            out List<BasicEventTypesForKeywordEditorData> basicEventTypesForKeywordEditorData
        )
        {
            DifficultyLoadResult result = _levelDataLoaderV2.Load(
                __instance,
                projectPath,
                filename,
                null
            );
            notes = result.Notes;
            waypoints = result.Waypoints;
            obstacles = result.Obstacles;
            sliders = result.Arcs;
            events = result.BasicEvents;
            basicEventTypesForKeywordEditorData = result.BasicEventTypesForKeyword;
            return false;
        }

        [AffinityPatch(typeof(BeatmapDataModelsLoader), nameof(BeatmapDataModelsLoader.Load_v3Raw))]
        [AffinityPrefix]
        private bool Loadv3(
            BeatmapDataModelsLoader __instance,
            string projectPath,
            string filename,
            out List<NoteEditorData> notes,
            out List<WaypointEditorData> waypoints,
            out List<ObstacleEditorData> obstacles,
            out List<ArcEditorData> arcs,
            out List<ChainEditorData> chains,
            out List<BasicEventEditorData> basicEvents,
            out List<BeatmapEditorEventBoxGroupInput> eventBoxGroups,
            out List<BasicEventTypesForKeywordEditorData> basicEventTypesForKeywordEditorData,
            out bool useNormalEventsAsCompatibleEvents
        )
        {
            DifficultyLoadResult result = _levelDataLoaderV3.Load(
                __instance,
                projectPath,
                filename,
                null
            );
            notes = result.Notes;
            waypoints = result.Waypoints;
            obstacles = result.Obstacles;
            arcs = result.Arcs;
            chains = result.Chains;
            basicEvents = result.BasicEvents;
            eventBoxGroups = result.EventBoxGroups;
            basicEventTypesForKeywordEditorData = result.BasicEventTypesForKeyword;
            useNormalEventsAsCompatibleEvents = result.UseNormalEventsAsCompatibleEvents;
            return false;
        }

        [AffinityPatch(typeof(BeatmapDataModelsLoader), nameof(BeatmapDataModelsLoader.Load_v4Raw))]
        [AffinityPrefix]
        private bool Loadv4(
            BeatmapDataModelsLoader __instance,
            string projectPath,
            string beatmapFilename,
            string lightshowFilename,
            out List<NoteEditorData> notes,
            out List<WaypointEditorData> waypoints,
            out List<ObstacleEditorData> obstacles,
            out List<ArcEditorData> arcs,
            out List<ChainEditorData> chains,
            out List<NoteJumpSpeedEditorData> noteJumpSpeedEditorEvents,
            out List<BasicEventEditorData> basicEvents,
            out List<BeatmapEditorEventBoxGroupInput> eventBoxGroups,
            out List<BasicEventTypesForKeywordEditorData> basicEventTypesForKeywordEditorData,
            out bool useNormalEventsAsCompatibleEvents
        )
        {
            DifficultyLoadResult result = _levelDataLoaderV4.Load(
                __instance,
                projectPath,
                beatmapFilename,
                lightshowFilename
            );
            notes = result.Notes;
            waypoints = result.Waypoints;
            obstacles = result.Obstacles;
            arcs = result.Arcs;
            chains = result.Chains;
            noteJumpSpeedEditorEvents = result.NjsEvents;
            basicEvents = result.BasicEvents;
            eventBoxGroups = result.EventBoxGroups;
            basicEventTypesForKeywordEditorData = result.BasicEventTypesForKeyword;
            useNormalEventsAsCompatibleEvents = result.UseNormalEventsAsCompatibleEvents;
            return false;
        }
    }
}
