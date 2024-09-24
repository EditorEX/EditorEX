using System;

namespace EditorEX.Essentials.Movement.Types
{
    public interface IObjectMovementType
    {
        string[] ViewModes { get; }

        Type Type { get; }
    }
}
