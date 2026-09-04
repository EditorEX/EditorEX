using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.MapData.Contexts;
using UnityEngine;

namespace EditorEX.MapData.Bookmarks
{
    public class CustomBookmarksController
    {
        private readonly BookmarkColorsSO _bookmarkColorsSo;
        private readonly ConditionalWeakTable<
            BookmarkCommands.UpdateBookmarkCommand,
            ColorEditState
        > _colorEdits = new();
        private readonly ConditionalWeakTable<
            BookmarkCommands.DeleteBookmarkCommand,
            BookmarkSetEditorData
        > _deletedSets = new();

        private CustomBookmarksController(BookmarkColorsSO bookmarkColorsSo)
        {
            _bookmarkColorsSo = bookmarkColorsSo;
            PendingColor = bookmarkColorsSo.GetColorByIndex(0);
        }

        public Color PendingColor { get; set; }

        public bool IsLegacyFormat
        {
            get
            {
                Version? version = MapContext.Version;
                return version != null && version.Major < 4;
            }
        }

        public Color[] PaletteColors => _bookmarkColorsSo._allColors;

        public Color NextPaletteColor(int index)
        {
            return _bookmarkColorsSo.GetColorByIndex(index);
        }

        public void ApplyToModel(
            BookmarksDataModel bookmarksDataModel,
            BeatmapDataModel beatmapDataModel,
            IReadOnlyList<CustomDataBookmark> bookmarks
        )
        {
            var sets = new BookmarkSetEditorData[bookmarks.Count];
            var bookmarksBySetId = new Dictionary<BeatmapEditorObjectId, List<BookmarkEditorData>>(
                bookmarks.Count
            );

            for (int i = 0; i < bookmarks.Count; i++)
            {
                CustomDataBookmark item = bookmarks[i];
                string name = string.IsNullOrEmpty(item.Name) ? "Bookmark" : item.Name;
                Color color = item.HasColor ? item.Color : _bookmarkColorsSo.GetColorByIndex(i);
                BookmarkSetEditorData set = BookmarkSetEditorData.CreateNew(
                    name,
                    color,
                    null,
                    beatmapDataModel.beatmapDifficulty,
                    beatmapDataModel.beatmapCharacteristic
                );
                BookmarkEditorData bookmark = BookmarkEditorData.CreateNew(
                    item.Beat,
                    item.Name ?? "",
                    item.Name ?? ""
                );
                sets[i] = set;
                bookmarksBySetId[set.id] = new List<BookmarkEditorData> { bookmark };
            }

            bookmarksDataModel.UpdateWith(sets, bookmarksBySetId, clearData: true);
            foreach (BookmarkSetEditorData set in sets)
            {
                bookmarksDataModel.SetBookmarkSetEnabled(set.id, enabled: true);
            }

            if (sets.Length > 0)
            {
                bookmarksDataModel.SelectBookmarkSet(sets[0].id);
            }
        }

        public void RememberColorEdit(
            BookmarkCommands.UpdateBookmarkCommand command,
            BookmarkSetEditorData originalSet,
            Color newColor
        )
        {
            _colorEdits.Remove(command);
            _colorEdits.Add(command, new ColorEditState(originalSet, newColor));
        }

        public bool TryGetColorEdit(
            BookmarkCommands.UpdateBookmarkCommand command,
            out ColorEditState state
        )
        {
            return _colorEdits.TryGetValue(command, out state);
        }

        public void RememberDeletedSet(
            BookmarkCommands.DeleteBookmarkCommand command,
            BookmarkSetEditorData set
        )
        {
            _deletedSets.Remove(command);
            _deletedSets.Add(command, set);
        }

        public bool TryGetDeletedSet(
            BookmarkCommands.DeleteBookmarkCommand command,
            out BookmarkSetEditorData set
        )
        {
            return _deletedSets.TryGetValue(command, out set);
        }

        public class ColorEditState
        {
            public ColorEditState(BookmarkSetEditorData originalSet, Color newColor)
            {
                OriginalSet = originalSet;
                NewColor = newColor;
            }

            public BookmarkSetEditorData OriginalSet { get; }

            public Color NewColor { get; }
        }
    }
}
