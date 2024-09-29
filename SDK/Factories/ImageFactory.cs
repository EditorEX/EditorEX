using BeatSaberMarkupLanguage;
using EditorEX.SDK.Base;
using HMUI;
using UnityEngine;

namespace EditorEX.SDK.Factories
{
    // This class MUST be injected using Zenject. You cannot create it manually.
    public class ImageFactory
    {
        public ImageView Create(Transform parent, Sprite sprite, LayoutData layoutData)
        {
            var image = Create(parent, layoutData);

            image.sprite = sprite;

            return image;
        }

        public ImageView Create(Transform parent, string location, LayoutData layoutData)
        {
            var image = Create(parent, layoutData);

            image.SetImageAsync(location);

            return image;
        }

        private ImageView Create(Transform parent, LayoutData layoutData)
        {
            GameObject gameObj = new("ExText")
            {
                layer = 5,
            };

            gameObj.SetActive(false);
            gameObj.transform.SetParent(parent, false);

            var rectTransform = gameObj.transform as RectTransform;

            rectTransform.sizeDelta = layoutData.sizeDelta ?? rectTransform.sizeDelta;
            rectTransform.anchoredPosition = layoutData.anchoredPosition ?? rectTransform.anchoredPosition;

            var image = gameObj.GetComponent<ImageView>();

            return image;
        }
    }
}
