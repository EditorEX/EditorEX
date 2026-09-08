using BeatmapEditor3D.Controller;
using SiraUtil.Affinity;

namespace EditorEX.Essentials.Patches
{
    internal class CurrentNoteJumpSpeedSkipPatch : IAffinity
    {
        [AffinityPatch(
            typeof(CurrentNoteJumpSpeedController),
            nameof(CurrentNoteJumpSpeedController.CalculateCurrentNjs)
        )]
        [AffinityPrefix]
        private bool SkipRedundantStaticNjs(CurrentNoteJumpSpeedController __instance)
        {
            float halfJumpDurationInBeats = CoreMathUtils.CalculateHalfJumpDurationInBeats(
                4f,
                18f,
                __instance._beatmapDataModel.noteJumpSpeed,
                __instance._oneBeat,
                __instance._beatmapDataModel.noteJumpStartBeatOffset
            );

            return !CurrentNoteJumpSpeedSkip.CanSkipSignalFire(
                __instance._beatmapObjectsDataModel.noteJumpSpeedEventFrames.Count,
                __instance._beatmapState.noteJumpSpeed,
                __instance._beatmapState.halfJumpDurationInBeats,
                __instance._beatmapState.oneBeat,
                __instance._beatmapDataModel.noteJumpSpeed,
                halfJumpDurationInBeats,
                __instance._oneBeat
            );
        }
    }
}
