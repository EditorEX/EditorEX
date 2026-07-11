using System;
using EditorEX.Util;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace EditorEX.SDK.ReactiveComponents.Dropdown
{
    public class EditorTextDropdown<TKey>
        : EditorDropdown<TKey, string, EditorTextDropdown<TKey>.ComponentCell>
    {
        public class ComponentCell
            : ReactiveComponent,
                IKeyedControlCell<TKey, string>,
                IPreviewableCell
        {
            public bool UsedAsPreview
            {
                set
                {
                    _isPreview = value;
                    ApplyPreviewMode();
                }
            }

            public event Action<TKey>? CellAskedToBeSelectedEvent;

            public void Init(TKey key, string text)
            {
                _key = key;
                _label.Text = text;
            }

            public void OnCellStateChange(bool selected)
            {
                _selected = selected;
                ApplyCellState();
            }

            private bool _isPreview;
            private bool _selected;
            private TKey? _key;
            private EditorLabel _label = null!;
            private EditorBackground _background = null!;

            protected override GameObject Construct()
            {
                return new LayoutChildren
                {
                    new EditorLabel() { Alignment = TextAlignmentOptions.Left, FontSize = 18f }
                        .AsFlexItem(flexGrow: 1f)
                        .Bind(ref _label),
                }
                    .As<EditorBackground>(x =>
                    {
                        x.Source = "#Background4px";
                        x.ImageType = Image.Type.Sliced;
                        x.Color = Color.clear;
                        x.RaycastTarget = true;
                    })
                    .AsFlexGroup(alignItems: Reactive.Yoga.Align.Center, padding: 8f)
                    .AsFlexItem(size: new() { x = "auto", y = 40f })
                    .With(x => x.WrappedImage.WithPointerEvents(onDown: _ => SelectSelf()))
                    .Bind(ref _background)
                    .Use();
            }

            protected override void OnInitialize()
            {
                ApplyPreviewMode();
            }

            private void ApplyPreviewMode()
            {
                if (_background == null)
                {
                    return;
                }

                _background.WrappedImage.DisablePointerEvents = _isPreview;
                _background.WrappedImage.ImageView.enabled = !_isPreview;
                _label.RaycastTarget = false;
                ApplyCellState();
            }

            private void ApplyCellState()
            {
                if (_background == null)
                {
                    return;
                }

                _background.Color = _selected ? new Color(0.2f, 0.28f, 0.32f, 1f) : Color.clear;
            }

            private void SelectSelf()
            {
                if (_key != null)
                {
                    CellAskedToBeSelectedEvent?.Invoke(_key);
                }
            }
        }
    }
}
