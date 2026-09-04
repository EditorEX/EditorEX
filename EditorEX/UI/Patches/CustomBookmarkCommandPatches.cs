using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.LevelEditor;
using EditorEX.MapData.Bookmarks;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace EditorEX.UI.Patches
{
    internal class CustomBookmarkCommandPatches : IAffinity
    {
        private readonly CustomBookmarksController _customBookmarksController;

        private CustomBookmarkCommandPatches(CustomBookmarksController customBookmarksController)
        {
            _customBookmarksController = customBookmarksController;
        }

        [AffinityPatch(
            typeof(BookmarkCommands.CreateBookmarkCommand),
            nameof(BookmarkCommands.CreateBookmarkCommand.Execute)
        )]
        [AffinityPrefix]
        private bool PrefixCreateExecute(BookmarkCommands.CreateBookmarkCommand __instance)
        {
            if (!_customBookmarksController.IsLegacyFormat)
            {
                return true;
            }

            string label = __instance._signal.label ?? "";
            __instance._bookmarkData = BookmarkEditorData.CreateNew(
                __instance._beatmapState.beat,
                label,
                label
            );
            __instance.AddBookmarkDataToModel();
            return false;
        }

        [AffinityPatch(typeof(BookmarkCommands.CreateBookmarkCommand), "AddBookmarkDataToModel")]
        [AffinityPrefix]
        private bool PrefixAddBookmarkDataToModel(BookmarkCommands.CreateBookmarkCommand __instance)
        {
            if (!_customBookmarksController.IsLegacyFormat)
            {
                return true;
            }

            string label = __instance._bookmarkData.label ?? "";
            if (string.IsNullOrEmpty(label))
            {
                label = "Bookmark";
            }

            BookmarkSetEditorData set = BookmarkSetEditorData.CreateNew(
                label,
                _customBookmarksController.PendingColor,
                null,
                __instance._beatmapDataModel.beatmapDifficulty,
                __instance._beatmapDataModel.beatmapCharacteristic
            );
            __instance._bookmarksDataModel.AddBookmarkSet(set);
            __instance._createdSet = true;
            __instance._bookmarksDataModel.SelectBookmarkSet(set.id);
            __instance._bookmarksDataModel.SetBookmarkSetEnabled(set.id, enabled: true);
            __instance._bookmarkSetId = set.id;
            __instance._bookmarksDataModel.AddBookmark(set.id, __instance._bookmarkData);
            __instance._signalBus.Fire(new BookmarkSetCommands.SelectedBookmarkSetChangedSignal());
            __instance._signalBus.Fire(new BookmarkCommands.BookmarksChangedSignal());
            __instance._signalBus.Fire(new BookmarkSetCommands.BookmarkSetsChangedSignal());
            __instance._signalBus.Fire<BeatmapLevelUpdatedSignal>();
            return false;
        }

        [AffinityPatch(
            typeof(BookmarkCommands.UpdateBookmarkCommand),
            nameof(BookmarkCommands.UpdateBookmarkCommand.Execute)
        )]
        [AffinityPrefix]
        private bool PrefixUpdateExecute(BookmarkCommands.UpdateBookmarkCommand __instance)
        {
            if (!_customBookmarksController.IsLegacyFormat)
            {
                return true;
            }

            BookmarkEditorData original = __instance._bookmarksDataModel.bookmarkById[
                __instance._signal.bookmarkId
            ];
            string label = __instance._signal.label ?? "";
            __instance._originalBookmarkData = original;
            __instance._updatedBookmarkData = BookmarkEditorData.CopyWithModifications(
                original,
                null,
                null,
                label,
                label
            );
            BookmarkSetEditorData originalSet = __instance._bookmarksDataModel.bookmarkSetById[
                __instance._signal.bookmarkSetId
            ];
            _customBookmarksController.RememberColorEdit(
                __instance,
                originalSet,
                _customBookmarksController.PendingColor
            );
            ApplyUpdatedBookmark(
                __instance,
                __instance._updatedBookmarkData,
                _customBookmarksController.PendingColor
            );
            return false;
        }

        [AffinityPatch(
            typeof(BookmarkCommands.UpdateBookmarkCommand),
            nameof(BookmarkCommands.UpdateBookmarkCommand.Undo)
        )]
        [AffinityPrefix]
        private bool PrefixUpdateUndo(BookmarkCommands.UpdateBookmarkCommand __instance)
        {
            if (
                !_customBookmarksController.IsLegacyFormat
                || !_customBookmarksController.TryGetColorEdit(__instance, out var state)
            )
            {
                return true;
            }

            ApplyUpdatedBookmark(
                __instance,
                __instance._originalBookmarkData,
                state.OriginalSet.color
            );
            return false;
        }

        [AffinityPatch(
            typeof(BookmarkCommands.UpdateBookmarkCommand),
            nameof(BookmarkCommands.UpdateBookmarkCommand.Redo)
        )]
        [AffinityPrefix]
        private bool PrefixUpdateRedo(BookmarkCommands.UpdateBookmarkCommand __instance)
        {
            if (
                !_customBookmarksController.IsLegacyFormat
                || !_customBookmarksController.TryGetColorEdit(__instance, out var state)
            )
            {
                return true;
            }

            ApplyUpdatedBookmark(__instance, __instance._updatedBookmarkData, state.NewColor);
            return false;
        }

        [AffinityPatch(
            typeof(BookmarkCommands.DeleteBookmarkCommand),
            nameof(BookmarkCommands.DeleteBookmarkCommand.Execute)
        )]
        [AffinityPrefix]
        private bool PrefixDeleteExecute(BookmarkCommands.DeleteBookmarkCommand __instance)
        {
            if (!_customBookmarksController.IsLegacyFormat)
            {
                return true;
            }

            __instance._originalBookmarkData = __instance._bookmarksDataModel.bookmarkById[
                __instance._signal.bookmarkId
            ];
            if (
                __instance._bookmarksDataModel.bookmarkSetById.TryGetValue(
                    __instance._signal.bookmarkSetId,
                    out var set
                )
            )
            {
                _customBookmarksController.RememberDeletedSet(__instance, set);
            }

            DeleteLegacyBookmark(__instance);
            return false;
        }

        [AffinityPatch(
            typeof(BookmarkCommands.DeleteBookmarkCommand),
            nameof(BookmarkCommands.DeleteBookmarkCommand.Undo)
        )]
        [AffinityPrefix]
        private bool PrefixDeleteUndo(BookmarkCommands.DeleteBookmarkCommand __instance)
        {
            if (
                !_customBookmarksController.IsLegacyFormat
                || !_customBookmarksController.TryGetDeletedSet(__instance, out var set)
            )
            {
                return true;
            }

            __instance._bookmarksDataModel.AddBookmarkSet(set);
            __instance._bookmarksDataModel.SetBookmarkSetEnabled(set.id, enabled: true);
            __instance._bookmarksDataModel.AddBookmark(set.id, __instance._originalBookmarkData);
            __instance._signalBus.Fire(new BookmarkCommands.BookmarksChangedSignal());
            __instance._signalBus.Fire(new BookmarkSetCommands.BookmarkSetsChangedSignal());
            __instance._signalBus.Fire<BeatmapLevelUpdatedSignal>();
            return false;
        }

        [AffinityPatch(
            typeof(BookmarkCommands.DeleteBookmarkCommand),
            nameof(BookmarkCommands.DeleteBookmarkCommand.Redo)
        )]
        [AffinityPrefix]
        private bool PrefixDeleteRedo(BookmarkCommands.DeleteBookmarkCommand __instance)
        {
            if (!_customBookmarksController.IsLegacyFormat)
            {
                return true;
            }

            DeleteLegacyBookmark(__instance);
            return false;
        }

        private static void ApplyUpdatedBookmark(
            BookmarkCommands.UpdateBookmarkCommand command,
            BookmarkEditorData bookmarkData,
            Color color
        )
        {
            command._bookmarksDataModel.UpdateBookmark(command._signal.bookmarkSetId, bookmarkData);
            if (
                command._bookmarksDataModel.bookmarkSetById.TryGetValue(
                    command._signal.bookmarkSetId,
                    out var set
                )
            )
            {
                command._bookmarksDataModel.UpdateBookmarkSet(
                    BookmarkSetEditorData.CopyWithModifications(set, set.id, set.name, color)
                );
            }

            command._signalBus.Fire(new BookmarkCommands.BookmarksChangedSignal());
            command._signalBus.Fire(new BookmarkSetCommands.BookmarkSetsChangedSignal());
            command._signalBus.Fire<BeatmapLevelUpdatedSignal>();
        }

        private static void DeleteLegacyBookmark(BookmarkCommands.DeleteBookmarkCommand command)
        {
            command._bookmarksDataModel.DeleteBookmark(
                command._signal.bookmarkId,
                command._signal.bookmarkSetId
            );
            command._bookmarksDataModel.DeleteBookmarkSet(command._signal.bookmarkSetId);
            command._signalBus.Fire(new BookmarkCommands.BookmarksChangedSignal());
            command._signalBus.Fire(new BookmarkSetCommands.BookmarkSetsChangedSignal());
            command._signalBus.Fire<BeatmapLevelUpdatedSignal>();
        }
    }
}
