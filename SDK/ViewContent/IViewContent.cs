using Reactive;
using UnityEngine;

namespace EditorEX.SDK.ViewContent
{
    public interface IViewContent<T>
    {
        public T GetViewData();
        public ReactiveComponent Create();
        public void OnEnable();
        public void OnHide();
    }
}
