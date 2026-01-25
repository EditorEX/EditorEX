namespace EditorEX.Essentials.Features.ViewMode
{
    public class ViewMode
    {
        public ViewMode(
            string displayName,
            string iD,
            bool previewObjects,
            bool showGridAndSelection,
            bool lockCamera
        )
        {
            DisplayName = displayName;
            ID = iD;
            PreviewObjects = previewObjects;
            ShowGridAndSelection = showGridAndSelection;
            LockCamera = lockCamera;
        }

        public string DisplayName { get; set; }
        public string ID { get; }
        public bool PreviewObjects { get; }
        public bool ShowGridAndSelection { get; }
        public bool LockCamera { get; set; }
        public bool Available { get; set; }
    }
}
