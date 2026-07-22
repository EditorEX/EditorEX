using UnityEngine;

namespace EditorEX.SDK.Collectors
{
    public interface IColorCollector
    {
        SimpleColorSO GetColor(string name);
    }
}
