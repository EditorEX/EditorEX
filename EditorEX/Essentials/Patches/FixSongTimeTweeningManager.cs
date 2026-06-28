using BeatmapEditor3D;
using HarmonyLib;
using SiraUtil.Affinity;
using Tweening;
using Zenject;

namespace EditorEX.Essentials.Patches;

public class FixSongTimeTweening : IAffinity
{
    private AudioDataModel _audioDataModel;

    [Inject]
    public void Construct(AudioDataModel audioDataModel)
    {
        _audioDataModel = audioDataModel;
    }

    [AffinityPatch(typeof(Tween), nameof(Tween.SetStartTimeAndEndTime))]
    [AffinityPrefix]
    private bool SetStartTimeAndEndTime(Tween __instance, float startTime, float endTime)
    {
        __instance._startTime = _audioDataModel.bpmData.BeatToSeconds(startTime);
        __instance._duration =
            _audioDataModel.bpmData.BeatToSeconds(endTime) - __instance._startTime;
        return false;
    }
}
