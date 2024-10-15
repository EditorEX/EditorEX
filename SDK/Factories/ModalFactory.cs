using EditorEX.SDK.Collectors;
using EditorEX.SDK.Components;
using HMUI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.SDK.Factories
{
    internal class ModalFactory
    {
        private ImageFactory _imageFactory;
        private ColorCollector _colorCollector;
        private IInstantiator _instantiator;

        [Inject]
        private void Construct(
            ImageFactory imageFactory,
            ColorCollector colorCollector,
            IInstantiator instantiator)
        {
            _imageFactory = imageFactory;
            _colorCollector = colorCollector;
            _instantiator = instantiator;
        }

        public EditorModalView Create(Transform parent)
        {
            var gameObject = new GameObject("ExModalView");
            var modalView = _instantiator.InstantiateComponent<EditorModalView>(gameObject);

            gameObject.transform.SetParent(parent, false);

            modalView.blockerClickedEvent += () =>
            {
                modalView.Hide();
            };

            var bg = _imageFactory.Create(modalView.transform, "#Background8px", new Base.LayoutData());
            bg._colorSo = _colorCollector.GetColor("VerticalList/Background/Pressed");
            
            var highlight = _imageFactory.Create(modalView.transform, "#Background8px", new Base.LayoutData());
            highlight._colorSo = _colorCollector.GetColor("Button/Text/Highlighted");

            bg.transform.SetParent(highlight.transform);
            bg.rectTransform.sizeDelta = new Vector2(-5f, -5f);
            bg.rectTransform.anchorMin = Vector2.zero;
            bg.rectTransform.anchorMax = Vector2.one;

            gameObject.AddComponent<StackLayoutGroup>();

            var fitter = gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return modalView;
        }
    }
}
