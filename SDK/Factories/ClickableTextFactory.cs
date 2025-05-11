using EditorEX.SDK.Collectors;
using EditorEX.SDK.Components;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace EditorEX.SDK.Factories
{
    public class ClickableTextFactory
    {
        private TextFactory _textFactory;
        private TransitionCollector _transitionCollector;
        private IInstantiator _instantiator;

        [Inject]
        private void Construct(
            TextFactory textFactory,
            TransitionCollector transitionCollector,
            IInstantiator instantiator)
        {
            _textFactory = textFactory;
            _transitionCollector = transitionCollector;
            _instantiator = instantiator;
        }

        public EditorClickableText Create(Transform parent, string text, float fontSize, Action<PointerEventData> onClick)
        {
            var clickableText = _textFactory.Create<EditorClickableText>(parent, text, fontSize, "None");
            clickableText.OnClickEvent += onClick;

            clickableText.gameObject.SetActive(false);

            var selectableStateController = _instantiator.InstantiateComponent<NoTransitionClickableTextSelectableStateController>(clickableText.gameObject);
            selectableStateController._component = clickableText;

            var transition = clickableText.gameObject.AddComponent<ColorTMPTextStateTransition>();
            transition._transition = _transitionCollector.GetTransition<ColorTransitionSO>("Button/Text");
            transition._selectableStateController = selectableStateController;
            transition._component = clickableText;

            clickableText.gameObject.SetActive(true);

            return clickableText;
        }

        public EditorClickableText Create(Transform parent, string text, float fontSize, Action onClick)
        {
            return Create(parent, text, fontSize, _ => onClick());
        }
    }
}
