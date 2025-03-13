using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
