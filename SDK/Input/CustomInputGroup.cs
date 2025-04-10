using System;
using System.Collections.Generic;
using BeatmapEditor3D.InputSystem;

namespace EditorEX.SDK.Input
{
    public class CustomInputGroup : ICustomInputGroup
    {
        public CustomInputGroup(string groupID, string displayName, CustomInputAction[] actions)
        {
            ID = groupID;
            Name = displayName;
            this.actions = [.. actions];
        }

        public void AssignGroupIndex(int index)
        {
            if (GroupIndex != -1)
            {
                throw new InvalidOperationException($"Group {ID} has already been assigned an index of {GroupIndex}.");
            }
            GroupIndex = index;
        }

        public KeyBindingGroupType GetKeyBindingGroupType()
        {
            return (KeyBindingGroupType)GroupIndex;
        }

        public CustomInputAction[] GetKeybindings()
        {
            return actions.ToArray();
        }

        public string ID { get; }
        public string Name { get; }
        public int GroupIndex { get; private set; } = -1;
        private readonly List<CustomInputAction> actions;
    }
}
