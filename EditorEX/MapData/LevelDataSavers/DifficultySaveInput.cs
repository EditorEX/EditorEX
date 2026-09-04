using System;
using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using BeatmapSaveDataVersion3;
using EditorEX.CustomJSONData;
using EditorEX.MapData.Bookmarks;
using EditorEX.MapData.LevelDataLoaders;

namespace EditorEX.MapData.LevelDataSavers
{
    public sealed class DifficultySaveInput
    {
        public List<NoteEditorData> Notes { get; set; } = new();

        public List<WaypointEditorData> Waypoints { get; set; } = new();

        public List<ObstacleEditorData> Obstacles { get; set; } = new();

        public List<ArcEditorData> Arcs { get; set; } = new();

        public List<ChainEditorData> Chains { get; set; } = new();

        public List<BasicEventEditorData> BasicEvents { get; set; } = new();

        public List<BeatmapEditorEventBoxGroupInput> EventBoxGroups { get; set; } = new();

        public List<BasicEventTypesForKeywordEditorData> BasicEventTypesForKeyword { get; set; } =
            new();

        public List<BpmChangeEventData> BpmChanges { get; set; } = new();

        public List<CustomDataBookmark> Bookmarks { get; set; } = new();

        public bool UseNormalEventsAsCompatibleEvents { get; set; }

        public Version? MapVersion { get; set; }

        public static DifficultySaveInput FromLoadResult(
            DifficultyLoadResult loaded,
            ICustomDataRepository repo
        )
        {
            var customData =
                repo.GetBeatmapData()?.customData ?? repo.GetCustomBeatmapSaveData()?.customData;
            return new DifficultySaveInput
            {
                Notes = loaded.Notes,
                Waypoints = loaded.Waypoints,
                Obstacles = loaded.Obstacles,
                Arcs = loaded.Arcs,
                Chains = loaded.Chains,
                BasicEvents = loaded.BasicEvents,
                EventBoxGroups = loaded.EventBoxGroups,
                BasicEventTypesForKeyword = loaded.BasicEventTypesForKeyword,
                BpmChanges = loaded.BpmChanges,
                Bookmarks = CustomDataBookmarkCodec.Read(customData, v3: true),
                UseNormalEventsAsCompatibleEvents = loaded.UseNormalEventsAsCompatibleEvents,
            };
        }
    }
}
