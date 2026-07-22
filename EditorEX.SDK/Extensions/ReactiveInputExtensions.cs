using System;
using BeatmapEditor3D;
using EditorEX.SDK.ReactiveComponents;
using Reactive;

namespace EditorEX.SDK.Extensions
{
    public static class ReactiveInputExtensions
    {
        /// <summary>
        /// Wraps a component in an EditorNamedRail, which provides a labeled container for UI elements.
        /// </summary>
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
    }
}
