using BeatmapEditor3D;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.Types;
using EditorEX.MapData.SaveDataLoaders;
using HarmonyLib;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace EditorEX.CustomJSONData.Patches.Loading
{
    public class AudioDataLoaderPatch : IAffinity
    {
        private readonly SiraLog _siraLog;
        private readonly V2BeatmapBpmDataVersionedLoader _v2BeatmapBpmDataVersionedLoader;

        [Inject]
        public AudioDataLoaderPatch(
            SiraLog siraLog,
            V2BeatmapBpmDataVersionedLoader v2BeatmapBpmDataVersionedLoader)
        {
            _siraLog = siraLog;
            _v2BeatmapBpmDataVersionedLoader = v2BeatmapBpmDataVersionedLoader;
        }

        [AffinityPatch(typeof(BeatmapProjectFileHelper), nameof(BeatmapProjectFileHelper.CopyBeatmapFile), AffinityMethodType.Normal, null, new Type[] { typeof(string), typeof(string), typeof(string) })]
        [AffinityPrefix]
        private bool FixCopyFile(string sourceProjectPath, string destinationProjectPath, string beatmapFileFilename)
        {
            if (beatmapFileFilename == "")
            {
                return false;
            }
            if (beatmapFileFilename == "BPMInfo.dat")
            {
                return File.Exists(Path.Combine(sourceProjectPath, beatmapFileFilename));
            }
            return true;
        }

        [AffinityPatch(typeof(AudioDataLoader), nameof(AudioDataLoader.LoadOnlyRegions))]
        [AffinityPrefix]
        private bool LoadOnlyRegionsPatch(AudioDataLoader __instance, string projectPath, string audioDataFilename)
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

            if (setBpmDataWithClip)
            {
                int startOffset = AudioTimeHelper.SecondsToSamples(__instance._beatmapLevelDataModel.songTimeOffset, __instance._audioDataModel.audioClip.frequency);
                bpmData = new BpmData(__instance._beatmapLevelDataModel.beatsPerMinute, __instance._audioDataModel.audioClip.samples, __instance._audioDataModel.audioClip.frequency, startOffset);
                if (bpmData == null)
                {
                    __instance._signalBus.Fire(new BeatmapDataModelSignals.AudioDataLoadedResult(LoadAudioDataResult.UnableToLoadAudioData));
                    return false;
                }
            }
            __instance._audioDataModel.UpdateWith(bpmData, __instance._audioDataModel.audioClip);
            __instance._beatmapBasicEventsDataModel.SetBpmData(bpmData);
            return false;
        }

        [AffinityPatch(typeof(AudioDataLoader), nameof(AudioDataLoader.Load))]
        [AffinityPrefix]
        private bool LoadLoadPatch(AudioDataLoader __instance, string projectPath, string audioDataFilename)
        {
            bool hasV2BpmInfo = File.Exists(Path.Combine(projectPath, "BPMInfo.dat"));
            bool hasV4BpmInfo = File.Exists(Path.Combine(projectPath, audioDataFilename)) && audioDataFilename != "BPMInfo.dat";

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
                        _siraLog.Info($"hi!!! {result.length}");
                        __instance._audioDataModel.UpdateWith(bpmData, result);
                        __instance._beatmapBasicEventsDataModel.SetBpmData(bpmData);
                        __instance._waveformDataModel.PrepareWaveformData(result);
                        __instance._signalBus.Fire(new BeatmapDataModelSignals.AudioDataLoadedResult(LoadAudioDataResult.Success));
                        __instance._waveformDataModel._computeMonoSamples.Complete();
                        __instance._waveformDataModel._computeMonoSamples.scheduled = true;
                    }
                });
            }

            return false;
        }
    }
}