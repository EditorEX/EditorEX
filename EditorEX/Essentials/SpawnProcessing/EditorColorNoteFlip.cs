using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.Util;
using Heck;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

namespace EditorEX.Essentials.SpawnProcessing
{
    internal static class EditorColorNoteFlip
    {
        public static void Apply(
            IReadOnlyList<NoteEditorData> colorNotesData,
            ICustomDataRepository repo,
            bool v2
        )
        {
            if (colorNotesData.Count != 2)
            {
                return;
            }

            float offset = 4 / 2f;
            float[] lineIndexes = new float[2];
            float[] lineLayers = new float[2];
            for (int i = 0; i < 2; i++)
            {
                if (colorNotesData[i] is not NoteEditorData noteData)
                {
                    continue;
                }

                CustomData customData = noteData.GetOrCreateCustomData(repo);
                IEnumerable<float?>? position = customData
                    .GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)
                    ?.ToList();
                lineIndexes[i] =
                    position?.ElementAtOrDefault(0) + offset ?? colorNotesData[i].column;
                lineLayers[i] = position?.ElementAtOrDefault(1) ?? (float)colorNotesData[i].row;
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
                if (colorNotesData[i] is not NoteEditorData noteData)
                {
                    continue;
                }

                CustomData customData = noteData.GetOrCreateCustomData(repo);
                customData[INTERNAL_FLIPLINEINDEX] = lineIndexes[1 - i];

                float flipYSide = (lineIndexes[i] > lineIndexes[1 - i]) ? 1 : -1;
                if (
                    (lineIndexes[i] > lineIndexes[1 - i] && lineLayers[i] < lineLayers[1 - i])
                    || (lineIndexes[i] < lineIndexes[1 - i] && lineLayers[i] > lineLayers[1 - i])
                )
                {
                    flipYSide *= -1f;
                }

                customData[INTERNAL_FLIPYSIDE] = flipYSide;
            }
        }
    }
}
