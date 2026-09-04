using System.Collections.Generic;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.MapData.Bookmarks;
using EditorEX.MapData.Contexts;
using SiraUtil.Affinity;
using Zenject;

namespace EditorEX.CustomJSONData.Patches.Loading
{
    internal class CustomBookmarksLoadPatch : IAffinity
    {
        private readonly ICustomDataRepository _customDataRepository;
        private readonly CustomBookmarksController _customBookmarksController;
        private readonly SignalBus _signalBus;

        private CustomBookmarksLoadPatch(
            ICustomDataRepository customDataRepository,
            CustomBookmarksController customBookmarksController,
            SignalBus signalBus
        )
        {
            _customDataRepository = customDataRepository;
            _customBookmarksController = customBookmarksController;
            _signalBus = signalBus;
        }

        [AffinityPatch(typeof(BeatmapProjectManager), nameof(BeatmapProjectManager.LoadBeatmap))]
        [AffinityPostfix]
        private void PostfixLoadBeatmap(BeatmapProjectManager __instance, bool __result)
        {
            if (!__result || !_customBookmarksController.IsLegacyFormat)
            {
                return;
            }

            CustomData? customData =
                _customDataRepository.GetBeatmapData()?.customData
                ?? _customDataRepository.GetCustomBeatmapSaveData()?.customData;
            bool v3 = MapContext.Version.Major >= 3;
            List<CustomDataBookmark> bookmarks = CustomDataBookmarkCodec.Read(customData, v3);
            _customBookmarksController.ApplyToModel(
                __instance._bookmarksDataModel,
                __instance._beatmapDataModel,
                bookmarks
            );
            _signalBus.Fire(new BookmarkCommands.BookmarksChangedSignal());
            _signalBus.Fire(new BookmarkSetCommands.BookmarkSetsChangedSignal());
        }
    }
}
