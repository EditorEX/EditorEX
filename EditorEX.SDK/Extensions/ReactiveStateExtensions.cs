using System;
using Reactive;
using UnityEngine;

namespace EditorEX.SDK.Extensions
{
    public static class ReactiveStateExtensions
    {
        /// <summary>
        /// Makes a component's enabled state react to an state value.
        /// The component will be enabled when the state's value equals the specified value.
        /// </summary>
        public static T EnabledWithState<T, R>(this T component, State<R> state, Func<R> value)
            where T : ReactiveComponent
        {
            component.On(
                state,
                val =>
                {
                    component.Enabled = val?.Equals(value()) ?? true;
                },
                true
            );
            return component;
        }

        /// <summary>
        /// Makes a GameObject's active state react to a state value.
        /// The GameObject will be active when the state's value equals the specified value.
        /// </summary>
        public static GameObject EnabledWithState<R>(
            this GameObject component,
            State<R> state,
            Func<R> value
        )
        {
            component.SetActive(state.Value?.Equals(value()) ?? true);
            void Closure(R val)
            {
                if (component == null)
                {
                    state.ValueChangedEvent -= Closure;
                    return;
                }
                component.SetActive(state.Value?.Equals(value()) ?? true);
            }
            state.ValueChangedEvent += Closure;
            return component;
        }

        /// <summary>
        /// Makes a component's enabled state react inversely to an state value.
        /// The component will be disabled when the state's value equals the specified value.
        /// </summary>
        public static T DisabledWithState<T, R>(this T component, State<R> state, Func<R> value)
            where T : ReactiveComponent
        {
            component.Enabled = !state.Value?.Equals(value()) ?? false;
            component.On(
                state,
                val =>
                {
                    component.Enabled = !val?.Equals(value()) ?? false;
                }
            );
            return component;
        }

        /// <summary>
        /// Makes a component's enabled state react to an state value.
        /// The component will be enabled when the state's value equals the specified value.
        /// </summary>
        public static T EnabledWithState<T, R>(this T component, State<R> state, R value)
            where T : ReactiveComponent
        {
            return component.EnabledWithState(state, () => value);
        }

        /// <summary>
        /// Makes a GameObject's active state react to a state value.
        /// The GameObject will be active when the state's value equals the specified value.
        /// </summary>
        public static GameObject EnabledWithState<R>(
            this GameObject component,
            State<R> state,
            R value
        )
        {
            return component.EnabledWithState(state, () => value);
        }

        /// <summary>
        /// Makes a component's enabled state react inversely to an state value.
        /// The component will be disabled when the state's value equals the specified value.
        /// </summary>
        public static T DisabledWithState<T, R>(this T component, State<R> state, R value)
            where T : ReactiveComponent
        {
            return component.DisabledWithState(state, () => value);
        }

        /// <summary>
        /// Creates a new state value with a default value assigned and exports it.
        /// </summary>
        public static T ExtractState<T, R>(this T component, out State<R> state, R value)
            where T : ReactiveComponent
        {
            state = new State<R>(value);
            return component;
        }
    }
}
