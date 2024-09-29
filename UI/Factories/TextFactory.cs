using EditorEX.UI.Collectors;
using HMUI;
using UnityEngine;
using Zenject;

namespace EditorEX.UI.Factories
{
    // This class MUST be injected using Zenject. You cannot create it manually.
    public class TextFactory
    {
        private ColorCollector _colorCollector;
        private FontCollector _fontCollector;

        [Inject]
        private void Construct(ColorCollector colorCollector, FontCollector fontCollector)
        {
            _colorCollector = colorCollector;
            _fontCollector = fontCollector;
        }

        public CurvedTextMeshPro Create(Transform parent, string text, float fontSize, string colorType)
        {
            GameObject gameObj = new("ExText")
            {
                layer = 5,
            };

            gameObj.SetActive(false);
            gameObj.transform.SetParent(parent, false);

            var textMesh = gameObj.AddComponent<CurvedTextMeshPro>();
            textMesh.font = _fontCollector.GetFontAsset();
            textMesh.fontSharedMaterial = _fontCollector.GetMaterial();
            textMesh.fontSize = fontSize;
            textMesh.useScriptableObjectColors = true;
            textMesh._colorSo = _colorCollector.GetColor(colorType);
            textMesh.text = text;

            textMesh.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            textMesh.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            gameObj.SetActive(true);

            return textMesh;
        }
    }
}
