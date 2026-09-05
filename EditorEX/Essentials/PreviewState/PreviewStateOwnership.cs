using System;
using System.Collections.Generic;

namespace EditorEX.Essentials.PreviewState
{
    internal static class PreviewStateOwnership
    {
        public static float NextExclusiveEnd<T>(
            IReadOnlyList<T> sorted,
            int index,
            Func<T, float> beatOf,
            Func<T, T, bool> conflicts
        )
        {
            T current = sorted[index];
            for (int j = index + 1; j < sorted.Count; j++)
            {
                if (conflicts(current, sorted[j]))
                {
                    return beatOf(sorted[j]);
                }
            }

            return float.MaxValue;
        }
    }
}
