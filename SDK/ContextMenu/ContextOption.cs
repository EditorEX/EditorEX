using System;

namespace EditorEX.SDK.ContextMenu
{
    public struct ContextOption<T> : IContextOption where T : IContextMenuObject
    {
        public ContextOption(string text, Action<T> onClick)
        {
            Text = text;
            OnClick = onClick;
        }

        public string Text { get; set; }
        public Action<T> OnClick { get; set; }

        public string GetText()
        {
            return Text;
        }

        public void Invoke(object obj)
        {
            if (obj.GetType().IsAssignableFrom(typeof(T))) 
            {
                OnClick((T)obj);
            }
        }
    }
}
