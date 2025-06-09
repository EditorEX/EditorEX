using BeatmapEditor3D;
using BeatmapEditor3D.Views;
using EditorEX.UI.Components.ContextMenu;
using SiraUtil.Affinity;
using Zenject;
using static BeatmapEditor3D.DataModels.BeatmapsCollectionDataModel;

namespace EditorEX.UI.Patches.SDK
{
    internal class AddBeatmapListContextMenu : IAffinity
    {
        private readonly IInstantiator _instantiator;

        private AddBeatmapListContextMenu(
            IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        [AffinityPatch(typeof(BeatmapsListTableCell), nameof(BeatmapsListTableCell.SetData))]
        [AffinityPostfix]
        private void SetData(BeatmapsListTableCell __instance, BeatmapInfoData beatmapInfo)
        {
            BeatmapListContextMenu contextMenu = __instance.gameObject.GetComponent<BeatmapListContextMenu>();
            if (__instance.gameObject.GetComponent<BeatmapListContextMenu>() == null)
            {
                contextMenu = _instantiator.InstantiateComponent<BeatmapListContextMenu>(__instance.gameObject);
            }
            contextMenu.SetData(beatmapInfo);
        }

        [AffinityPatch(typeof(RecentBeatmapView), nameof(RecentBeatmapView.SetData))]
        [AffinityPostfix]
        private void SetData(RecentBeatmapView __instance, BeatmapInfoData beatmapInfoData)
        {
            BeatmapListContextMenu contextMenu = __instance.gameObject.GetComponent<BeatmapListContextMenu>();
            if (__instance.gameObject.GetComponent<BeatmapListContextMenu>() == null)
            {
                contextMenu = _instantiator.InstantiateComponent<BeatmapListContextMenu>(__instance.gameObject);
            }
            contextMenu.SetData(beatmapInfoData);
        }
    }
}
