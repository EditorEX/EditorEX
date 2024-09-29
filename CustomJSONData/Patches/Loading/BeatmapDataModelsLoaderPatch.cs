using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using EditorEX.MapData.LevelDataLoaders;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace EditorEX.CustomJSONData.Patches.Loading
{
    [AffinityPatch]
    internal class BeatmapDataModelsLoaderPatch : IAffinity
    {
        private LevelDataLoaderV2 _levelDataLoaderV2;
        private LevelDataLoaderV3 _levelDataLoaderV3;

        [Inject]
        private void Construct(LevelDataLoaderV2 levelDataLoaderV2, LevelDataLoaderV3 levelDataLoaderV3)
        {
            _levelDataLoaderV2 = levelDataLoaderV2;
            _levelDataLoaderV3 = levelDataLoaderV3;
        }

        [AffinityPatch(typeof(BeatmapDataModelsLoader), nameof(BeatmapDataModelsLoader.Load_v2Raw))]
        [AffinityPrefix]
        private bool Loadv2(BeatmapDataModelsLoader __instance, string projectPath, string filename, out List<NoteEditorData> notes, out List<WaypointEditorData> waypoints, out List<ObstacleEditorData> obstacles, out List<ArcEditorData> sliders, out List<BasicEventEditorData> events, out List<BasicEventTypesForKeywordEditorData> basicEventTypesForKeywordEditorData)
        {
            _levelDataLoaderV2.Load(__instance, projectPath, filename, out notes, out waypoints, out obstacles, out sliders, out events, out basicEventTypesForKeywordEditorData);
            return false;
        }

        [AffinityPatch(typeof(BeatmapDataModelsLoader), nameof(BeatmapDataModelsLoader.Load_v3Raw))]
        [AffinityPrefix]
        private bool Loadv3(BeatmapDataModelsLoader __instance, string projectPath, string filename, out List<NoteEditorData> notes, out List<WaypointEditorData> waypoints, out List<ObstacleEditorData> obstacles, out List<ArcEditorData> arcs, out List<ChainEditorData> chains, out List<BasicEventEditorData> basicEvents, out List<BeatmapEditorEventBoxGroupInput> eventBoxGroups, out List<BasicEventTypesForKeywordEditorData> basicEventTypesForKeywordEditorData, out bool useNormalEventsAsCompatibleEvents)
        {
            _levelDataLoaderV3.Load(__instance, projectPath, filename, out notes, out waypoints, out obstacles, out arcs, out chains, out basicEvents, out eventBoxGroups, out basicEventTypesForKeywordEditorData, out useNormalEventsAsCompatibleEvents);
            return false;
        }
    }
}
