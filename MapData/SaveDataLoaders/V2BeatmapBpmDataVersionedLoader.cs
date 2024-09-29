using System;
using System.Linq;
using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData.Bpm;
using Zenject;

namespace EditorEX.MapData.SaveDataLoaders
{
	public class V2BeatmapBpmDataVersionedLoader
	{
		[Inject]
		private readonly BeatmapLevelDataModel _beatmapLevelDataModel;

		private readonly Version _version100 = new Version(1, 0, 0);

		public BpmData Load(string projectPath)
		{
			if (BeatmapProjectFileHelper.GetVersionedJSONVersion(projectPath, "BPMInfo.dat") == _version100)
			{
				return Load_v1(projectPath);
			}
			else
			{
				return LoadCurrent(projectPath);
			}
		}

		private BpmData LoadCurrent(string projectPath)
		{
			BpmInfoSerializedDataV2 bpmSerializedData = BeatmapProjectFileHelper.LoadBeatmapJsonObject<BpmInfoSerializedDataV2>(projectPath, "BPMInfo.dat");
			if (bpmSerializedData != null)
			{
				int startOffset = AudioTimeHelper.SecondsToSamples(_beatmapLevelDataModel.songTimeOffset, bpmSerializedData.songFrequency);
				BpmData bpmData = new BpmData(bpmSerializedData.songFrequency, startOffset, bpmSerializedData.regions.Select((BpmInfoSerializedDataV2.BpmRegionSerializedData region) => new BpmRegion(region.startSampleIndex, region.endSampleIndex, region.startBeat, region.endBeat, bpmSerializedData.songFrequency)).ToList());
				return bpmData;
			}
			return null;
		}

		private BpmData Load_v1(string projectPath)
		{
			BpmInfoSerializedDataV1 bpmSerializedData = BeatmapProjectFileHelper.LoadBeatmapJsonObject<BpmInfoSerializedDataV1>(projectPath, "BPMInfo.dat");
			if (bpmSerializedData != null)
			{
				int startOffset = AudioTimeHelper.SecondsToSamples(_beatmapLevelDataModel.songTimeOffset, bpmSerializedData.songFrequency);
				BpmData bpmData = new BpmData(bpmSerializedData.songFrequency, startOffset, bpmSerializedData.regions.Select((BpmInfoSerializedDataV1.BpmRegionSerializedData region) => new BpmRegion(region.startSampleIndex, region.endSampleIndex, region.startBeat, region.endBeat + 1f, bpmSerializedData.songFrequency)).ToList());
				return bpmData;
			}
			return null;
		}
	}
}