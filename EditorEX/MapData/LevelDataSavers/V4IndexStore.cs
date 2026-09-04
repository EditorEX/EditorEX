using System.Collections.Generic;
using BeatmapEditor3D.SerializedData;

namespace EditorEX.MapData.LevelDataSavers
{
    public sealed class V4IndexStore<TData>
        where TData : notnull
    {
        public Dictionary<TData, int> Map { get; } = new();

        public List<TData> Data { get; } = new();

        public int GetIndex(TData item)
        {
            return BeatmapSaverUtils.GetIndex(item, Map, Data);
        }
    }
}
