using BeatmapEditor3D.DataModels;
using EditorEX.Essentials.SpawnProcessing;
using IPA.Utilities;

namespace EditorEX.Util
{
    public static class SpawnDataAssociationExtensions
    {
        // Sliders

        public static void SetHasHeadNote(this BaseSliderEditorData? editorData, bool hasHeadNote)
        {
            EditorSpawnDataRepository.GetSpawnData(editorData).hasHeadNote = hasHeadNote;
        }

        public static void SetHasTailNote(this BaseSliderEditorData? editorData, bool hasTailNote)
        {
            EditorSpawnDataRepository.GetSpawnData(editorData).hasTailNote = hasTailNote;
        }

        public static void SetHeadBeforeJumpLineLayer(
            this BaseSliderEditorData? editorData,
            NoteLineLayer lineLayer
        )
        {
            EditorSpawnDataRepository.GetSpawnData(editorData).headBeforeJumpLineLayer = lineLayer;
        }

        public static void SetTailBeforeJumpLineLayer(
            this BaseSliderEditorData? editorData,
            NoteLineLayer lineLayer
        )
        {
            EditorSpawnDataRepository.GetSpawnData(editorData).tailBeforeJumpLineLayer = lineLayer;
        }

        public static void SetCutDirectionAngleOffset(
            this BaseSliderEditorData? editorData,
            float headCutDirectionAngleOffset,
            float tailCutDirectionAngleOffset
        )
        {
            EditorSpawnDataRepository.GetSpawnData(editorData).headCutDirectionAngleOffset =
                headCutDirectionAngleOffset;
            EditorSpawnDataRepository.GetSpawnData(editorData).tailCutDirectionAngleOffset =
                tailCutDirectionAngleOffset;
        }

        // Notes

        public static void SetBeforeJumpNoteLineLayer(
            this NoteEditorData? editorData,
            NoteLineLayer lineLayer
        )
        {
            EditorSpawnDataRepository.GetSpawnData(editorData).beforeJumpNoteLineLayer = lineLayer;
        }

        public static void ChangeToBurstSliderHead(this NoteEditorData? editorData)
        {
            EditorSpawnDataRepository.GetSpawnData(editorData).gameplayType = NoteData
                .GameplayType
                .BurstSliderHead;
        }

        public static void ChangeToGameNote(this NoteEditorData? editorData)
        {
            EditorSpawnDataRepository.GetSpawnData(editorData).gameplayType = NoteData
                .GameplayType
                .Normal;
        }

        public static void SetNoteFlipToNote(
            this NoteEditorData? editorData,
            NoteEditorData? targetNote
        )
        {
            var baseSpawn = EditorSpawnDataRepository.GetSpawnData(editorData);
            baseSpawn.flipLineIndex = targetNote.column;
            baseSpawn.flipYSide = (float)((editorData.column > targetNote.column) ? 1 : (-1));
            if (
                (editorData.column > targetNote.column && editorData.row < targetNote.row)
                || (editorData.column < targetNote.column && editorData.column > targetNote.row)
            )
            {
                baseSpawn.flipYSide *= -1f;
            }
        }

        public static void SetCutDirectionAngleOffset(
            this NoteEditorData? editorData,
            float cutDirectionAngleOffset
        )
        {
            EditorSpawnDataRepository.GetSpawnData(editorData).cutDirectionAngleOffset =
                cutDirectionAngleOffset;
        }

        public static void ResetNoteFlip(this NoteEditorData? editorData)
        {
            EditorSpawnDataRepository.GetSpawnData(editorData).flipLineIndex = editorData.column;
            EditorSpawnDataRepository.GetSpawnData(editorData).flipYSide = 0f;
        }

        public static void SetNoteToAnyCutDirection(this NoteEditorData editorData)
        {
            editorData.SetField("cutDirection", NoteCutDirection.Any);
        }

        public static void ChangeNoteCutDirection(
            this NoteEditorData editorData,
            NoteCutDirection newCutDirection
        )
        {
            editorData.SetField("cutDirection", newCutDirection);
        }
    }
}
