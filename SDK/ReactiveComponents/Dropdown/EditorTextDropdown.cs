using System;
using EditorEX.Util;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using TMPro;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents.Dropdown
{
    public class EditorTextDropdown<TKey>
        : EditorDropdown<TKey, string, EditorTextDropdown<TKey>.ComponentCell>
    {
        public class ComponentCell : KeyedControlCell<TKey, string>, IPreviewableCell
        {
            public bool UsedAsPreview
            {
                set
                {
                    _button.RaycastTarget = !value;
                    _button.Visible = !value;
                    _label.RaycastTarget = !value;
                }
            }

            public override void OnInit(TKey item, string text)
            {
                _label.Text = text;
            }

            private EditorLabel _label = null!;
            private EditorBackgroundButton _button = null!;

            protected override GameObject Construct()
            {
                return new LayoutChildren
                {
                    new Layout().AsFlexItem(flexGrow: 1f),

                    new EditorLabel() { Alignment = TextAlignmentOptions.Left, FontSize = 18f }
                        .AsFlexItem(flexGrow: 99f)
                        .WithRectExpand()
                        .Bind(ref _label),
                }
                .As<EditorBackgroundButton>(x =>
                {
                    x.OnClick = SelectSelf;
                })
                .AsFlexGroup(alignItems: Reactive.Yoga.Align.Center, padding: 8f)
                .AsFlexItem(size: new() { x = "auto", y = 40f })
                .Bind(ref _button)
                .Use();
            }

            public override void OnCellStateChange(bool selected) { }
        }
    }
}
