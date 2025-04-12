using System;
using System.Reflection;
using Vivify.ObjectPrefab.Collections;

namespace EditorEX.Essentials.Visuals.Note;

internal class EditorVivifyNotePrefabManager : IDisposable
{
    internal readonly EventInfo Changed = typeof(PrefabDictionary).GetEvent("Changed", BindingFlags.Instance)!;
    internal PrefabDictionary AnyDirectionNotePrefabs { get; } = new();

    internal PrefabDictionary BombNotePrefabs { get; } = new();

    internal PrefabDictionary BurstSliderElementPrefabs { get; } = new();

    internal PrefabDictionary BurstSliderPrefabs { get; } = new();

    internal PrefabDictionary ColorNotePrefabs { get; } = new();

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}