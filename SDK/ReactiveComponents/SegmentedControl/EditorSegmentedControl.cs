using EditorEX.SDK.ReactiveComponents.SegmentedControl;
using Reactive;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents
{
    public class EditorSegmentedControl : ReactiveComponent
    {
        protected override void Construct(RectTransform rect)
        {
            new Layout() {
                Children = {
                    new EditorSegmentedControlButton(0) {
                        Text = "Left",
                    },
                    new EditorSegmentedControlButton(1) {
                        Text = "Middle",
                    },
                    new EditorSegmentedControlButton(2) {
                        Text = "Right",
                    },
                }
            }.AsFlexGroup(Reactive.Yoga.FlexDirection.Row, gap: 100f)
            .Use(rect);
        }
    }
}