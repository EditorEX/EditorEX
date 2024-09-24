using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using BeatmapSaveDataVersion2_6_0AndEarlier;
using BetterEditor.CustomJSONData.Converters;
using BetterEditor.CustomJSONData.CustomEvents;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BetterEditor.CustomJSONData.Patches
{

    [HarmonyPatch(typeof(BeatmapLevelDataModelVersionedLoader), nameof(BeatmapLevelDataModelVersionedLoader.Load_v2Raw))]
    public static class BeatmapLevelDataModelVersionedLoaderV2Patch
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

        public static CustomEventEditorData CreateCustomEventEditorData(Custom2_6_0AndEarlierBeatmapSaveData.CustomEventData data)
        {
            return CustomEventEditorData.CreateNew(data.time, data.type, data.data, true);
        }

        [HarmonyPrefix]
        public static bool Load_v2RawPatch(BeatmapLevelDataModelVersionedLoader __instance, string projectPath, string filename, out List<NoteEditorData> notes, out List<WaypointEditorData> waypoints, out List<ObstacleEditorData> obstacles, out List<ArcEditorData> sliders, out List<BasicEventEditorData> events, out List<BasicEventTypesForKeywordEditorData> basicEventTypesForKeywordEditorData)
        {
            CustomDataRepository.ClearAll();

            var customLevelInfoSaveData = CustomLevelInfoSaveData.Deserialize(File.ReadAllText(Path.Combine(projectPath, "Info.dat")));

            Version version = BeatmapSaveDataHelpers.GetVersion(File.ReadAllText(Path.Combine(projectPath, filename)));
            Custom2_6_0AndEarlierBeatmapSaveData beatmapSaveData = Custom2_6_0AndEarlierBeatmapSaveData.Deserialize(version, Path.Combine(projectPath, filename));
            Plugin.Log.Info("Loading " + filename);
            Plugin.Log.Info($"Loaded {beatmapSaveData.notes.Count} notes; {beatmapSaveData.events.Count} events");
            notes = GetEditorData(beatmapSaveData.notes.Where(x => x.type != BeatmapSaveData.NoteType.GhostNote && x.type != BeatmapSaveData.NoteType.None), BeatmapLevelDataModelVersionedLoader.CreateNoteEditorData_v2, x => x.customData).ToList();
            waypoints = GetEditorData(beatmapSaveData.waypoints, BeatmapLevelDataModelVersionedLoader.CreateWaypointEditorData_v2, x => x.customData).ToList();
            obstacles = GetEditorData(beatmapSaveData.obstacles, BeatmapLevelDataModelVersionedLoader.CreateObstacleEditorData_v2, x => x.customData).ToList();
            sliders = GetEditorData(beatmapSaveData.sliders, BeatmapLevelDataModelVersionedLoader.CreateSliderEditorData_v2, x => x.customData).ToList();
            events = GetEditorData(beatmapSaveData.events, BeatmapLevelDataModelVersionedLoader.CreateEventEditorData_v2, x => x.customData).ToList();
            var customEvents = GetEditorData(beatmapSaveData.customEvents, CreateCustomEventEditorData, x => x.data).ToList();

            // Our static representation is v3.
            CustomDataRepository.SetCustomBeatmapSaveData(V3SaveDataConverter.ConvertToV3(customLevelInfoSaveData.beatmapCustomDatasByFilename[filename], customLevelInfoSaveData.customData, beatmapSaveData));
            CustomDataRepository.SetCustomEvents(customEvents);

            if (beatmapSaveData.specialEventsKeywordFilters != null && beatmapSaveData.specialEventsKeywordFilters.keywords != null)
            {
                basicEventTypesForKeywordEditorData = beatmapSaveData.specialEventsKeywordFilters.keywords.Select(new Func<BeatmapSaveData.SpecialEventsForKeyword, BasicEventTypesForKeywordEditorData>(__instance.CreateBasicEventTypesForKeywordData_v2)).ToList<BasicEventTypesForKeywordEditorData>();
                return false;
            }
            basicEventTypesForKeywordEditorData = new List<BasicEventTypesForKeywordEditorData>();

            return false;
        }
    }
}
