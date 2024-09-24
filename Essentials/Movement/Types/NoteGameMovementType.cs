using BetterEditor.Essentials.Movement.Note.MovementProvider;
using System;

namespace BetterEditor.Essentials.Movement.Types
{
    public class NoteGameMovementType : IObjectMovementType
    {
        public string[] ViewModes => new string[] { "Preview" };

        public Type Type => typeof(EditorNoteGameMovement);
    }
}
