using System;
using System.Linq;

namespace EditorEX.SDK.ContextMenu
{
    public abstract class ContextMenuProvider<T> : IContextMenuProvider where T : IContextMenuObject
    {
        public abstract ContextOption<T>[] GetContextOptions();

        public IContextOption[] GetIContextOptions()
        {
            return GetContextOptions().Cast<IContextOption>().ToArray();
        }

        public Type GetContextType()
        {
            return typeof(T);
        }
    }
}
