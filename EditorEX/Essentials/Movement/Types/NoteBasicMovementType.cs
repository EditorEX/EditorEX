using System;
using EditorEX.Essentials.Movement.Note.MovementProvider;

namespace EditorEX.Essentials.Movement.Types
{
    public class NoteBasicMovementType : IObjectMovementType
    {
        public string[] ViewModes => new string[] { "Normal" };

        public Type Type => typeof(EditorNoteBasicMovement);
    }
}
