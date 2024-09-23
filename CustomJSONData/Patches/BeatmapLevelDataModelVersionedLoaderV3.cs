using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using BeatmapSaveDataVersion3;
using BetterEditor.CustomJSONData.CustomEvents;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BetterEditor.CustomJSONData.Patches
{

	[HarmonyPatch(typeof(BeatmapLevelDataModelVersionedLoader), nameof(BeatmapLevelDataModelVersionedLoader.Load_v3Raw))]
	public static class BeatmapLevelDataModelVersionedLoaderV3Patch
	{
		static IEnumerable<U> GetEditorData<T, U>(IEnumerable<T> list, Func<T, U> convert, Func<T, CustomData> getCustomData) where U : BaseEditorData
		{
			foreach (T obj in list)
			{
				var customData = getCustomData(obj);
				var editorData = convert(obj);
				CustomDataRepository.AddCustomData(editorData, customData);

				yield return editorData;
			}
		}

		public static CustomEventEditorData CreateCustomEventEditorData(CustomBeatmapSaveData.CustomEventData data)
		{
			return CustomEventEditorData.CreateNew(data.beat, data.type, data.customData, false);
		}

		[HarmonyPrefix]
		public static bool Load_v3RawPatch(BeatmapLevelDataModelVersionedLoader __instance, string projectPath, string filename, out List<NoteEditorData> notes, out List<WaypointEditorData> waypoints, out List<ObstacleEditorData> obstacles, out List<ArcEditorData> arcs, out List<ChainEditorData> chains, out List<BasicEventEditorData> basicEvents, out List<BeatmapEditorEventBoxGroupInput> eventBoxGroups, out List<BasicEventTypesForKeywordEditorData> basicEventTypesForKeywordEditorData, out bool useNormalEventsAsCompatibleEvents)
		{
			CustomDataRepository.ClearAll();

			var customLevelInfoSaveData = CustomLevelInfoSaveData.Deserialize(File.ReadAllText(Path.Combine(projectPath, "Info.dat")));

			string fullPath = Path.Combine(projectPath, filename);

			Version version = BeatmapSaveDataHelpers.GetVersion(File.ReadAllText(fullPath));
			CustomBeatmapSaveData beatmapSaveData = CustomBeatmapSaveData.Deserialize(fullPath, customLevelInfoSaveData.beatmapCustomDatasByFilename[filename], customLevelInfoSaveData.customData);
			Plugin.Log.Info("Loading " + filename);
			Plugin.Log.Info($"Loaded {beatmapSaveData.colorNotes.Count} notes; {beatmapSaveData.basicBeatmapEvents.Count} events");
			notes = GetEditorData(beatmapSaveData.colorNotes.Cast<CustomBeatmapSaveData.ColorNoteData>(), BeatmapLevelDataModelVersionedLoader.CreateColorNoteEditorData_v3, x => x.customData).ToList();
			notes.AddRange(GetEditorData(beatmapSaveData.bombNotes.Cast<CustomBeatmapSaveData.BombNoteData>(), BeatmapLevelDataModelVersionedLoader.CreateBombNoteEditorData_v3, x => x.customData).ToList());
			waypoints = GetEditorData(beatmapSaveData.waypoints.Cast<CustomBeatmapSaveData.WaypointData>(), BeatmapLevelDataModelVersionedLoader.CreateWaypointEditorData_v3, x => x.customData).ToList();
			obstacles = GetEditorData(beatmapSaveData.obstacles.Cast<CustomBeatmapSaveData.ObstacleData>(), BeatmapLevelDataModelVersionedLoader.CreateObstacleEditorData_v3, x => x.customData).ToList();
			arcs = GetEditorData(beatmapSaveData.sliders.Cast<CustomBeatmapSaveData.SliderData>(), BeatmapLevelDataModelVersionedLoader.CreateSliderEditorData_v3, x => x.customData).ToList();
			chains = GetEditorData(beatmapSaveData.burstSliders.Cast<CustomBeatmapSaveData.BurstSliderData>(), BeatmapLevelDataModelVersionedLoader.CreateBurstSliderEditorData_v3, x => x.customData).ToList();
			basicEvents = GetEditorData(beatmapSaveData.basicBeatmapEvents.Cast<CustomBeatmapSaveData.BasicEventData>(), BeatmapLevelDataModelVersionedLoader.CreateEventEditorData_v3, x => x.customData).ToList();
			basicEvents.AddRange(GetEditorData(beatmapSaveData.colorBoostBeatmapEvents.Cast<CustomBeatmapSaveData.ColorBoostEventData>(), BeatmapLevelDataModelVersionedLoader.CreateEventEditorDataFromColorBoost_v3, x => x.customData).ToList());
			basicEvents.AddRange(GetEditorData(beatmapSaveData.rotationEvents.Cast<CustomBeatmapSaveData.RotationEventData>(), BeatmapLevelDataModelVersionedLoader.CreateEventEditorDataFromRotation_v3, x => x.customData).ToList());
			var customEvents = GetEditorData(beatmapSaveData.customEvents, CreateCustomEventEditorData, x => x.customData).ToList();

			CustomDataRepository.SetCustomBeatmapSaveData(beatmapSaveData);
			CustomDataRepository.SetCustomEvents(customEvents);

			eventBoxGroups = new List<BeatmapEditorEventBoxGroupInput>()
				.Concat(beatmapSaveData.lightColorEventBoxGroups.Select(new Func<BeatmapSaveData.LightColorEventBoxGroup, BeatmapEditorEventBoxGroupInput>(BeatmapLevelDataModelVersionedLoader.CreateLightColorEventBoxGroup_v3)).OrderBy((BeatmapEditorEventBoxGroupInput e) => e.eventBoxGroup.beat))
				.Concat(beatmapSaveData.lightRotationEventBoxGroups.Select(new Func<BeatmapSaveData.LightRotationEventBoxGroup, BeatmapEditorEventBoxGroupInput>(BeatmapLevelDataModelVersionedLoader.CreateLightRotationEventBoxGroup_v3)).OrderBy((BeatmapEditorEventBoxGroupInput e) => e.eventBoxGroup.beat))
				.Concat(beatmapSaveData.lightTranslationEventBoxGroups.Select(new Func<BeatmapSaveData.LightTranslationEventBoxGroup, BeatmapEditorEventBoxGroupInput>(BeatmapLevelDataModelVersionedLoader.CreateLightTranslationEventBoxGroup_v3)).OrderBy((BeatmapEditorEventBoxGroupInput e) => e.eventBoxGroup.beat))
				.Concat(beatmapSaveData.vfxEventBoxGroups.Select(new Func<BeatmapSaveData.FxEventBoxGroup, BeatmapEditorEventBoxGroupInput>((x) => BeatmapLevelDataModelVersionedLoader.CreateFxEventBoxGroupWithFxEventsCollection_v3(x, beatmapSaveData._fxEventsCollection))).OrderBy((BeatmapEditorEventBoxGroupInput e) => e.eventBoxGroup.beat))
				.ToList();
			basicEventTypesForKeywordEditorData = beatmapSaveData.basicEventTypesWithKeywords.data.Select(new Func<BeatmapSaveData.BasicEventTypesWithKeywords.BasicEventTypesForKeyword, BasicEventTypesForKeywordEditorData>(__instance.CreateBasicEventTypesForKeywordData_v3)).ToList<BasicEventTypesForKeywordEditorData>();
			useNormalEventsAsCompatibleEvents = beatmapSaveData.useNormalEventsAsCompatibleEvents;

			return false;
		}
	}
}
