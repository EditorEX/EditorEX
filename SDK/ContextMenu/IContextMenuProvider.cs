using System;

namespace EditorEX.SDK.ContextMenu
{
    public interface IContextMenuProvider
    {
        public IContextOption[] GetIContextOptions();
        public Type GetContextType();
    }
}
