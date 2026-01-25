namespace EditorEX.SDK.ContextMenu.Objects
{
    public class SourceListContextMenuObject : IContextMenuObject
    {
        public SourceListContextMenuObject(string sourceName)
        {
            SourceName = sourceName;
        }

        public string SourceName { get; private set; }
    }
}
