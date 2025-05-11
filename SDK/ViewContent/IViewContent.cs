using UnityEngine;

namespace EditorEX.SDK.ViewContent
{
    public interface IViewContent<T>
    {
        public T GetViewData();
        public void Create(GameObject host);
        public void OnEnable();
        public void OnHide();
    }
}
