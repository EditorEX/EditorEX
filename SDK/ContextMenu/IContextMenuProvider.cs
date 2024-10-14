using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.SDK.ContextMenu
{
    public interface IContextMenuProvider
    {
        public IContextOption[] GetIContextOptions();
        public Type GetContextType();
    }
}
