using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EditorEX.Essentials
{
    public static class TypeProviderUtils
    {
        public static bool GetProvidedComponent<T>(this GameObject gameObject, ITypeProvider typeProvider, T existing, out T newComponent) where T : IObjectComponent
        {
            var components = gameObject?.GetComponents<T>();
            var types = components?.Select(x => x.GetType())?.ToArray();
            if (types == null)
            {
                newComponent = existing;
                return false;
            }
            var type = typeProvider.GetProvidedType(types, false);

            T newToUse = (T)(object)gameObject.GetComponent(type);

            // We can't use != with generic values, can't use a type constraint for IEquatible as then the interface would need to inherit such also.
            if (!EqualityComparer<T>.Default.Equals(existing, newToUse))
            {
                existing?.Disable();
                newComponent = newToUse;
                return true;
            }

            newComponent = existing;
            return false;
        }
    }
}
