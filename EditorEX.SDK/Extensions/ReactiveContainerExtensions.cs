using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Native;
using Reactive;
using UnityEngine;

namespace EditorEX.SDK.Extensions
{
    public static class ReactiveContainerExtensions
    {
        public static T WithReactiveContainer<T>(this T component, IReactiveContainer container)
            where T : IReactiveComponent
        {
            var content = component.Content;
            if (content.GetComponent<Composition>() == null)
            {
                content.AddComponent<Composition>();
            }

            var holder =
                content.GetComponent<ReactiveContainerHolder>()
                ?? content.AddComponent<ReactiveContainerHolder>();
            holder.ReactiveContainer = container;
            return component;
        }

        public static GameObject WithReactiveContainer(
            this GameObject gameObject,
            IReactiveContainer container
        )
        {
            if (gameObject.GetComponent<Composition>() == null)
            {
                gameObject.AddComponent<Composition>();
            }

            var comp =
                gameObject.GetComponent<ReactiveContainerHolder>()
                ?? gameObject.AddComponent<ReactiveContainerHolder>();
            comp.ReactiveContainer = container;
            return gameObject;
        }
    }
}
