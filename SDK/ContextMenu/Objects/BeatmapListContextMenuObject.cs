using static BeatmapEditor3D.DataModels.BeatmapsCollectionDataModel;

namespace EditorEX.SDK.ContextMenu.Objects
{
    public class BeatmapListContextMenuObject : IContextMenuObject
    {
        public BeatmapListContextMenuObject(BeatmapInfoData beatmapInfoData)
        {
            this.BeatmapInfoData = beatmapInfoData;
        }

        public BeatmapInfoData BeatmapInfoData { get; private set; }
    }
}
