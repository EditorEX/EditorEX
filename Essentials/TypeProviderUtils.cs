using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.VariableMovement;
using UnityEngine;

namespace EditorEX.Essentials
{
    public static class TypeProviderUtils
    {
        public static bool GetProvidedComponent<T>(
            this GameObject? gameObject,
            ITypeProvider typeProvider,
            T? existing,
            out T? newComponent
        )
            where T : IObjectComponent
        {
            var components = gameObject?.GetComponents<T>();
            var types = components?.Select(x => x.GetType())?.ToArray();
            if (types == null)
            {
                newComponent = existing;
                return false;
            }
            Debug.Log("x1 " + (types == null));
            var type = typeProvider.GetProvidedType(types);
            Debug.Log("x2 " + (type == null));
            Debug.Log("x3 " + type.FullName);

            T? newToUse = (T?)(object?)gameObject?.GetComponent(type);

            // We can't use != with generic values, can't use a type constraint for IEquatible as then the interface would need to inherit such also.
            if (
                newToUse != null
                && (
                    (existing != null && !EqualityComparer<T>.Default.Equals(existing, newToUse))
                    || existing == null
                )
            )
            {
                Debug.Log("setting");
                existing?.Disable();
                newComponent = newToUse;
                return true;
            }

            Debug.Log("setting2");
            newComponent = existing;
            return false;
        }

        public static bool GetProvidedVariableMovementDataProvider(
            this GameObject? gameObject,
            ITypeProvider typeProvider,
            BaseEditorData? data,
            IVariableMovementDataProvider? existing,
            out IVariableMovementDataProvider? newComponent
        )
        {
            var holder = gameObject?.GetComponent<VariableMovementHolder>();
            if (holder == null)
            {
                newComponent = existing;
                return false;
            }
            var type = typeProvider.GetProvidedType(null);

            IVariableMovementDataProvider? newToUse = null;

            switch (type)
            {
                case not null when type == typeof(VariableMovementDataProvider):
                    newToUse = holder.Original!;
                    break;
                case not null when type == typeof(EditorNoodleMovementDataProvider):
                    if (data != null)
                        newToUse = holder.Pool?.Spawn(data)!;
                    break;
            }

            // We can't use != with generic values, can't use a type constraint for IEquatible as then the interface would need to inherit such also.
            if (
                newToUse != null
                && (
                    (
                        existing != null
                        && !EqualityComparer<IVariableMovementDataProvider>.Default.Equals(
                            existing,
                            newToUse
                        )
                    )
                    || existing == null
                )
            )
            {
                if (existing is EditorNoodleMovementDataProvider noodle)
                {
                    holder.Pool?.Despawn(noodle);
                }
                newComponent = newToUse;
                return true;
            }

            newComponent = existing;
            return false;
        }
    }
}
