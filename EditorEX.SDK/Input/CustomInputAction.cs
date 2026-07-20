using System;
using BeatmapEditor3D.InputSystem;

namespace EditorEX.SDK.Input
{
    public class CustomInputAction
    {
        public CustomInputAction(string name, bool strict, InputKey[] keys)
        {
            Name = name;
            Keys = keys;
            Strict = strict;
        }

        public CustomInputAction(string name, InputKey[] keys)
        {
            Name = name;
            Keys = keys;
            Strict = false;
        }

        public void AssignActionIndex(int index)
        {
            if (ActionIndex != -1)
            {
                throw new InvalidOperationException(
                    $"Action {Name} has already been assigned an index of {ActionIndex}."
                );
            }
            ActionIndex = index;
        }

        public InputAction GetInputAction()
        {
            return (InputAction)ActionIndex;
        }

        public string Name { get; }
        public InputKey[] Keys { get; }
        public int ActionIndex { get; private set; } = -1;
        public bool Strict { get; }
    }
}
