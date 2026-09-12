using BeatmapEditor3D.DataModels;

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

        public float startNoteLineLayer { get; set; }

        public float tailStartNoteLineLayer { get; set; }

        public float flipLineIndex { get; set; }

        public float flipYSide { get; set; }

        public float cutDirectionAngleOffset { get; set; }

        public void ResetToIdentity(BaseBeatmapObjectEditorData data)
        {
            hasHeadNote = false;
            hasTailNote = false;
            headBeforeJumpLineLayer = (NoteLineLayer)data.row;
            tailBeforeJumpLineLayer = (NoteLineLayer)data.row;
            headCutDirectionAngleOffset = 0f;
            tailCutDirectionAngleOffset = 0f;
            gameplayType = NoteData.GameplayType.Normal;
            timeToNextColorNote = float.MaxValue;
            timeToPrevColorNote = 0f;
            beforeJumpNoteLineLayer = (NoteLineLayer)data.row;
            startNoteLineLayer = data.row;
            tailStartNoteLineLayer = data.row;
            flipLineIndex = data.column;
            flipYSide = 0f;
            cutDirectionAngleOffset = 0f;
        }
    }
}
