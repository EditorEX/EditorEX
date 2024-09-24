using EditorEX.Essentials.Movement.Note.MovementProvider;
using System;

namespace EditorEX.Essentials.Movement.Types
{
    public class NoteGameMovementType : IObjectMovementType
    {
        public string[] ViewModes => new string[] { "Preview" };

        public Type Type => typeof(EditorNoteGameMovement);
    }
}
