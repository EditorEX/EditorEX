using System;

namespace BetterEditor.Essentials.ViewMode
{
    public class ActiveViewMode
    {
        public string Mode { get; set; } = "Preview";
        public bool NoodleExtensions { get; set; } = true;

        public Action ModeChanged;
    }
}
