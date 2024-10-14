using static BeatmapEditor3D.DataModels.BeatmapsCollectionDataModel;

namespace EditorEX.SDK.ContextMenu
{
    public class BeatmapListContextMenuObject : IContextMenuObject
    {
        public BeatmapListContextMenuObject(BeatmapInfoData beatmapInfoData)
        {
            this.beatmapInfoData = beatmapInfoData;
        }

        public BeatmapInfoData beatmapInfoData { get; private set; }
    }
}
