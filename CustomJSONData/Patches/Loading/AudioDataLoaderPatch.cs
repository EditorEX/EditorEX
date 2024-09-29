using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using EditorEX.MapData.SaveDataLoaders;
using HarmonyLib;
using SiraUtil.Affinity;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace EditorEX.CustomJSONData.Patches.Loading
{
    public class AudioDataLoaderPatch : IAffinity
    {
        private static V2BeatmapBpmDataVersionedLoader _v2BeatmapBpmDataVersionedLoader;

        [Inject]
        public AudioDataLoaderPatch(V2BeatmapBpmDataVersionedLoader v2BeatmapBpmDataVersionedLoader)
        {
            _v2BeatmapBpmDataVersionedLoader = v2BeatmapBpmDataVersionedLoader;
        }

        [AffinityPatch(typeof(AudioDataLoader), nameof(AudioDataLoader.Load))]
        [AffinityPrefix]
        private bool Prefix(AudioDataLoader __instance, string projectPath, string audioDataFilename)
        {
            bool hasV2BpmInfo = File.Exists(Path.Combine(projectPath, "BPMInfo.dat"));
            bool hasV4BpmInfo = File.Exists(Path.Combine(projectPath, audioDataFilename));

            BpmData bpmData = null;

            if (hasV2BpmInfo)
            {
                bpmData = _v2BeatmapBpmDataVersionedLoader.Load(projectPath);
                if (bpmData == null)
                {
                    __instance._signalBus.Fire(new BeatmapDataModelSignals.AudioDataLoadedResult(LoadAudioDataResult.UnableToLoadAudioData));
                    return false;
                }
            }

            if (hasV4BpmInfo)
            {
                BeatmapLevelSaveDataVersion4.AudioSaveData audioSaveData = BeatmapProjectFileHelper.LoadBeatmapJsonObject<BeatmapLevelSaveDataVersion4.AudioSaveData>(projectPath, audioDataFilename);
                if (audioSaveData == null)
                {
                    __instance._signalBus.Fire(new BeatmapDataModelSignals.AudioDataLoadedResult(LoadAudioDataResult.UnableToLoadAudioData));
                    return false;
                }
                else
                {
                    bpmData = AudioDataLoader.LoadBpmData(audioSaveData, __instance._beatmapLevelDataModel.songTimeOffset);
                }
            }

            bool setBpmDataWithClip = false;

            if (bpmData == null)
            {
                setBpmDataWithClip = true;
            }

            Task<AudioClip> audioClip = __instance._audioClipLoader.LoadAudioFile(Path.Combine(projectPath, __instance._beatmapLevelDataModel.songFilename));
            if (audioClip == null)
            {
                __instance._signalBus.Fire(new BeatmapDataModelSignals.AudioDataLoadedResult(LoadAudioDataResult.UnableToLoadAudio));
            }
            else
            {
                audioClip.ContinueWith(task =>
                {
                    var result = audioClip.Result;
                    if (result == null)
                    {
                        __instance._signalBus.Fire(new BeatmapDataModelSignals.AudioDataLoadedResult(LoadAudioDataResult.UnableToLoadAudio));
                    }
                    else
                    {
                        if (setBpmDataWithClip)
                        {
                            int startOffset = AudioTimeHelper.SecondsToSamples(__instance._beatmapLevelDataModel.songTimeOffset, result.frequency);
                            bpmData = new BpmData(__instance._beatmapLevelDataModel.beatsPerMinute, result.samples, result.frequency, startOffset);
                            if (bpmData == null)
                            {
                                __instance._signalBus.Fire(new BeatmapDataModelSignals.AudioDataLoadedResult(LoadAudioDataResult.UnableToLoadAudioData));
                                return;
                            }
                        }
                        __instance._audioDataModel.UpdateWith(bpmData, result);
                        __instance._beatmapBasicEventsDataModel.SetBpmData(bpmData);
                        __instance._waveformDataModel.PrepareWaveformData(result);
                        __instance._signalBus.Fire(new BeatmapDataModelSignals.AudioDataLoadedResult(LoadAudioDataResult.Success));
                    }
                });
            }

            return false;
        }
    }
}