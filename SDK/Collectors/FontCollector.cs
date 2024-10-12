using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Collectors
{
    public class FontCollector : IInitializable
    {
        private TMP_FontAsset _font;
        private Material _material;

        public void Initialize()
        {
            var text = Resources.FindObjectsOfTypeAll<TMP_Text>().FirstOrDefault(x => x.font.name.StartsWith("NotoSans-Medium"));
            _font = Object.Instantiate(text.font);
            _material = Object.Instantiate(text.fontSharedMaterial);
        }

        public TMP_FontAsset GetFontAsset()
        {
            return _font;
        }

        public Material GetMaterial()
        {
            return _material;
        }
    }
}
