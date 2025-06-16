using UnityEngine;

namespace EditorEX.HierarchyTraverser.Modifiers
{
    public class SizeDeltaModifier(float? width, float? height) : IModifier
    {
        public void Apply(ITraversable node)
        {
            var rectTransform = node.GetRectTransform();
            if (rectTransform == null)
                return;
            rectTransform.sizeDelta = new Vector2(
                width ?? rectTransform.sizeDelta.x,
                height ?? rectTransform.sizeDelta.y
            );
        }
    }
}
