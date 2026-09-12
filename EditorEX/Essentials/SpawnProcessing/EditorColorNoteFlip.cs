using System;
using System.Collections.Generic;
using BeatmapEditor3D.DataModels;

namespace EditorEX.Essentials.SpawnProcessing
{
    internal static class EditorColorNoteFlip
    {
        public static void Apply(
            IReadOnlyList<NoteEditorData> colorNotesData,
            Func<NoteEditorData, (float x, float y)> positionOf
        )
        {
            if (colorNotesData.Count != 2)
            {
                return;
            }

            float[] lineIndexes = new float[2];
            float[] lineLayers = new float[2];
            for (int i = 0; i < 2; i++)
            {
                (float x, float y) = positionOf(colorNotesData[i]);
                lineIndexes[i] = x;
                lineLayers[i] = y;
            }

            if (
                colorNotesData[0].type == colorNotesData[1].type
                || (
                    (
                        colorNotesData[0].type != ColorType.ColorA
                        || !(lineIndexes[0] > lineIndexes[1])
                    )
                    && (
                        colorNotesData[0].type != ColorType.ColorB
                        || !(lineIndexes[0] < lineIndexes[1])
                    )
                )
            )
            {
                return;
            }

            for (int i = 0; i < 2; i++)
            {
                EditorObjectSpawnData spawn = EditorSpawnDataRepository.GetSpawnData(
                    colorNotesData[i]
                );
                spawn.flipLineIndex = lineIndexes[1 - i];

                float flipYSide = (lineIndexes[i] > lineIndexes[1 - i]) ? 1 : -1;
                if (
                    (lineIndexes[i] > lineIndexes[1 - i] && lineLayers[i] < lineLayers[1 - i])
                    || (lineIndexes[i] < lineIndexes[1 - i] && lineLayers[i] > lineLayers[1 - i])
                )
                {
                    flipYSide *= -1f;
                }

                spawn.flipYSide = flipYSide;
            }
        }
    }
}
