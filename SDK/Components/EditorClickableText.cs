using System;
using HMUI;
using UnityEngine.EventSystems;

namespace EditorEX.SDK.Components
{
    public class EditorClickableText
        : CurvedTextMeshPro,
            IPointerDownHandler,
            IPointerClickHandler,
            IPointerUpHandler,
            IPointerEnterHandler,
            IPointerExitHandler
    {
        public SelectionState state { get; private set; } = SelectionState.Normal;

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
            if (state == SelectionState.Highlighted)
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
            this.state = state;
            selectionStateDidChangeEvent?.Invoke(this.state);
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
