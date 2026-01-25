namespace EditorEX.Essentials.SpawnProcessing
{
    public class EditorObjectSpawnData
    {
        // Sliders

        public bool hasHeadNote { get; set; }

        public NoteLineLayer headBeforeJumpLineLayer { get; set; }

        public float headCutDirectionAngleOffset { get; set; }

        public bool hasTailNote { get; set; }

        public NoteLineLayer tailBeforeJumpLineLayer { get; set; }

        public float tailCutDirectionAngleOffset { get; set; }

        // Notes

        public NoteData.GameplayType gameplayType { get; set; }

        public float timeToNextColorNote { get; set; }

        public float timeToPrevColorNote { get; set; }

        public NoteLineLayer beforeJumpNoteLineLayer { get; set; }

        public int flipLineIndex { get; set; }

        public float flipYSide { get; set; }

        public float cutDirectionAngleOffset { get; set; }
    }
}
