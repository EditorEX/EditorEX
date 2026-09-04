using CustomJSONData.CustomBeatmap;
using V4 = BeatmapSaveDataVersion4;

namespace EditorEX.CustomJSONData.VersionedSaveData
{
    public class CustomBeatmapBeatIndex : V4.BeatmapBeatIndex
    {
        public CustomData customData;
    }

    public class CustomBeatIndex : V4.BeatIndex
    {
        public CustomData customData;
    }

    public class CustomChainBeatIndex : V4.ChainBeatIndex
    {
        public CustomData customData;
    }

    public class CustomArcBeatIndex : V4.ArcBeatIndex
    {
        public CustomData customData;
    }
}
