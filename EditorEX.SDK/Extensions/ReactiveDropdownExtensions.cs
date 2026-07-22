using System.Collections.Generic;
using EditorEX.SDK.ReactiveComponents.Dropdown;
using Reactive;
using Reactive.BeatSaber.Components;

namespace EditorEX.SDK.Extensions
{
    public static class ReactiveDropdownExtensions
    {
        /// <summary>
        /// Creates an state value from a dropdown component's current selection.
        /// </summary>
        public static EditorDropdown<T> ExtractStateFromDropdown<T>(
            this EditorDropdown<T> component,
            out State<(T, BsDropdownItem)> state
        )
        {
            return component.ExtractState(
                out state,
                (component.Key, component.Items[component.Key])
            );
        }

        /// <summary>
        /// Binds a dropdown component to an state value for two-way data binding.
        /// Updates the state when dropdown selection changes and updates dropdown when observable changes.
        /// </summary>
        public static EditorDropdown<T> DropdownWithState<T>(
            this EditorDropdown<T> component,
            State<(T, BsDropdownItem)> state
        )
        {
            component.OnKeyChanged += key =>
            {
                if (EqualityComparer<T>.Default.Equals(state.Value.Item1, key))
                {
                    return;
                }

                state.Value = (key, component.Items[key]);
            };
            state.ValueChangedEvent += value =>
            {
                if (
                    component.Items.ContainsKey(value.Item1)
                    && !EqualityComparer<T>.Default.Equals(component.Key, value.Item1)
                )
                {
                    component.Key = value.Item1;
                }
            };
            return component;
        }
    }
}
