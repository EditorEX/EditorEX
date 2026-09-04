using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using EditorEX.MapData.Bookmarks;
using SiraUtil.Affinity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.UI.Patches
{
    internal class CreateBookmarkDialogPatches : IAffinity
    {
        private readonly CustomBookmarksController _customBookmarksController;
        private readonly BookmarksDataModel _bookmarksDataModel;

        private CreateBookmarkDialogPatches(
            CustomBookmarksController customBookmarksController,
            BookmarksDataModel bookmarksDataModel
        )
        {
            _customBookmarksController = customBookmarksController;
            _bookmarksDataModel = bookmarksDataModel;
        }

        [AffinityPatch(
            typeof(CreateBookmarkDialogViewController),
            nameof(CreateBookmarkDialogViewController.Init),
            AffinityMethodType.Normal,
            null,
            typeof(System.Action<
                BeatmapEditorObjectId?,
                BeatmapEditorObjectId?,
                int,
                string,
                string
            >)
        )]
        [AffinityPostfix]
        private void PostfixInitCreate(CreateBookmarkDialogViewController __instance)
        {
            ConfigureDialog(__instance, null);
        }

        [AffinityPatch(
            typeof(CreateBookmarkDialogViewController),
            nameof(CreateBookmarkDialogViewController.Init),
            AffinityMethodType.Normal,
            null,
            typeof(BookmarkEditorData),
            typeof(BeatmapEditorObjectId),
            typeof(System.Action<
                BeatmapEditorObjectId?,
                BeatmapEditorObjectId?,
                int,
                string,
                string
            >)
        )]
        [AffinityPostfix]
        private void PostfixInitEdit(
            CreateBookmarkDialogViewController __instance,
            BeatmapEditorObjectId bookmarkSetId
        )
        {
            Color? existing = null;
            if (_bookmarksDataModel.bookmarkSetById.TryGetValue(bookmarkSetId, out var set))
            {
                existing = set.color;
            }

            ConfigureDialog(__instance, existing);
        }

        private void ConfigureDialog(
            CreateBookmarkDialogViewController dialog,
            Color? existingColor
        )
        {
            bool legacy = _customBookmarksController.IsLegacyFormat;
            SetTextFieldVisible(dialog, !legacy);
            Transform? swatches = dialog.transform.Find("ColorSwatches");
            if (!legacy)
            {
                if (swatches != null)
                {
                    swatches.gameObject.SetActive(false);
                }

                return;
            }

            if (existingColor.HasValue)
            {
                _customBookmarksController.PendingColor = existingColor.Value;
            }
            else
            {
                _customBookmarksController.PendingColor =
                    _customBookmarksController.NextPaletteColor(
                        _bookmarksDataModel.bookmarkSetById.Count
                    );
            }

            EnsureSwatches(dialog);
            swatches = dialog.transform.Find("ColorSwatches");
            if (swatches != null)
            {
                swatches.gameObject.SetActive(true);
                HighlightSwatches(swatches, _customBookmarksController.PendingColor);
            }
        }

        private static void SetTextFieldVisible(
            CreateBookmarkDialogViewController dialog,
            bool visible
        )
        {
            TMP_InputField textField = dialog._textInputField;
            if (textField == null)
            {
                return;
            }

            Transform textTransform = textField.transform;
            Transform labelParent = dialog._labelInputField.transform.parent;
            if (textTransform.parent != null && textTransform.parent != labelParent)
            {
                textTransform.parent.gameObject.SetActive(visible);
            }
            else
            {
                textField.gameObject.SetActive(visible);
            }
        }

        private void EnsureSwatches(CreateBookmarkDialogViewController dialog)
        {
            if (dialog.transform.Find("ColorSwatches") != null)
            {
                return;
            }

            Transform parent = dialog._labelInputField.transform.parent;
            var row = new GameObject("ColorSwatches");
            row.transform.SetParent(parent, false);
            row.transform.SetSiblingIndex(dialog._labelInputField.transform.GetSiblingIndex() + 1);

            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.padding = new RectOffset(0, 0, 4, 4);

            var rowElement = row.AddComponent<LayoutElement>();
            rowElement.minHeight = 28f;
            rowElement.preferredHeight = 28f;

            Sprite? sprite = dialog.GetComponentInChildren<Image>(true)?.sprite;
            Color[]? colors = _customBookmarksController.PaletteColors;
            if (colors == null || colors.Length == 0)
            {
                return;
            }
            for (int i = 0; i < colors.Length; i++)
            {
                Color color = colors[i];
                var swatch = new GameObject("Swatch");
                swatch.transform.SetParent(row.transform, false);
                var image = swatch.AddComponent<Image>();
                image.sprite = sprite;
                image.color = color;
                image.raycastTarget = true;
                var button = swatch.AddComponent<Button>();
                button.targetGraphic = image;
                var element = swatch.AddComponent<LayoutElement>();
                element.minWidth = 24f;
                element.minHeight = 24f;
                element.preferredWidth = 24f;
                element.preferredHeight = 24f;
                Color captured = color;
                button.onClick.AddListener(() =>
                {
                    _customBookmarksController.PendingColor = captured;
                    HighlightSwatches(row.transform, captured);
                });
            }
        }

        private static void HighlightSwatches(Transform row, Color selected)
        {
            for (int i = 0; i < row.childCount; i++)
            {
                Transform child = row.GetChild(i);
                var image = child.GetComponent<Image>();
                bool isSelected = image != null && ApproximatelyEqual(image.color, selected);
                child.localScale = isSelected ? new Vector3(1.2f, 1.2f, 1f) : Vector3.one;
            }
        }

        private static bool ApproximatelyEqual(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.01f
                && Mathf.Abs(a.g - b.g) < 0.01f
                && Mathf.Abs(a.b - b.b) < 0.01f;
        }
    }
}
