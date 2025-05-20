using EditorEX.SDK.ReactiveComponents.Table;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents.Dropdown
{
    public partial class EditorDropdown<TKey, TParam, TCell> {
        private class EditorDropdownCellWrapper : TableCell<DropdownOption> {
            private TCell _cell = default!;

            protected override void OnInit(DropdownOption item) {
                _cell.Init(item.key, item.param);
            }

            protected override void OnCellStateChange(bool selected) {
                _cell.OnCellStateChange(selected);
            }

            protected override GameObject Construct() {
                return new TCell().Bind(ref _cell).Use(null);
            }

            protected override void OnInitialize() {
                this.WithSizeDelta(0f, 40f);
                _cell.CellAskedToBeSelectedEvent += HandleCellAskedToBeSelected;
            }

            private void HandleCellAskedToBeSelected(TKey key) {
                SelectSelf(true);
            }
        }

        private class EditorDropdownOptionsModal : ModalBase {
            private const int MaxDisplayedItems = 5;
            private const float ItemSize = 40f;

            private RectTransform _buttonTransform = null!;

            public void ApplyLayout(RectTransform buttonRect) {
                _buttonTransform = buttonRect;
                
                var width = buttonRect.rect.width;
                var height = Mathf.Clamp(Table.Items.Count, 1, MaxDisplayedItems) * ItemSize + 2;

                this.AsFlexItem(size: new() { x = width, y = height });
            }

            protected override void OnOpen(bool opened) {
                if (!opened) {
                    this.WithAnchor(_buttonTransform, RelativePlacement.BottomCenter);
                }
            }

            public EditorTable<DropdownOption, EditorDropdownCellWrapper> Table => _table;

            private EditorTable<DropdownOption, EditorDropdownCellWrapper> _table = null!;

            protected override GameObject Construct() {
                return new Layout {
                    Children = {
                        new EditorBackground() {
                                Source = "#Background4px",
                                Color = new Color(0.17f, 0.18f, 0.2f, 1f),
                                ImageType = UnityEngine.UI.Image.Type.Sliced,
                                Children = {
                                    new EditorTable<DropdownOption, EditorDropdownCellWrapper>()
                                        .WithListener(
                                            x => x.SelectedIndexes,
                                            _ => CloseInternal()
                                        )
                                        .AsFlexItem(flexGrow: 1f)
                                        .Bind(ref _table)
                                }
                            }
                            .AsFlexGroup(padding: new() { top = 1f, bottom = 1f })
                            .AsFlexItem(flexGrow: 1f),

                        // Scrollbar
                        /*new Scrollbar()
                            .AsFlexItem(
                                size: new() { x = 2f, y = 100.pct() },
                                position: new() { right = -4f }
                            )
                            .With(x => Table.Scrollbar = x)*/
                    }
                }.AsFlexGroup(gap: 2f, constrainHorizontal: false, constrainVertical: false).Use();
            }
        }

        private class SharedDropdownOptionsModal : SharedModal<EditorDropdownOptionsModal> {
            protected override void OnSpawn() {
                Modal.WithJumpAnimation();
            }
        }
    }
}