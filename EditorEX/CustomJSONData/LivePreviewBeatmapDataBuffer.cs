using CustomJSONData.CustomBeatmap;

namespace EditorEX.CustomJSONData
{
    /// <summary>
    /// Holds the custom live-preview <see cref="CustomBeatmapData"/> created while
    /// <c>Bind&lt;BeatmapData&gt;().FromInstance(...)</c> is still open (no Resolve allowed).
    /// Flushed into <see cref="ICustomDataRepository"/> once DI is safe.
    /// </summary>
    internal static class LivePreviewBeatmapDataBuffer
    {
        public static CustomBeatmapData? Pending { get; set; }

        public static CustomBeatmapData? Consume()
        {
            var data = Pending;
            Pending = null;
            return data;
        }
    }
}
