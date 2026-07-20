using System;
using System.Collections.Generic;
using Reactive.BeatSaber.Components;
using UnityEngine.EventSystems;

namespace EditorEX.SDK.Components
{
    public class EditorNativeClickableImage
        : FixedImageView,
            IPointerDownHandler,
            IPointerClickHandler,
            IPointerUpHandler,
            IPointerEnterHandler,
            IPointerExitHandler
    {
        public static List<EditorNativeClickableImage> ClickableImages { get; } = new();

        public SelectionState State { get; private set; } = SelectionState.Normal;

        public event Action<PointerEventData> OnClickEvent;

        public event Action<SelectionState> selectionStateDidChangeEvent;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
            {
                OnClickEvent?.Invoke(eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SetState(SelectionState.Pressed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetState(SelectionState.Highlighted);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (State == SelectionState.Highlighted)
            {
                SetState(SelectionState.Normal);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SetState(SelectionState.Highlighted);
        }

        private void SetState(SelectionState state)
        {
            this.State = state;
            selectionStateDidChangeEvent?.Invoke(this.State);
        }

        public override void OnEnable()
        {
            if (!ClickableImages.Contains(this))
            {
                ClickableImages.Add(this);
            }
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (ClickableImages.Contains(this))
            {
                ClickableImages.Remove(this);
            }
            base.OnDisable();
        }

        public enum SelectionState
        {
            Normal,
            Highlighted,
            Pressed,
            Disabled,
        }
    }
}
