using System;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents.Native;
using HMUI;
using Reactive;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.SDK.ReactiveComponents.Dropdown
{
    public class EditorDropdownCell : ReactiveComponent
    {
        private static readonly Color LabelColor = new(0.5568628f, 0.5882353f, 0.6039216f, 1f);

        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        public Sprite? Icon
        {
            get => _icon.Sprite;
            set
            {
                _icon.Sprite = value;
                _icon.Enabled = value != null;
            }
        }

        public Action? OnClick
        {
            get => _onClick;
            set
            {
                _onClick = value;
                if (_cell != null)
                {
                    _cell.Clicked = value;
                }
            }
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                ApplySelected();
            }
        }

        private Action? _onClick;
        private bool _selected;
        private EditorLabel _label = null!;
        private EditorImage _icon = null!;
        private EditorBackground _background = null!;
        private EditorDropdownSelectableCell _cell = null!;

        protected override GameObject Construct()
        {
            return new LayoutChildren
            {
                new EditorLabel
                {
                    Alignment = TextAlignmentOptions.MidlineLeft,
                    FontSize = 18f,
                    EnableWrapping = true,
                    Overflow = TextOverflowModes.Ellipsis,
                    Color = LabelColor,
                    RaycastTarget = false,
                }
                    .AsFlexItem(flexGrow: 1f)
                    .Bind(ref _label),
                new EditorImage
                {
                    PreserveAspect = true,
                    RaycastTarget = false,
                    Enabled = false,
                    WithinLayoutIfDisabled = false,
                }
                    .AsFlexItem(size: 20f, aspectRatio: 1f)
                    .Bind(ref _icon),
            }
                .As<LayoutChildren, EditorBackground>(x =>
                {
                    x.Source = "#WhitePixel";
                    x.ImageType = Image.Type.Simple;
                    x.RaycastTarget = true;
                })
                .AsFlexGroup(alignItems: Align.Center, gap: 4f, padding: new YogaFrame(0f, 10f))
                .Bind(ref _background)
                .WithNativeComponent(out _cell)
                .Use(null);
        }

        protected override void OnStart()
        {
            _cell.Clicked = _onClick;
            ApplySelected();

            var holder = Content.transform.GetComponentInParent<ReactiveContainerHolder>();
            var container = holder?.ReactiveContainer;
            if (container == null)
            {
                ApplyLabelStyle();
                base.OnStart();
                return;
            }

            Content.SetActive(false);
            var selectableStateController =
                container.Instantiator.InstantiateComponent<SelectableCellSelectableStateController>(
                    Content
                );
            selectableStateController._component = _cell;

            var backgroundTransition =
                _background.Content.gameObject.AddComponent<ColorGraphicStateTransition>();
            backgroundTransition._transition =
                container.TransitionCollector.GetTransition<ColorTransitionSO>(
                    "SelectableCell/Background"
                );
            backgroundTransition._selectableStateController = selectableStateController;
            backgroundTransition._component = _background.WrappedImage.ImageView;
            Content.SetActive(true);

            ApplyLabelStyle();
            base.OnStart();
        }

        private void ApplyLabelStyle()
        {
            _label.FontSize = 18f;
            _label.Alignment = TextAlignmentOptions.MidlineLeft;
            _label.EnableWrapping = true;
            _label.Overflow = TextOverflowModes.Ellipsis;
            _label.Color = LabelColor;
            _label.UseScriptableObjectColors = false;
            _label.RaycastTarget = false;
        }

        private void ApplySelected()
        {
            if (_cell == null)
            {
                return;
            }

            _cell.SetSelected(_selected, SelectableCell.TransitionType.Instant, null, false);
        }
    }
}
