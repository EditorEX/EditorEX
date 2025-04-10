using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatmapEditor3D.InputSystem;

namespace EditorEX.SDK.Input
{
    public class CustomInputAction
    {
        public CustomInputAction(string name, InputKey[] keys)
        {
            Name = name;
            Keys = keys;
        }

        public void AssignActionIndex(int index)
        {
            if (ActionIndex != -1)
            {
                throw new InvalidOperationException($"Action {Name} has already been assigned an index of {ActionIndex}.");
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
    }
}
