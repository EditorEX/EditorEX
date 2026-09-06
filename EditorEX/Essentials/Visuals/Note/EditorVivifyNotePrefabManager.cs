using System;
using System.Reflection;
using Heck.Animation;
using Vivify.ObjectPrefab.Collections;

namespace EditorEX.Essentials.Visuals.Note;

internal class EditorVivifyNotePrefabManager
{
    private static readonly EventInfo Changed = typeof(PrefabDictionary).GetEvent(
        "Changed",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
    )!;

    internal PrefabDictionary AnyDirectionNotePrefabs { get; } = new();

    internal PrefabDictionary BombNotePrefabs { get; } = new();

    internal PrefabDictionary BurstSliderElementPrefabs { get; } = new();

    internal PrefabDictionary BurstSliderPrefabs { get; } = new();

    internal PrefabDictionary ColorNotePrefabs { get; } = new();

    internal void Subscribe(PrefabDictionary dictionary, Action<Track> handler)
    {
        InvokeAccessor(Changed.GetAddMethod(nonPublic: true), dictionary, handler);
    }

    internal void Unsubscribe(PrefabDictionary dictionary, Action<Track> handler)
    {
        InvokeAccessor(Changed.GetRemoveMethod(nonPublic: true), dictionary, handler);
    }

    private static void InvokeAccessor(
        MethodInfo? accessor,
        PrefabDictionary dictionary,
        Action<Track> handler
    )
    {
        if (accessor == null)
        {
            throw new InvalidOperationException("PrefabDictionary.Changed has no accessor.");
        }

        accessor.Invoke(dictionary, new object[] { handler });
    }
}
