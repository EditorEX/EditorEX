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
        public static T WithReactiveContainer<T>(this T component, ReactiveContainer container) where T : IReactiveComponent
        {
            component.WithNativeComponent<T, ReactiveContainerHolder>(out var comp);
            comp.ReactiveContainer = container;
            return component;
        }

        public static T EnabledWithObservable<T, R>(this T component, ObservableValue<R> observable, R value) where T : ReactiveComponent
        {
            component.Enabled = observable.Value?.Equals(value) ?? true;
            component.Animate(observable, () =>
            {
                component.Enabled = observable.Value?.Equals(value) ?? true;
            });
            return component;
        }

        public static GameObject EnabledWithObservable<R>(this GameObject component, ObservableValue<R> observable, R value)
        {
            component.SetActive(observable.Value?.Equals(value) ?? true);
            void Closure(R val)
            {
                if (component == null)
                {
                    observable.ValueChangedEvent -= Closure;
                    return;
                }
                component.SetActive(observable.Value?.Equals(value) ?? true);
            }
            observable.ValueChangedEvent += Closure;
            return component;
        }

        public static EditorNamedRail InEditorNamedRail(this ILayoutItem comp, string text, float fontSize = 12f, float ratio = 70f)
        {
            ILayoutItem comp2 = comp;
            EditorNamedRail namedRail = new EditorNamedRail();
            namedRail.Label.Text = text;
            namedRail.Label.FontSize = fontSize;
            namedRail.Component = comp2;
            namedRail.Ratio = ratio;
            return namedRail.With(delegate (EditorNamedRail x)
            {
                if (comp2 is ISkewedComponent skewedComponent)
                {
                    ((ISkewedComponent)x.Label).Skew = skewedComponent.Skew;
                }
            });
        }

        public static EditorNamedRail LinkNamedRailWithValidator<T, U>(this EditorNamedRail namedRail) where T : BaseInputFieldValidator<U>
        {
            Debug.Assert(namedRail.Component is ReactiveComponent, "Component must be a ReactiveComponent to link with a validator.");
            Debug.Assert((namedRail.Component as ReactiveComponent)!.Content, "Component has no content.");
            Debug.Assert((namedRail.Component as ReactiveComponent)!.Content.GetComponent<T>() == null, "Component must have a validator.");
            var component = (namedRail.Component as ReactiveComponent)!.Content.GetComponent<T>();
            component._valueModifiedHintGo = namedRail.ModifiedHint;
            return namedRail;
        }

        public static EditorStringInput WithInputValidator<T, U>(this EditorStringInput input, out T comp, Action<U>? onInputValidated = null) where T : BaseInputFieldValidator<U>
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

        public static EditorStringInput WithInputValidatorCopy<T, U>(this EditorStringInput input, T orig, ref T comp, Action<U>? onInputValidated = null) where T : BaseInputFieldValidator<U>
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

        public static void PresentEditor(this IModal comp, Transform child, bool animated = true)
        {
            Transform transform = child.GetComponentInParent<ReactiveContainerHolder>().transform;
            ModalSystem<Reactive.Components.Basic.ModalSystem>.PresentModal(comp, transform, animated);
        }

        public static EditorTextDropdown<T> ExtractObservableFromDropdown<T>(this EditorTextDropdown<T> component, out ObservableValue<(T, string)> observable)
        {
            return component.ExtractObservable(out observable, (component.SelectedKey, component.Items[component.SelectedKey]));
        }

        public static T ExtractObservable<T, R>(this T component, out ObservableValue<R> observable, R value) where T : ReactiveComponent
        {
            observable = new ObservableValue<R>(value);
            return component;
        }

        public static EditorTextDropdown<T> DropdownWithObservable<T>(this EditorTextDropdown<T> component, ObservableValue<(T, string)> observable)
        {
            component.SelectedKeyChangedEvent += (key) =>
            {
                if (observable?.Value.Item1?.Equals(key) ?? false)
                {
                    return;
                }
                observable.Value = (key, component.Items[key]);
            };
            observable.ValueChangedEvent += (value) =>
            {
                if (component.Items.ContainsKey(value.Item1))
                {
                    component.Select(value.Item1);
                }
            };
            return component;
        }
    }
}