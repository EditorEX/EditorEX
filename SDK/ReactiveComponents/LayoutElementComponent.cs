using System;
using Reactive;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.SDK.ReactiveComponents
{
    public class LayoutElementComponent : ReactiveComponent, ILeafLayoutItem
    {
        private readonly LayoutElement _layoutElement;
        public LayoutElementComponent(LayoutElement layoutElement)
        {
            _layoutElement = layoutElement;
        }

        protected override void OnStart()
        {
            _layoutElement.transform.SetParent(ContentTransform, false);
            LeafLayoutUpdatedEvent?.Invoke(this);
            ScheduleLayoutRecalculation();
        }

        public event Action<ILeafLayoutItem>? LeafLayoutUpdatedEvent;

        public Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            var measuredWidth = widthMode == MeasureMode.Undefined ? Mathf.Infinity : width;
            var measuredHeight = heightMode == MeasureMode.Undefined ? Mathf.Infinity : height;

            var rect = _layoutElement.GetComponent<RectTransform>().rect;
            var elementSize = new Vector2(rect.width, rect.height);

            return new()
            {
                x = widthMode == MeasureMode.Exactly ? width : Mathf.Min(elementSize.x, measuredWidth),
                y = heightMode == MeasureMode.Exactly ? height : Mathf.Min(elementSize.y, measuredHeight)
            };
        }
    }
}