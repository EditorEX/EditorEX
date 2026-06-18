namespace EditorEX.Essentials.Features.ViewMode;

public class ViewModeSwitchedSignal(ViewMode viewMode)
{
    public ViewMode ViewMode { get; private set; } = viewMode;
}
