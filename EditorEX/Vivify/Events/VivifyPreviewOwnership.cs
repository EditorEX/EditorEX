using System.Collections.Generic;

namespace EditorEX.Vivify.Events
{
    internal static class VivifyPreviewOwnership
    {
        public static bool Conflicts(IReadOnlyList<string> idsA, IReadOnlyList<string> idsB)
        {
            for (int i = 0; i < idsA.Count; i++)
            {
                for (int j = 0; j < idsB.Count; j++)
                {
                    if (idsA[i] == idsB[j])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryPostProcessingExclusiveEnd(
            float fromBeat,
            float durationBeats,
            out float toBeat
        )
        {
            if (durationBeats <= 0f)
            {
                toBeat = fromBeat;
                return false;
            }

            toBeat = fromBeat + durationBeats;
            return true;
        }
    }
}
