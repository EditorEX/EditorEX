using BeatmapEditor3D.InputSystem;
using EditorEX.Essentials;
using EditorEX.Essentials.ViewMode;
using EditorEX.SDK.Input;

namespace EditorEX.Util
{
    public static class KeybindsExtensions
    {
        public static CustomInputBuilder.GroupBuilding AddViewModeBindings(this CustomInputBuilder.GroupBuilding group)
        {
            for (int i = 0; i < ViewModeRepository.GetViewModes().Count; i++)
            {
                var viewMode = ViewModeRepository.GetViewModes()[i];
                var keybindingName = $"Switch to {viewMode.DisplayName} Viewing Mode";
                InputKey[] defaultKeys = [InputKey.key1 + i];
                CustomInputAction? reference = null;
                group.AddKeybinding(keybindingName, defaultKeys, ref reference);
                if (reference != null)
                {
                    InputRef.ViewModeBindings.Add(reference);
                }
            }
            return group;
        }
    }
}
