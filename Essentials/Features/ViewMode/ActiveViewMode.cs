﻿using System;

namespace EditorEX.Essentials.Features.ViewMode
{
    public class ActiveViewMode
    {
        public ViewMode? Mode { get; set; }
        public ViewMode? LastMode { get; set; }
        public bool NoodleExtensions { get; set; } = true;

        public Action? ModeChanged;
    }
}
