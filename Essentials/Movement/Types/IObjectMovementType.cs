using System;

namespace BetterEditor.Essentials.Movement.Types
{
    public interface IObjectMovementType
    {
        string[] ViewModes { get; }

        Type Type { get; }
    }
}
