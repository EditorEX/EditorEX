using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using BeatmapSaveDataVersion3;

namespace EditorEX.MapData.LevelDataLoaders
{
    public sealed class DifficultyLoadResult
    {
        public List<NoteEditorData> Notes { get; set; } = new();

        public List<WaypointEditorData> Waypoints { get; set; } = new();

        public List<ObstacleEditorData> Obstacles { get; set; } = new();

        public List<ArcEditorData> Arcs { get; set; } = new();

        public List<ChainEditorData> Chains { get; set; } = new();

        public List<NoteJumpSpeedEditorData> NjsEvents { get; set; } = new();

        public List<BasicEventEditorData> BasicEvents { get; set; } = new();

        public List<BeatmapEditorEventBoxGroupInput> EventBoxGroups { get; set; } = new();

        public List<BasicEventTypesForKeywordEditorData> BasicEventTypesForKeyword { get; set; } =
            new();

        public List<BpmChangeEventData> BpmChanges { get; set; } = new();

        public bool UseNormalEventsAsCompatibleEvents { get; set; }
    }
}
