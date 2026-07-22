using System.Linq;
using BeatmapEditor3D;
using EditorEX.Util;
using SiraUtil.Affinity;

namespace EditorEX.UI.Patches
{
    /// <summary>
    /// Applies the beatmaps list's text filter (owned by <see cref="BeatmapsListSourcesUI"/>) to
    /// the table view, and keeps the filtered index/selection consistent with the underlying
    /// (unfiltered) collection.
    /// </summary>
    internal class BeatmapsListFilterPatches : IAffinity
    {
        private readonly BeatmapsListSourcesUI _sourcesUI;

        private BeatmapsListFilterPatches(BeatmapsListSourcesUI sourcesUI)
        {
            _sourcesUI = sourcesUI;
        }

        private void ApplyFilter(BeatmapsListViewController instance)
        {
            var filteredMaps = BeatmapFilterUtil.Filter(
                instance._beatmapsCollectionDataModel._beatmapInfos,
                _sourcesUI.FilterText
            );
            instance._beatmapsListTableView.SetData(filteredMaps);
        }

        [AffinityPatch(
            typeof(BeatmapsListViewController),
            nameof(BeatmapsListViewController.DidActivate)
        )]
        [AffinityPostfix]
        private void Filter(BeatmapsListViewController __instance, bool firstActivation)
        {
            if (!firstActivation)
            {
                ApplyFilter(__instance);
            }
        }

        [AffinityPatch(
            typeof(BeatmapsListViewController),
            nameof(BeatmapsListViewController.HandleBeatmapsCollectionDataModelUpdated)
        )]
        [AffinityPrefix]
        private bool KeepFilter(BeatmapsListViewController __instance)
        {
            ApplyFilter(__instance);
            return false;
        }

        // Maintains the proper index.
        [AffinityPatch(
            typeof(BeatmapsListViewController),
            nameof(BeatmapsListViewController.HandleBeatmapListTableViewOpenBeatmapLevel)
        )]
        [AffinityPrefix]
        private void FixIndex(BeatmapsListViewController __instance, ref int idx)
        {
            var filteredMaps = __instance._beatmapsListTableView._beatmapInfos;
            idx = __instance
                ._beatmapsCollectionDataModel.beatmapInfos.ToList()
                .IndexOf(filteredMaps[idx]);
        }
    }
}
