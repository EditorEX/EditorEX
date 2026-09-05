using System;
using HMUI;

namespace EditorEX.SDK.ReactiveComponents.Dropdown
{
    public class EditorDropdownSelectableCell : SelectableCell
    {
        public Action? Clicked;

        public override void InternalToggle()
        {
            Clicked?.Invoke();
        }
    }
}
