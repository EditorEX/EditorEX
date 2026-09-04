using System;
using CustomJSONData.CustomBeatmap;
using V4 = BeatmapSaveDataVersion4;

namespace EditorEX.CustomJSONData.VersionedSaveData
{
    public class CustomBeatmapSaveDataV4
    {
        public const string CurrentVersion = "4.1.0";

        public string version = CurrentVersion;

        public CustomBeatmapBeatIndex[] colorNotes = Array.Empty<CustomBeatmapBeatIndex>();

        public CustomBeatmapBeatIndex[] bombNotes = Array.Empty<CustomBeatmapBeatIndex>();

        public CustomBeatmapBeatIndex[] obstacles = Array.Empty<CustomBeatmapBeatIndex>();

        public CustomChainBeatIndex[] chains = Array.Empty<CustomChainBeatIndex>();

        public CustomArcBeatIndex[] arcs = Array.Empty<CustomArcBeatIndex>();

        public V4.ColorNote[] colorNotesData = Array.Empty<V4.ColorNote>();

        public V4.BombNote[] bombNotesData = Array.Empty<V4.BombNote>();

        public V4.Obstacle[] obstaclesData = Array.Empty<V4.Obstacle>();

        public V4.Chain[] chainsData = Array.Empty<V4.Chain>();

        public V4.Arc[] arcsData = Array.Empty<V4.Arc>();

        public CustomBeatIndex[] njsEvents = Array.Empty<CustomBeatIndex>();

        public V4.NoteJumpMovementSpeedEvent[] njsEventData =
            Array.Empty<V4.NoteJumpMovementSpeedEvent>();

        public CustomData customData;
    }
}
