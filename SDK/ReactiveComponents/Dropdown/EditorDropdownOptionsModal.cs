using System.Windows.Forms;
using EditorEX.SDK.ReactiveComponents.Attachable;
using EditorEX.SDK.ReactiveComponents.Table;
using Reactive;
using Reactive.Components;
using Reactive.Components.Basic;
using Reactive.Yoga;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents.Dropdown
{
    public partial class EditorDropdown<TKey, TParam, TCell>
    {
        private class EditorDropdownCellWrapper : TableCell<DropdownOption>
        {
            private TCell _cell = default!;

            protected override void OnInit(DropdownOption item)
            {
                _cell.Init(item.key, item.param);
            }

            protected override void OnCellStateChange(bool selected)
            {
                _cell.OnCellStateChange(selected);
            }

            protected override GameObject Construct()
            {
                return new TCell().Bind(ref _cell).Use(null);
            }

            protected override void OnInitialize()
            {
                this.WithSizeDelta(0f, 40f);
                _cell.CellAskedToBeSelectedEvent += HandleCellAskedToBeSelected;
            }

            private void HandleCellAskedToBeSelected(TKey key)
            {
                SelectSelf(true);
            }
        }

        private class EditorDropdownOptionsModal : ModalBase
        {
            private const int MaxDisplayedItems = 5;
            private const float ItemSize = 40f;

            private RectTransform _buttonTransform = null!;

            public void ApplyLayout(RectTransform buttonRect)
            {
                _buttonTransform = buttonRect;

                var width = buttonRect.rect.width;
                var height = Mathf.Clamp(Table.Items.Count, 1, MaxDisplayedItems) * ItemSize + 4;

                this.AsFlexItem(size: new() { x = width, y = height });
            }

            protected override void OnOpen(bool opened)
            {
                if (!opened)
                {
                    this.WithAnchor(_buttonTransform, RelativePlacement.BottomCenter);
                }
            }

            public Table<DropdownOption, EditorDropdownCellWrapper> Table => _table;

            private Table<DropdownOption, EditorDropdownCellWrapper> _table = null!;

            protected override GameObject Construct()
            {
                return new Layout
                {
                    Children =
                    {
                        new EditorBackground()
                        {
                            Source = "#Background4px",
                            ImageType = UnityEngine.UI.Image.Type.Sliced,
                            Children =
                            {
                                new Table<DropdownOption, EditorDropdownCellWrapper>()
                                {
                                    ScrollbarScrollSize = -4,
                                }
                                    .WithListener(x => x.SelectedIndexes, _ => CloseInternal())
                                    .AsFlexItem(flexGrow: 0.98f)
                                    .Bind(ref _table),
                                // Scrollbar
                                new EditorScrollbar()
                                    .AsFlexItem(
                                        size: new() { x = 7f, y = 100.pct() },
                                        position: new() { right = 2f }
                                    )
                                    .With(x => Table.Scrollbar = x),
                            },
                        }
                            .With(x =>
                                x.WrappedImage.Attach<ColorSOAttachable>("Button/Background/Normal")
                            )
                            .AsFlexGroup(padding: 2f)
                            .AsFlexItem(flexGrow: 1f),
                    },
                }
                    .AsFlexGroup(gap: 2f, constrainHorizontal: false, constrainVertical: false)
                    .Use();
            }
        }
    }
}
