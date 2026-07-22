using UnityEngine;

namespace EditorEX.SDK.ContextMenu
{
    public interface IContextMenuService
    {
        void ShowContextMenu<T>(T data, Vector2 position, object? linkedObject)
            where T : IContextMenuObject;

        void TryInvalidate(object instance);
    }
}
