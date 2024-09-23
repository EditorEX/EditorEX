using BeatmapEditor3D.DataModels;
using BeatmapSaveDataVersion2_6_0AndEarlier;
using BetterEditor.CustomJSONData.Converters;
using BetterEditor.CustomJSONData.Util;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Xml;

namespace BetterEditor.CustomJSONData.Patches
{

	[HarmonyPatch(typeof(BeatmapProjectManager), nameof(BeatmapProjectManager.SaveBeatmapLevel))]
	public static class BeatmapProjectManagerV2SupportPatch
	{
		public static readonly JsonSerializerSettings compactNoDefault = new JsonSerializerSettings
		{
			DefaultValueHandling = DefaultValueHandling.Ignore,
			Formatting = Newtonsoft.Json.Formatting.None,
			ContractResolver = new DefaultContractResolver
			{
				IgnoreSerializableAttribute = false
			}
		};

		public static readonly JsonSerializerSettings compactWithDefault = new JsonSerializerSettings
		{
			DefaultValueHandling = DefaultValueHandling.Include,
			Formatting = Newtonsoft.Json.Formatting.None,
			ContractResolver = new DefaultContractResolver
			{
				IgnoreSerializableAttribute = false
			}
		};

		public static void SaveBeatmapLevel(ISaveData saveData, string projectPath, string beatmapLevelFilename, object beatmapSaveData, bool supportsFloatValue)
		{
			bool serializeDefaults = true;

			string path = Path.Combine(projectPath, beatmapLevelFilename);
			try
			{
				var settings = serializeDefaults ? compactWithDefault : compactNoDefault;
				if (!supportsFloatValue)
				{
					settings.Converters.Add(new V2BasicEventDataWithoutFloatValueConverter());
				}
				string text = JsonConvert.SerializeObject(beatmapSaveData, settings);
				saveData.Save(path, text);
			}
			catch (Exception ex)
			{
				Plugin.Log.Warn(ex);
			}
			Directory.SetLastWriteTime(projectPath, DateTime.Now);
		}

		[HarmonyPrefix]
		public static bool SaveBeatmapLevelPatch(BeatmapProjectManager __instance, bool clearDirty)
		{
			if (!__instance._projectOpened)
			{
				return false;
			}
			IDifficultyBeatmapSetData difficultyBeatmapSetData;
			if (!__instance._beatmapDataModel.difficultyBeatmapSets.TryGetValue(__instance._beatmapLevelDataModel.beatmapCharacteristic, out difficultyBeatmapSetData))
			{
				return false;
			}
			IDifficultyBeatmapData difficultyBeatmapData;
			if (!difficultyBeatmapSetData.difficultyBeatmaps.TryGetValue(__instance._beatmapLevelDataModel.beatmapDifficulty, out difficultyBeatmapData))
			{
				return false;
			}
			if (!__instance._beatmapLevelDataModelSaver.NeedsSaving())
			{
				return false;
			}
			Version version3 = new Version(3, 0, 0);
			Version version2_5_0 = new Version(2, 5, 0);
			Version version = BeatmapSaveDataHelpers.GetVersion(File.ReadAllText(Path.Combine(__instance._workingBeatmapProject, difficultyBeatmapData.beatmapFilename)));
			Plugin.Log.Info($"Saving V{version.ToString()}");
			object saveData;
			if (version < version3)
			{
				// Save as V2
				saveData = new BeatmapLevelDataModelSaverV2(__instance._beatmapLevelDataModelSaver).Save(version);
			}
			else
			{
				// Save as V3
				saveData = new BeatmapLevelDataModelSaverV3(__instance._beatmapLevelDataModelSaver).Save(version);
			}
			SaveBeatmapLevel(__instance._saveData, __instance._workingBeatmapProject, difficultyBeatmapData.beatmapFilename, saveData, version >= version2_5_0);
			if (clearDirty)
			{
				__instance._beatmapLevelDataModel.ClearDirty();
				__instance._beatmapEventsDataModel.ClearDirty();
				__instance._beatmapEventBoxGroupsDataModel.ClearDirty();
				__instance.BackupProject();
				__instance.SaveTempProject();
			}

			return false;
		}
	}
}
