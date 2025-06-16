using System.Collections.Generic;
using EditorEX.SDK.Input;

namespace EditorEX.Essentials
{
    public static class InputRef
    {
        public static CustomInputGroup? EssentialsGroup;
        public static CustomInputAction? ToggleEditorGUI;
        public static List<CustomInputAction> ViewModeBindings = new List<CustomInputAction>();
    }
}
