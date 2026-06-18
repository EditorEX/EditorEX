using System;
using System.Collections.Generic;
using BeatmapEditor3D;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Dropdown;
using EditorEX.SDK.ReactiveComponents.Native;
using JetBrains.Annotations;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace EditorEX.Util
{
    public static class ReactiveExtensions
    {
        public static T WithReactiveContainer<T>(this T component, ReactiveContainer container)
            where T : IReactiveComponent
        {
            component.WithNativeComponent<T, ReactiveContainerHolder>(out var comp);
            comp.ReactiveContainer = container;
            return component;
        }

        public static GameObject WithReactiveContainer(
            this GameObject gameObject,
            ReactiveContainer container
        )
        {
            var comp = gameObject.AddComponent<ReactiveContainerHolder>();
            comp.ReactiveContainer = container;
            return gameObject;
        }

        /// <summary>
        /// Makes a component's enabled state react to an state value.
        /// The component will be enabled when the state's value equals the specified value.
        /// </summary>
        /// <typeparam name="T">The type of the component</typeparam>
        /// <typeparam name="R">The type of the state value</typeparam>
        /// <param name="component">The component to update</param>
        /// <param name="state">The state value to watch</param>
        /// <param name="value">A function returning the value to check against</param>
        /// <returns>The component for chaining</returns>
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
        /// <typeparam name="R">The type of the state value</typeparam>
        /// <param name="component">The GameObject to update</param>
        /// <param name="state">The state value to watch</param>
        /// <param name="value">A function returning the value to check against</param>
        /// <returns>The GameObject for chaining</returns>
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
        /// <typeparam name="T">The type of the component</typeparam>
        /// <typeparam name="R">The type of the state value</typeparam>
        /// <param name="component">The component to update</param>
        /// <param name="state">The state value to watch</param>
        /// <param name="value">A function returning the value to check against</param>
        /// <returns>The component for chaining</returns>
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
        /// <typeparam name="T">The type of the component</typeparam>
        /// <typeparam name="R">The type of the state value</typeparam>
        /// <param name="component">The component to update</param>
        /// <param name="state">The state value to watch</param>
        /// <param name="value">The value to check against</param>
        /// <returns>The component for chaining</returns>
        public static T EnabledWithState<T, R>(this T component, State<R> state, R value)
            where T : ReactiveComponent
        {
            return component.EnabledWithState(state, () => value);
        }

        /// <summary>
        /// Makes a GameObject's active state react to a state value.
        /// The GameObject will be active when the state's value equals the specified value.
        /// </summary>
        /// <typeparam name="R">The type of the state value</typeparam>
        /// <param name="component">The GameObject to update</param>
        /// <param name="state">The state value to watch</param>
        /// <param name="value">The value to check against</param>
        /// <returns>The GameObject for chaining</returns>
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
        /// <typeparam name="T">The type of the component</typeparam>
        /// <typeparam name="R">The type of the state value</typeparam>
        /// <param name="component">The component to update</param>
        /// <param name="state">The state value to watch</param>
        /// <param name="value">The value to check against</param>
        /// <returns>The component for chaining</returns>
        public static T DisabledWithState<T, R>(this T component, State<R> state, R value)
            where T : ReactiveComponent
        {
            return component.DisabledWithState(state, () => value);
        }

        /// <summary>
        /// Wraps a component in an EditorNamedRail, which provides a labeled container for UI elements.
        /// </summary>
        /// <param name="comp">The component to wrap</param>
        /// <param name="text">The label text to display</param>
        /// <param name="fontSize">The font size of the label</param>
        /// <param name="ratio">The ratio between label width and component width</param>
        /// <returns>The configured EditorNamedRail</returns>
        public static EditorNamedRail InEditorNamedRail(
            this ILayoutItem comp,
            string text,
            float fontSize = 12f,
            float ratio = 70f
        )
        {
            ILayoutItem comp2 = comp;
            EditorNamedRail namedRail = new EditorNamedRail();
            namedRail.Label.Text = text;
            namedRail.Label.FontSize = fontSize;
            namedRail.Component = comp2;
            namedRail.Ratio = ratio;
            return namedRail;
        }

        /// <summary>
        /// Links a named rail with a validator component to enable visual feedback for validation states.
        /// </summary>
        /// <typeparam name="T">The type of validator</typeparam>
        /// <typeparam name="U">The type of value being validated</typeparam>
        /// <param name="namedRail">The named rail to link with a validator</param>
        /// <returns>The named rail for chaining</returns>
        public static EditorNamedRail LinkNamedRailWithValidator<T, U>(
            this EditorNamedRail namedRail
        )
            where T : BaseInputFieldValidator<U>
        {
            var component = (namedRail.Component as ReactiveComponent)!.Content.GetComponent<T>();
            component._valueModifiedHintGo = namedRail.ModifiedHint;
            return namedRail;
        }

        /// <summary>
        /// Adds an input validator component to a string input field.
        /// </summary>
        /// <typeparam name="T">The type of validator to add</typeparam>
        /// <typeparam name="U">The type of value being validated</typeparam>
        /// <param name="input">The string input to add validation to</param>
        /// <param name="comp">The output validator component</param>
        /// <param name="onInputValidated">Optional callback for when input is validated</param>
        /// <returns>The input for chaining</returns>
        public static EditorStringInput WithInputValidator<T, U>(
            this EditorStringInput input,
            out T comp,
            Action<U>? onInputValidated = null
        )
            where T : BaseInputFieldValidator<U>
        {
            input.Content.SetActive(false);
            comp = input.Content.AddComponent<T>();
            comp._inputField = input.InputField;
            if (onInputValidated != null)
            {
                comp.onInputValidated += onInputValidated;
            }
            input.Content.SetActive(true);
            return input;
        }

        /// <summary>
        /// Adds an input validator component to a string input field, copying settings from an existing validator.
        /// </summary>
        /// <typeparam name="T">The type of validator to add</typeparam>
        /// <typeparam name="U">The type of value being validated</typeparam>
        /// <param name="input">The string input to add validation to</param>
        /// <param name="orig">The original validator to copy settings from</param>
        /// <param name="comp">The output validator component</param>
        /// <param name="onInputValidated">Optional callback for when input is validated</param>
        /// <returns>The input for chaining</returns>
        public static EditorStringInput WithInputValidatorCopy<T, U>(
            this EditorStringInput input,
            T orig,
            ref T comp,
            Action<U>? onInputValidated = null
        )
            where T : BaseInputFieldValidator<U>
        {
            WithInputValidator(input, out comp, onInputValidated);
            switch (orig)
            {
                case FloatInputFieldValidator floatValidator:
                    var newFloatValidator = comp as FloatInputFieldValidator;
                    newFloatValidator!._min = floatValidator._min;
                    newFloatValidator._max = floatValidator._max;
                    newFloatValidator._validatorType = floatValidator._validatorType;
                    break;
                case IntInputFieldValidator intValidator:
                    var newIntValidator = comp as IntInputFieldValidator;
                    newIntValidator!._min = intValidator._min;
                    newIntValidator._max = intValidator._max;
                    newIntValidator._validatorType = intValidator._validatorType;
                    break;
                case StringInputFieldValidator stringValidator:
                    var newStringValidator = comp as StringInputFieldValidator;
                    newStringValidator!._trimSpaces = stringValidator._trimSpaces;
                    break;
            }
            return input;
        }

        /// <summary>
        /// Presents a modal editor UI component with proper parent transform handling.
        /// </summary>
        /// <param name="comp">The modal component to present</param>
        /// <param name="child">The child transform used to find the parent container</param>
        /// <param name="animated">Whether to animate the presentation</param>
        public static void PresentEditor(this IModal comp, Transform child, bool animated = true)
        {
            Transform transform = child.GetComponentInParent<ReactiveContainerHolder>().transform;
            ModalSystem<Reactive.Components.Basic.ModalSystem>.PresentModal(
                comp,
                transform,
                animated
            );
        }

        /// <summary>
        /// Creates an state value from a dropdown component's current selection.
        /// </summary>
        /// <typeparam name="T">The type of the dropdown key</typeparam>
        /// <param name="component">The dropdown component</param>
        /// <param name="state">The output state containing the selected key and text</param>
        /// <returns>The dropdown for chaining</returns>
        public static EditorTextDropdown<T> ExtractStateFromDropdown<T>(
            this EditorTextDropdown<T> component,
            out State<(T, string)> state
        )
        {
            return component.ExtractState(
                out state,
                (component.SelectedKey, component.Items[component.SelectedKey])
            );
        }

        /// <summary>
        /// Creates a new state value with a default value assigned and exports it.
        /// </summary>
        /// <typeparam name="T">The type of the component</typeparam>
        /// <typeparam name="R">The type of the state value</typeparam>
        /// <param name="component">The component to associate with</param>
        /// <param name="state">The output state value</param>
        /// <param name="value">The initial value for the state</param>
        /// <returns>The component for chaining</returns>
        public static T ExtractState<T, R>(this T component, out State<R> state, R value)
            where T : ReactiveComponent
        {
            state = new State<R>(value);
            return component;
        }

        /// <summary>
        /// Binds a dropdown component to an state value for two-way data binding.
        /// Updates the state when dropdown selection changes and updates dropdown when observable changes.
        /// </summary>
        /// <typeparam name="T">The type of the dropdown key</typeparam>
        /// <param name="component">The dropdown component to bind</param>
        /// <param name="state">The state to bind to</param>
        /// <returns>The dropdown for chaining</returns>
        public static EditorTextDropdown<T> DropdownWithState<T>(
            this EditorTextDropdown<T> component,
            State<(T, string)> state
        )
        {
            component.WithListener(
                "SelectedKey",
                (T x) =>
                {
                    if (state?.Value.Item1?.Equals(x) ?? false)
                    {
                        return;
                    }
                    state?.Value = (x, component.Items[x]);
                }
            );
            state.ValueChangedEvent += (value) =>
            {
                if (component.Items.ContainsKey(value.Item1))
                {
                    component.Select(value.Item1);
                }
            };
            return component;
        }

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
