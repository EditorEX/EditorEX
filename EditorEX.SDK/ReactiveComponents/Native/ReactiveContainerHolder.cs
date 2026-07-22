using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents.Native
{
    public class ReactiveContainerHolder : MonoBehaviour
    {
        public IReactiveContainer ReactiveContainer { get; set; } = null!;
    }
}
