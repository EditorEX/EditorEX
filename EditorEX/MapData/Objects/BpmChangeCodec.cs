using BeatmapEditor3D.DataModels;
using V3 = BeatmapSaveDataVersion3;

namespace EditorEX.MapData.Objects
{
    public static class BpmChangeCodec
    {
        public static V3.BpmChangeEventData SaveV3(BpmRegion r)
        {
            return new V3.BpmChangeEventData(r.startBeat, r.bpm);
        }
    }
}
