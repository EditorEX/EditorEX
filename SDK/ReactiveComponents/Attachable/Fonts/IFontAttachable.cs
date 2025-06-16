using TMPro;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents.Attachable
{
    public interface IFontAttachable
    {
        public TMP_FontAsset Font { get; set; }

        public Material Material { get; set; }
    }
}
