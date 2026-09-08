using Heck.Animation;

namespace EditorEX.Heck.Events
{
    internal static class HeckTrackPreviewSampler
    {
        public static float EasedProgress(
            float beat,
            float fromBeat,
            float durationBeats,
            int repeat,
            Functions easing,
            out bool complete
        )
        {
            if (durationBeats <= 0f)
            {
                complete = true;
                return 1f;
            }

            float elapsed = beat - fromBeat;
            if (elapsed < 0f)
            {
                elapsed = 0f;
            }

            float total = durationBeats * (repeat + 1);
            if (elapsed >= total)
            {
                complete = true;
                return 1f;
            }

            complete = false;
            float cycleElapsed = elapsed % durationBeats;
            return Easings.Interpolate(cycleElapsed / durationBeats, easing);
        }

        public static bool CanSkipSample(
            bool settled,
            bool hasBaseProvider,
            float beat,
            float latchBeat
        )
        {
            return settled && !hasBaseProvider && beat >= latchBeat;
        }

        public static bool OnLastRepeat(
            float beat,
            float fromBeat,
            float durationBeats,
            int repeat,
            bool complete
        )
        {
            if (complete || repeat <= 0 || durationBeats <= 0f)
            {
                return true;
            }

            return beat - fromBeat >= durationBeats * repeat;
        }
    }
}
