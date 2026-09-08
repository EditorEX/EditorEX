using UnityEngine;

namespace EditorEX.Essentials.Patches
{
    internal static class CurrentNoteJumpSpeedSkip
    {
        public static bool CanSkipSignalFire(
            int njsEventFrameCount,
            float stateNjs,
            float stateHalfJumpDurationInBeats,
            float stateOneBeat,
            float baseNjs,
            float halfJumpDurationInBeats,
            float oneBeat
        )
        {
            if (njsEventFrameCount > 0)
            {
                return false;
            }

            float njs = Mathf.Max(0.01f, baseNjs);
            return stateNjs == njs
                && stateHalfJumpDurationInBeats == halfJumpDurationInBeats
                && stateOneBeat == oneBeat;
        }
    }
}
