using System;
using System.Collections.Generic;
using EditorEX.SDK.ReactiveComponents;
using Reactive;
using Reactive.Components;

namespace EditorEX.SDK.Extensions
{
    public static class ReactiveLayoutExtensions
    {
        public static Layout AsLayout<T>(this T children, Action<Layout>? configure = null)
            where T : List<IReactiveComponent>
        {
            Layout layout = new();
            layout.Children.AddRange(children);
            configure?.Invoke(layout);
            return layout;
        }

        public static R As<T, R>(this T children, Action<R>? configure = null)
            where T : List<IReactiveComponent>
            where R : Layout, new()
        {
            R layout = new();
            layout.Children.AddRange(children);
            configure?.Invoke(layout);
            return layout;
        }

        public static R As<R>(this LayoutChildren children, Action<R>? configure = null)
            where R : Layout, new() => children.As<LayoutChildren, R>(configure);
    }
}
