namespace EditorEX.SDK.ContextMenu
{
    public interface IContextOption
    {
        public string GetText();
        public void Invoke(object obj);
    }
}
