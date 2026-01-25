using BeatmapEditor3D.InputSystem;

namespace EditorEX.SDK.Input
{
    public interface ICustomInputGroup
    {
        public string ID { get; }
        public string Name { get; }
        public int GroupIndex { get; }
        public CustomInputAction[] GetKeybindings();
        public void AssignGroupIndex(int index);
        public KeyBindingGroupType GetKeyBindingGroupType();
    }
}
