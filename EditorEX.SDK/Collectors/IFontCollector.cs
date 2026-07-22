using TMPro;
using UnityEngine;

namespace EditorEX.SDK.Collectors
{
    public interface IFontCollector
    {
        TMP_FontAsset GetFontAsset();
        Material GetMaterial();
    }
}
