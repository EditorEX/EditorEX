using UnityEngine;

namespace EditorEX.HierarchyTraverser.Modifiers
{
    public class AnchoredPositionModifier(float? x, float? y) : IModifier
    {
        public void Apply(ITraversable node)
        {
            var rectTransform = node.GetRectTransform();
            if (rectTransform == null)
                return;
            rectTransform.anchoredPosition = new Vector2(
                x ?? rectTransform.anchoredPosition.x,
                y ?? rectTransform.anchoredPosition.y
            );
        }
    }
}
