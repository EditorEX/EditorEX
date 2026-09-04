using BeatmapEditor3D.Views;
using EditorEX.MapData.Bookmarks;
using SiraUtil.Affinity;

namespace EditorEX.UI.Patches
{
    internal class BookmarkManagementViewPatches : IAffinity
    {
        private readonly CustomBookmarksController _customBookmarksController;

        private BookmarkManagementViewPatches(CustomBookmarksController customBookmarksController)
        {
            _customBookmarksController = customBookmarksController;
        }

        [AffinityPatch(typeof(BookmarkManagementView), nameof(BookmarkManagementView.DidActivate))]
        [AffinityPostfix]
        private void PostfixDidActivate(BookmarkManagementView __instance)
        {
            bool legacy = _customBookmarksController.IsLegacyFormat;
            __instance._showBookmarkSetsButton.gameObject.SetActive(!legacy);
            __instance._showBookmarksButton.gameObject.SetActive(!legacy);
            __instance._addBookmarkSetButton.gameObject.SetActive(!legacy);
            if (!legacy)
            {
                return;
            }

            __instance._bookmarksView.gameObject.SetActive(true);
            __instance._bookmarkSetsView.gameObject.SetActive(false);
        }
    }
}
