using System;

namespace EditorEX.Essentials
{
    public interface ITypeProvider
    {
        public Type GetProvidedType(Type[]? availableTypes);
    }
}
