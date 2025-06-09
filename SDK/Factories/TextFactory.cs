using EditorEX.SDK.Collectors;
using HMUI;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Factories
{
    // This class MUST be injected using Zenject. You cannot create it manually.
    public class TextFactory
    {
        private ColorCollector _colorCollector = null!;
        private FontCollector _fontCollector = null!;

        [Inject]
        private void Construct(
            ColorCollector colorCollector,
            FontCollector fontCollector)
        {
            _colorCollector = colorCollector;
            _fontCollector = fontCollector;
        }

        public CurvedTextMeshPro Create(Transform parent, string text, float fontSize, string colorType)
        {
            return Create<CurvedTextMeshPro>(parent, text, fontSize, colorType);
        }

        public T Create<T>(Transform parent, string text, float fontSize, string colorType) where T : CurvedTextMeshPro
        {
            GameObject gameObj = new($"Ex{typeof(T).Name}")
            {
                layer = 5,
            };

            gameObj.SetActive(false);
            gameObj.transform.SetParent(parent, false);

            var textMesh = gameObj.AddComponent<T>();
            textMesh.font = _fontCollector.GetFontAsset();
            textMesh.fontSharedMaterial = _fontCollector.GetMaterial();
            textMesh.fontSize = fontSize;
            textMesh.useScriptableObjectColors = colorType != "None";
            textMesh._colorSo = colorType != "None" ? _colorCollector.GetColor(colorType) : null;
            textMesh.text = text;

            textMesh.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            textMesh.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            gameObj.SetActive(true);

            return textMesh;
        }
    }
}
