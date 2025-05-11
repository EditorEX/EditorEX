using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Native;
using Reactive;
using UnityEngine;

namespace EditorEX.Util
{
    public static class ReactiveExtensions
    {
        public static T WithReactiveContainer<T>(this T component, ReactiveContainer container) where T : IReactiveComponent
        {
            component.WithNativeComponent<T, ReactiveContainerHolder>(out var comp);
            comp.ReactiveContainer = container;
            return component;
        }

        public static T EnabledWithObservable<T, R>(this T component, ObservableValue<R> observable, R value) where T : ReactiveComponent
        {
            component.Enabled = observable.Value?.Equals(value) ?? true;
            component.Animate(observable, () => {
                component.Enabled = observable.Value?.Equals(value) ?? true;
            });
            return component;
        }
    }
}