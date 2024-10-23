using UnityEngine;

namespace EditorEX.SDK.Base
{
    public struct LayoutData
    {
        public LayoutData(Vector2? sizeDelta, Vector2? anchoredPosition)
        {
            this.sizeDelta = sizeDelta;
            this.anchoredPosition = anchoredPosition;
        }

        public Vector2? sizeDelta { get; set; }
        public Vector2? anchoredPosition { get; set; }
    }
}
