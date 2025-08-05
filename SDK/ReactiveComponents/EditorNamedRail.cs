using EditorEX.Util;
using Reactive;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents
{
    public class EditorNamedRail : ReactiveComponent
    {
        public EditorLabel Label => _label;

        public ILayoutItem? Component
        {
            get => _component;
            set
            {
                if (_component != null)
                {
                    _container.Children.Remove(_component);
                }
                _component = value;
                if (_component != null)
                {
                    _component.AsFlexItem();
                    _container.Children.Add(_component);
                    if (_component.LayoutModifier is YogaModifier yogaModifier)
                    {
                        yogaModifier.Size = new YogaVector(70.pct(), "auto");
                    }
                }
            }
        }

        public float Ratio
        {
            set
            {
                if (_component?.LayoutModifier is YogaModifier compYogaModifier)
                {
                    compYogaModifier.Size = new YogaVector(value.pct(), "auto");
                }
                if (_labelContainer?.LayoutModifier is YogaModifier labelYogaModifier)
                {
                    labelYogaModifier.Size = new YogaVector((100f - value).pct(), "auto");
                }
            }
        }

        public GameObject ModifiedHint => _modifiedHint.Content;

        private ILayoutItem? _component;
        private EditorLabel _label = null!;
        private EditorLabel _modifiedHint = null!;
        private Layout _container = null!;
        private Layout _labelContainer = null!;

        protected override GameObject Construct()
        {
            return new LayoutChildren
            {
                new LayoutChildren
                {
                    new EditorLabel
                    {
                        Text = "Oops, text is missing",
                        Alignment = TextAlignmentOptions.Left,
                    }
                    .AsFlexItem(alignSelf: Align.Center)
                    .Bind(ref _label),

                    new EditorLabel { Text = "*", Enabled = false }
                        .Export(out _modifiedHint)
                        .AsFlexItem(),
                }
                .AsLayout()
                .AsFlexGroup()
                .AsFlexItem()
                .Export(out _labelContainer)
            }
            .AsLayout()
            .AsFlexGroup(
                justifyContent: Justify.SpaceBetween,
                alignItems: Align.Center,
                gap: 1f
            )
            .Bind(ref _container)
            .Use();
        }

        protected override void OnInitialize()
        {
            this.AsFlexItem();
        }
    }
}
