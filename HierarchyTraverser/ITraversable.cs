using UnityEngine;

namespace EditorEX.HierarchyTraverser
{
    public interface ITraversable
    {
        R GetComponent<R>() where R : Component;
        GameObject Get();
        Transform GetTransform();
        RectTransform GetRectTransform();
    }
}