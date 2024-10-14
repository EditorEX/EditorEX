using EditorEX.SDK.Collectors;
using EditorEX.SDK.Components;
using UnityEngine;
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

            _imageFactory.Create(modalView.transform, "#Background8px", new Base.LayoutData())._colorSo = _colorCollector.GetColor("VerticalList/Background/Pressed");
            var highlight = _imageFactory.Create(modalView.transform, "#Background8px", new Base.LayoutData());
            highlight._colorSo = _colorCollector.GetColor("Button/Text/Highlighted");
            highlight.transform.localScale = new Vector3(1.01f, 1.02f, 1f);

            return modalView;
        }
    }
}
