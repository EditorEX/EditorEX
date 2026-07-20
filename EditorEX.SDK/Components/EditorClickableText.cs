using System;
using System.Collections.Generic;
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
        public static List<EditorClickableText> ClickableTexts { get; } = new();

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

        private void SetState(SelectionState newState)
        {
            State = newState;
            selectionStateDidChangeEvent.Invoke(State);
        }

        public override void OnEnable()
        {
            if (!ClickableTexts.Contains(this))
            {
                ClickableTexts.Add(this);
            }

            base.OnEnable();
        }

        public override void OnDisable()
        {
            if (ClickableTexts.Contains(this))
            {
                ClickableTexts.Remove(this);
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
