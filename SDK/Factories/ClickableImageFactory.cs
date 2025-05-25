using EditorEX.SDK.Base;
using EditorEX.SDK.Collectors;
using EditorEX.SDK.Components;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace EditorEX.SDK.Factories
{
    public class ClickableImageFactory
    {
        private ImageFactory _imageFactory;
        private TransitionCollector _transitionCollector;
        private IInstantiator _instantiator;

        [Inject]
        private void Construct(
            ImageFactory imageFactory,
            TransitionCollector transitionCollector,
            IInstantiator instantiator)
        {
            _imageFactory = imageFactory;
            _transitionCollector = transitionCollector;
            _instantiator = instantiator;
        }

        public EditorNativeClickableImage Create(Transform parent, string location, LayoutData layoutData, Action<PointerEventData> onClick)
        {
            var clickableImage = _imageFactory.Create<EditorNativeClickableImage>(parent, location, layoutData);
            clickableImage.OnClickEvent += onClick;

            clickableImage.useScriptableObjectColors = false;

            clickableImage.gameObject.SetActive(false);

            var selectableStateController = _instantiator.InstantiateComponent<NoTransitionClickableImageSelectableStateController>(clickableImage.gameObject);
            selectableStateController._component = clickableImage;

            var transition = clickableImage.gameObject.AddComponent<ColorGraphicStateTransition>();
            transition._transition = _transitionCollector.GetTransition<ColorTransitionSO>("ClickableImage/Image");
            transition._selectableStateController = selectableStateController;
            transition._component = clickableImage;

            clickableImage.gameObject.SetActive(true);

            return clickableImage;
        }

        public EditorNativeClickableImage Create(Transform parent, string location, LayoutData layoutData, Action onClick)
        {
            return Create(parent, location, layoutData, _ => onClick());
        }
    }
}
