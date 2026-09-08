using System.Collections.Generic;
using Zenject;

namespace EditorEX.SDK.Integration.Patches
{
    internal static class SignalDeclarationLookup
    {
        public static bool TryGet<T>(
            Dictionary<BindingId, T> localMap,
            BindingId signalId,
            out T? value
        )
        {
            if (localMap.TryGetValue(signalId, out value))
            {
                return true;
            }

            foreach (KeyValuePair<BindingId, T> pair in localMap)
            {
                if (IsAssignableBinding(pair.Key, signalId))
                {
                    value = pair.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static bool IsAssignableBinding(BindingId left, BindingId right)
        {
            return (
                    left.Type == right.Type
                    || left.Type.IsAssignableFrom(right.Type)
                    || right.Type.IsAssignableFrom(left.Type)
                ) && Equals(left.Identifier, right.Identifier);
        }
    }
}
