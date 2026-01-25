using System;
using EditorEX.Essentials.Movement.Note.MovementProvider;

namespace EditorEX.Essentials.Movement.Types
{
    public class NoteGameMovementType : IObjectMovementType
    {
        public string[] ViewModes => new string[] { "Preview" };

        public Type Type => typeof(EditorNoteGameMovement);
    }
}
