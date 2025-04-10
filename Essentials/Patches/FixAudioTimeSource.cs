using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using SiraUtil.Affinity;

namespace EditorEX.Essentials.Patches
{
    internal class FixAudioTimeSource : IAffinity
    {
        [AffinityPatch(typeof(BeatmapEditorAudioTimeSyncController), nameof(BeatmapEditorAudioTimeSyncController.songTime), AffinityMethodType.Getter)]
        [AffinityPrefix]
        private bool FixSongTime(BeatmapEditorAudioTimeSyncController __instance, ref float __result)
        {
            __result = __instance._audioDataModel.bpmData.BeatToSeconds(__instance._beatmapState.beat);
            return false;
        }

        [AffinityPatch(typeof(BeatmapEditorAudioTimeSyncController), nameof(BeatmapEditorAudioTimeSyncController.lastFrameDeltaSongTime), AffinityMethodType.Getter)]
        [AffinityPrefix]
        private bool FixLastDeltaSongTime(BeatmapEditorAudioTimeSyncController __instance, ref float __result)
        {
            if (!__instance._beatmapState.isPlaying)
            {
                __result = 0f;
                return false;
            }
            __result = __instance._audioDataModel.bpmData.BeatToSeconds(__instance._beatmapState.beat) - __instance._audioDataModel.bpmData.BeatToSeconds(__instance._beatmapState.prevBeat);
            return false;
        }

        [AffinityPatch(typeof(BeatmapEditorAudioTimeSyncController), nameof(BeatmapEditorAudioTimeSyncController.songLength), AffinityMethodType.Getter)]
        [AffinityPatch(typeof(BeatmapEditorAudioTimeSyncController), nameof(BeatmapEditorAudioTimeSyncController.songEndTime), AffinityMethodType.Getter)]
        [AffinityPrefix]
        private bool FixSongEndTime(BeatmapEditorAudioTimeSyncController __instance, ref float __result)
        {
            __result = __instance._audioDataModel.audioClip.length;
            return false;
        }
    }
}
