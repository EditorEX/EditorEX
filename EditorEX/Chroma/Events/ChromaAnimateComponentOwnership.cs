namespace EditorEX.Chroma.Events
{
    internal static class ChromaAnimateComponentOwnership
    {
        public static bool Conflicts(
            object trackA,
            string componentA,
            string propertyA,
            object trackB,
            string componentB,
            string propertyB
        )
        {
            return ReferenceEquals(trackA, trackB)
                && componentA == componentB
                && propertyA == propertyB;
        }
    }

    internal static class ChromaFogPreview
    {
        public static float Channel(float? animated, float original)
        {
            return animated ?? original;
        }
    }
}
