using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EditorEX.UI.Components
{
    internal class DoubleClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public Action? OnDoubleClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
                OnDoubleClick?.Invoke();
        }
    }
}
