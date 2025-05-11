using EditorEX.SDK.Reactive;
using EditorEX.SDK.Reactive.Components.Native;
using Reactive;
using UnityEngine;

namespace EditorEX.Util
{
    public static class ReactiveExtensions
    {
        public static T WithReactiveContainer<T>(this T component, ReactiveContainer container) where T : IReactiveComponent
        {
            Debug.Log("hi");
            component.WithNativeComponent<T, ReactiveContainerHolder>(out var comp);
            comp.ReactiveContainer = container;
            return component;
        }
    }
}