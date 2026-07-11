using System;
using EditorEX.SDK.ReactiveComponents.Table;
using EditorEX.Util;
using Reactive;
using Reactive.Components;
using Reactive.Components.Basic;
using Reactive.Yoga;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents.Dropdown
{
    public partial class EditorDropdown<TKey, TParam, TCell>
    {
        private class EditorDropdownOptionsModal : ModalBase
        {
            private const int MaxDisplayedItems = 5;
            private const float ItemSize = 40f;

            private RectTransform _buttonTransform = null!;

            public ScrollContext ScrollContext { get; } = new();

            public Table<DropdownOption, IReactiveComponent> Table => _table;

            private Table<DropdownOption, IReactiveComponent> _table = null!;

            public void ApplyLayout(RectTransform buttonRect)
            {
                _buttonTransform = buttonRect;

                var width = buttonRect.rect.width;
                var displayed = Mathf.Clamp(Table.Items.Count, 1, MaxDisplayedItems);

                // The Table (its scroll viewport) needs a *concrete* height, otherwise
                // ViewSize resolves to 0 at layout time and no cells are ever spawned
                // until the user scrolls. This mirrors BsDropdown's fixed table size.
                _table.AsFlexItem(
                    flexGrow: 1f,
                    size: new() { x = "auto", y = displayed * ItemSize }
                );
                this.AsFlexItem(size: new() { x = width, y = "auto" });
            }

            protected override void OnOpen(bool opened)
            {
                if (!opened)
                {
                    this.WithAnchor(_buttonTransform, RelativePlacement.BottomCenter);
                }
            }

            private IReactiveComponent CreateCell(CellContext<DropdownOption> context)
            {
                var cell = new TCell();

                cell.CellAskedToBeSelectedEvent += _ => context.Selected = true;

                void UpdateCell(CellContext<DropdownOption> ctx)
                {
                    cell.Init(ctx.Item.key, ctx.Item.param);
                    cell.OnCellStateChange(ctx.Selected);
                }

                context.ValueChangedEvent += UpdateCell;
                UpdateCell(context);

                return cell;
            }

            protected override GameObject Construct()
            {
                return new LayoutChildren
                {
                    new Table<DropdownOption, IReactiveComponent>
                    {
                        ScrollContext = ScrollContext,
                        Items = Array.Empty<DropdownOption>(),
                        ConstructCell = CreateCell,
                    }
                        .AsFlexItem(flexGrow: 1f)
                        .Bind(ref _table),
                    // Scrollbar
                    new EditorScrollbar { ScrollContext = ScrollContext }.AsFlexItem(
                        size: new() { x = 6f, y = "auto" }
                    ),
                }
                    .As<EditorBackground>(x =>
                    {
                        x.Source = "#Background4px";
                        x.ImageType = UnityEngine.UI.Image.Type.Sliced;
                        x.Color = new Color(0.11f, 0.13f, 0.14f, 1f);
                    })
                    .AsFlexGroup(
                        direction: FlexDirection.Row,
                        alignItems: Align.Stretch,
                        gap: 1f,
                        padding: 2f
                    )
                    .Use();
            }
        }
    }
}
