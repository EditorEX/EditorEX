namespace EditorEX.Essentials.Features.ViewMode
{
    public class ViewModeSwitchedSignal
    {
        public ViewMode ViewMode { get; private set; }

        public ViewModeSwitchedSignal(ViewMode viewMode)
        {
            ViewMode = viewMode;
        }
    }
}
