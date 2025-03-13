using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Collectors
{
    public class TransitionCollector : IInitializable
    {
        internal Dictionary<string, BaseTransitionSO> _transitions = new();

        private ColorCollector _colorCollector;

        [Inject]
        private void Construct(
            ColorCollector colorCollector)
        {
            _colorCollector = colorCollector;
        }

        public void Initialize()
        {
            var transitions = Resources.FindObjectsOfTypeAll<BaseTransitionSO>();
            foreach (var transition in transitions)
            {
                if (transition.name.StartsWith("BeatmapEditor"))
                {
                    string remappedName = transition.name.Substring(14).Replace(".", "/");
                    _transitions[remappedName] = transition;
                    //Debug.Log($"Found transition: {remappedName}");
                }
            }

            InjectTransitions();
        }

        private void InjectTransitions()
        {
            AddColorTransition("EditorDropdown/CellBackground", "Dropdown/Background/Normal", "Button/Background/Highlighted", "Button/Background/Pressed", "Button/Background/Disabled", "Button/Background/Selected", "Button/Background/SelectedAndHighlighted");
            AddColorTransition("ClickableImage/Image", "ClickableImage/Default", "ClickableImage/Hover", "ClickableImage/Hover", "ClickableImage/Disabled", "ClickableImage/Hover", "ClickableImage/Hover");
        }

        private void AddColorTransition(string transitionName, string normalColor, string highlightedColor, string pressedColor, string disabledColor, string selectedColor, string selectedAndHighlightedColor)
        {
            var transition = NewColorTransition(_colorCollector.GetColor(normalColor), _colorCollector.GetColor(highlightedColor), _colorCollector.GetColor(pressedColor), _colorCollector.GetColor(disabledColor), _colorCollector.GetColor(selectedColor), _colorCollector.GetColor(selectedAndHighlightedColor));
            transition.name = transitionName;
            _transitions[transitionName] = transition;
        }

        private ColorTransitionSO NewColorTransition(ColorSO normalColor, ColorSO highlightedColor, ColorSO pressedColor, ColorSO disabledColor, ColorSO selectedColor, ColorSO selectedAndHighlightedColor)
        {
            var transition = ScriptableObject.CreateInstance<ColorTransitionSO>();
            transition._normalColor = normalColor;
            transition._highlightedColor = highlightedColor;
            transition._pressedColor = pressedColor;
            transition._disabledColor = disabledColor;
            transition._selectedColor = selectedColor;
            transition._selectedAndHighlightedColor = selectedAndHighlightedColor;
            transition._transitionTiming = _transitions.First().Value._transitionTiming;
            return transition;
        }

        public T GetTransition<T>(string name) where T : BaseTransitionSO
        {
            if (!_transitions.ContainsKey(name))
            {
                throw new ArgumentException($"Transition {name} does not exist! Did you mispell something?");
            }
            return (T)_transitions[name];
        }
    }
}