using BeatmapEditor3D;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Native;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
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

        public static EditorNamedRail InEditorNamedRail(this ILayoutItem comp, string text, float fontSize = 12f)
        {
            ILayoutItem comp2 = comp;
            EditorNamedRail namedRail = new EditorNamedRail();
            namedRail.Label.Text = text;
            namedRail.Label.FontSize = fontSize;
            namedRail.Component = comp2;
            return namedRail.With(delegate (EditorNamedRail x)
            {
                if (comp2 is ISkewedComponent skewedComponent)
                {
                    ((ISkewedComponent)x.Label).Skew = skewedComponent.Skew;
                }
            });
        }

        public static void PresentEditor(this IModal comp, Transform child, bool animated = true)
        {
            Transform transform = child.GetComponentInParent<ReactiveContainerHolder>().transform;
            ModalSystem<Reactive.Components.Basic.ModalSystem>.PresentModal(comp, transform, animated);
        }
    }
}