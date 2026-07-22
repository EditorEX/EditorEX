using UnityEngine;

namespace EditorEX.SDK.Hierarchy
{
    public interface ITraversable
    {
        R GetComponent<R>()
            where R : Component;
        GameObject Get();
        Transform GetTransform();
        RectTransform GetRectTransform();
    }
}
