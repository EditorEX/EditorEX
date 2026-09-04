using System;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Visuals;
using HMUI;
using SiraUtil.Affinity;
using TMPro;
using UnityEngine;

namespace EditorEX.UI.Patches
{
    /// <summary>
    /// Wraps long bookmark titles in the right-panel list and grows each row to fit, instead of
    /// overflowing the vanilla 40px <see cref="TableView"/> cell. Swaps the bookmarks table to
    /// <see cref="TableViewWithVariableSizedCells"/> so per-index <see cref="BookmarksView.CellSize"/>
    /// is honored, insets <c>NameLabel</c> so wrapped lines do not run under the beat number, and
    /// caps height at six lines with ellipsis.
    /// </summary>
    internal class BookmarkListWrapPatches : IAffinity
    {
        private const float MinCellHeight = 40f;
        private const float MaxLineCount = 6f;
        private const float EditDeleteColumnWidth = 85f;
        private const float NameLabelLeftInset = 10f;
        private const float NameLabelRightInset = 56f;
        private const float FallbackViewportWidth = 453f;
        private const float MultiLineTopMargin = 4f;

        private TMP_Text? _measurer;

        [AffinityPatch(typeof(BookmarksView), nameof(BookmarksView.DidActivate))]
        [AffinityPrefix]
        private void PrefixDidActivate(BookmarksView __instance)
        {
            EnsureVariableHeightTable(__instance);
        }

        [AffinityPatch(typeof(BookmarksView), nameof(BookmarksView.CellSize))]
        [AffinityPrefix]
        private bool PrefixCellSize(BookmarksView __instance, int idx, ref float __result)
        {
            if (idx < 0)
            {
                __result = MinCellHeight;
                return false;
            }

            var collection = __instance._bookmarkDataCollection;
            if (
                collection == null
                || idx >= collection.Count
                || !__instance._bookmarksModel.bookmarkById.TryGetValue(
                    collection[idx].bookmarkId,
                    out var bookmark
                )
            )
            {
                __result = MinCellHeight;
                return false;
            }

            __result = MeasureCellHeight(bookmark.label, __instance._tableView);
            return false;
        }

        [AffinityPatch(typeof(BookmarkTableCell), nameof(BookmarkTableCell.SetData))]
        [AffinityPostfix]
        private void PostfixSetData(BookmarkTableCell __instance, BookmarkEditorData data)
        {
            var nameLabel = __instance._bookmarkNameLabel;
            if (nameLabel == null)
            {
                return;
            }

            EnsureMeasurer(nameLabel);
            ApplyNameColumnLayout(nameLabel, data.label, __instance.tableCellOwner as TableView);
        }

        private void EnsureVariableHeightTable(BookmarksView view)
        {
            var old = view._tableView;
            if (old == null || old is TableViewWithVariableSizedCells)
            {
                return;
            }

            var content = old.contentTransform;
            CaptureMeasurerFromContent(content);

            var snapshot = TableViewSnapshot.Capture(old);
            var go = old.gameObject;
            go.SetActive(false);
            UnityEngine.Object.DestroyImmediate(old);

            var replacement = go.AddComponent<TableViewWithVariableSizedCells>();
            snapshot.Apply(replacement);
            replacement._preallocatedCells = Array.Empty<TableView.CellsGroup>();
            view._tableView = replacement;

            DestroyBookmarkCells(content);
            go.SetActive(true);
        }

        private void CaptureMeasurerFromContent(RectTransform? content)
        {
            if (_measurer != null || content == null)
            {
                return;
            }

            var template = content.GetComponentInChildren<BookmarkTableCell>(true);
            if (template != null && template._bookmarkNameLabel != null)
            {
                EnsureMeasurer(template._bookmarkNameLabel);
            }
        }

        private static void DestroyBookmarkCells(RectTransform? content)
        {
            if (content == null)
            {
                return;
            }

            var cells = content.GetComponentsInChildren<BookmarkTableCell>(true);
            for (int i = 0; i < cells.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(cells[i].gameObject);
            }
        }

        private void EnsureMeasurer(TMP_Text template)
        {
            if (_measurer != null)
            {
                return;
            }

            var go = new GameObject("EditorEXBookmarkLabelMeasurer");
            go.hideFlags = HideFlags.HideAndDontSave;
            go.SetActive(false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = template.font;
            tmp.fontSharedMaterial = template.fontSharedMaterial;
            tmp.fontSize = template.fontSize;
            tmp.characterSpacing = template.characterSpacing;
            tmp.wordSpacing = template.wordSpacing;
            tmp.lineSpacing = template.lineSpacing;
            tmp.extraPadding = template.extraPadding;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.richText = template.richText;
            tmp.overflowMode = TextOverflowModes.Overflow;
            _measurer = tmp;
        }

        private float MeasureCellHeight(string? label, TableView table)
        {
            if (_measurer == null || string.IsNullOrEmpty(label))
            {
                return MinCellHeight;
            }

            float wrapWidth = ResolveWrapWidth(table);
            float singleLine = _measurer.GetPreferredValues("A", wrapWidth, 10000f).y;
            if (singleLine <= 0f)
            {
                return MinCellHeight;
            }

            float padding = Mathf.Max(0f, MinCellHeight - singleLine);
            float preferred = _measurer.GetPreferredValues(label, wrapWidth, 10000f).y;
            float maxHeight = (singleLine * MaxLineCount) + padding;
            return Mathf.Clamp(preferred + padding, MinCellHeight, maxHeight);
        }

        private static float ResolveWrapWidth(TableView? table)
        {
            float viewportWidth = FallbackViewportWidth;
            if (table != null && table.viewportTransform != null)
            {
                float width = table.viewportTransform.rect.width;
                if (width > 1f)
                {
                    viewportWidth = width;
                }
            }

            return Mathf.Max(
                1f,
                viewportWidth - EditDeleteColumnWidth - NameLabelLeftInset - NameLabelRightInset
            );
        }

        private void ApplyNameColumnLayout(TMP_Text nameLabel, string? label, TableView? table)
        {
            nameLabel.textWrappingMode = TextWrappingModes.Normal;
            nameLabel.overflowMode = TextOverflowModes.Ellipsis;

            var rectTransform = nameLabel.rectTransform;
            Vector2 offsetMax = rectTransform.offsetMax;
            rectTransform.offsetMax = new Vector2(-NameLabelRightInset, offsetMax.y);

            float wrapWidth = ResolveWrapWidth(table);
            float preferredHeight = nameLabel
                .GetPreferredValues(label ?? string.Empty, wrapWidth, 10000f)
                .y;
            float singleLine = nameLabel.GetPreferredValues("A", wrapWidth, 10000f).y;
            bool multiLine = preferredHeight > singleLine + 1f;
            nameLabel.verticalAlignment = multiLine
                ? VerticalAlignmentOptions.Top
                : VerticalAlignmentOptions.Middle;
            nameLabel.margin = multiLine
                ? new Vector4(0f, MultiLineTopMargin, 0f, 0f)
                : Vector4.zero;
        }

        private readonly struct TableViewSnapshot
        {
            private readonly ScrollView _scrollView;
            private readonly bool _scrollToTopOnEnable;
            private readonly bool _alignToCenter;
            private readonly float _spacing;
            private readonly FloatRectOffset _padding;
            private readonly TableView.TableType _tableType;
            private readonly TableViewSelectionType _selectionType;
            private readonly bool _canSelectSelectedCell;
            private readonly bool _spawnCellsThatAreNotVisible;

            private TableViewSnapshot(
                ScrollView scrollView,
                bool scrollToTopOnEnable,
                bool alignToCenter,
                float spacing,
                FloatRectOffset padding,
                TableView.TableType tableType,
                TableViewSelectionType selectionType,
                bool canSelectSelectedCell,
                bool spawnCellsThatAreNotVisible
            )
            {
                _scrollView = scrollView;
                _scrollToTopOnEnable = scrollToTopOnEnable;
                _alignToCenter = alignToCenter;
                _spacing = spacing;
                _padding = padding;
                _tableType = tableType;
                _selectionType = selectionType;
                _canSelectSelectedCell = canSelectSelectedCell;
                _spawnCellsThatAreNotVisible = spawnCellsThatAreNotVisible;
            }

            public static TableViewSnapshot Capture(TableView table)
            {
                return new TableViewSnapshot(
                    table._scrollView,
                    table._scrollToTopOnEnable,
                    table._alignToCenter,
                    table._spacing,
                    table._padding,
                    table._tableType,
                    table._selectionType,
                    table._canSelectSelectedCell,
                    table._spawnCellsThatAreNotVisible
                );
            }

            public void Apply(TableView table)
            {
                table._scrollView = _scrollView;
                table._scrollToTopOnEnable = _scrollToTopOnEnable;
                table._alignToCenter = _alignToCenter;
                table._spacing = _spacing;
                table._padding = _padding;
                table._tableType = _tableType;
                table._selectionType = _selectionType;
                table._canSelectSelectedCell = _canSelectSelectedCell;
                table._spawnCellsThatAreNotVisible = _spawnCellsThatAreNotVisible;
            }
        }
    }
}
