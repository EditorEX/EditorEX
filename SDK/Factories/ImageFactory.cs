using BeatSaberMarkupLanguage;
using EditorEX.SDK.Base;
using EditorEX.SDK.Collectors;
using HMUI;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Factories
{
    // This class MUST be injected using Zenject. You cannot create it manually.
    public class ImageFactory
    {
        private ColorCollector _colorCollector = null!;

        [Inject]
        private void Construct(ColorCollector colorCollector)
        {
            _colorCollector = colorCollector;
        }

        public ImageView Create(Transform parent, Sprite sprite, LayoutData layoutData)
        {
            var image = Create<ImageView>(parent, layoutData);

            image.sprite = sprite;

            return image;
        }

        public ImageView Create(Transform parent, string location, LayoutData layoutData)
        {
            var image = Create<ImageView>(parent, layoutData);

            image.SetImageAsync(location, false);

            return image;
        }

        public T Create<T>(Transform parent, Sprite sprite, LayoutData layoutData)
            where T : ImageView
        {
            var image = Create<T>(parent, layoutData);

            image.sprite = sprite;

            return image;
        }

        public T Create<T>(Transform parent, string location, LayoutData layoutData)
            where T : ImageView
        {
            var image = Create<T>(parent, layoutData);

            image.SetImageAsync(location, false);

            return image;
        }

        private T Create<T>(Transform parent, LayoutData layoutData)
            where T : ImageView
        {
            GameObject gameObj = new("ExImage") { layer = 5 };

            gameObj.SetActive(false);
            gameObj.transform.SetParent(parent, false);

            var rectTransform = gameObj.AddComponent<RectTransform>();

            rectTransform.sizeDelta = layoutData.sizeDelta ?? rectTransform.sizeDelta;
            rectTransform.anchoredPosition =
                layoutData.anchoredPosition ?? rectTransform.anchoredPosition;

            var image = gameObj.AddComponent<T>();
            image._colorSo = _colorCollector.GetColor("Button/Background/Normal");
            image.useScriptableObjectColors = true;
            image.type = UnityEngine.UI.Image.Type.Sliced;

            gameObj.SetActive(true);

            gameObj.transform.SetAsFirstSibling();

            return image;
        }
    }
}
