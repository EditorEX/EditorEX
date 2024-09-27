using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.Essentials
{
    public interface ITypeProvider
    {
        public Type GetProvidedType(Type[] availableTypes, bool REDACTED);
    }
}
