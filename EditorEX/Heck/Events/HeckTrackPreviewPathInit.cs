using Heck.Animation;

namespace EditorEX.Heck.Events
{
    internal static class HeckTrackPreviewPathInit
    {
        public static void Apply(
            IPointDefinitionInterpolation interpolation,
            IPointDefinition? previous,
            IPointDefinition? current
        )
        {
            if (current == null)
            {
                interpolation.Init(null);
                return;
            }

            if (previous != null)
            {
                interpolation.Init(previous);
            }

            interpolation.Init(current);
        }
    }
}
