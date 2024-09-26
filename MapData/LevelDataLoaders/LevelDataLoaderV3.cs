using BeatmapEditor3D.DataModels;
using BeatmapEditor3D.SerializedData;
using BeatmapSaveDataCommon;
using BeatmapSaveDataVersion3;
using BeatmapSaveDataVersion4;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.MapData.LevelDataLoaders
{
    public class LevelDataLoaderV3
    {
        public static CustomEventEditorData CreateCustomEventEditorData(Version3CustomBeatmapSaveData.CustomEventSaveData data)
        {
            return CustomEventEditorData.CreateNew(data.beat, data.type, data.customData, false);
        }

        public void Load(BeatmapDataModelsLoader __instance, string projectPath, string filename, out List<NoteEditorData> notes, out List<WaypointEditorData> waypoints, out List<ObstacleEditorData> obstacles, out List<ArcEditorData> arcs, out List<ChainEditorData> chains, out List<BasicEventEditorData> basicEvents, out List<BeatmapEditorEventBoxGroupInput> eventBoxGroups, out List<BasicEventTypesForKeywordEditorData> basicEventTypesForKeywordEditorData, out bool useNormalEventsAsCompatibleEvents)
        {
            CustomDataRepository.ClearAll();

            var customLevelInfoSaveData = CustomLevelInfoSaveData.Deserialize(File.ReadAllText(Path.Combine(projectPath, "Info.dat")));

            string fullPath = Path.Combine(projectPath, filename);

            Version version = BeatmapSaveDataHelpers.GetVersion(File.ReadAllText(fullPath));

            Version3CustomBeatmapSaveData beatmapSaveData = Version3CustomBeatmapSaveData.Deserialize(fullPath);
            
            Plugin.Log.Info("Loading " + filename);
            Plugin.Log.Info($"Loaded {beatmapSaveData.colorNotes.Count} notes; {beatmapSaveData.basicBeatmapEvents.Count} events");
            
            notes = LevelDataLoaderUtil.GetEditorData(
                beatmapSaveData.colorNotes
                .Cast<Version3CustomBeatmapSaveData.ColorNoteSaveData>(), 
                BeatmapDataModelsLoader.CreateColorNoteEditorData_v3)
                .ToList();
            
            notes.AddRange(LevelDataLoaderUtil.GetEditorData(
                beatmapSaveData.bombNotes
                .Cast<Version3CustomBeatmapSaveData.BombNoteSaveData>(), 
                BeatmapDataModelsLoader.CreateBombNoteEditorData_v3)
                .ToList());
            
            waypoints = LevelDataLoaderUtil.GetEditorData(
                beatmapSaveData.waypoints
                .Cast<Version3CustomBeatmapSaveData.WaypointSaveData>(), 
                BeatmapDataModelsLoader.CreateWaypointEditorData_v3)
                .ToList();
            
            obstacles = LevelDataLoaderUtil.GetEditorData(
                beatmapSaveData.obstacles
                .Cast<Version3CustomBeatmapSaveData.ObstacleSaveData>(), 
                BeatmapDataModelsLoader.CreateObstacleEditorData_v3)
                .ToList();

            arcs = LevelDataLoaderUtil.GetEditorData(
                beatmapSaveData.sliders
                .Cast<Version3CustomBeatmapSaveData.SliderSaveData>(), 
                BeatmapDataModelsLoader.CreateArcEditorData_v3)
                .ToList();
            
            chains = LevelDataLoaderUtil.GetEditorData(
                beatmapSaveData.burstSliders
                .Cast<Version3CustomBeatmapSaveData.BurstSliderSaveData>(), 
                BeatmapDataModelsLoader.CreateChainEditorData_v3)
                .ToList();
            
            basicEvents = LevelDataLoaderUtil.GetEditorData(
                beatmapSaveData.basicBeatmapEvents
                .Cast<Version3CustomBeatmapSaveData.BasicEventSaveData>(), 
                BeatmapDataModelsLoader.CreateEventEditorData_v3)
                .ToList();
            
            basicEvents.AddRange(LevelDataLoaderUtil.GetEditorData(
                beatmapSaveData.colorBoostBeatmapEvents
                .Cast<Version3CustomBeatmapSaveData.ColorBoostEventSaveData>(), 
                BeatmapDataModelsLoader.CreateEventEditorDataFromColorBoost_v3)
                .ToList());
            
            basicEvents.AddRange(LevelDataLoaderUtil.GetEditorData(
                beatmapSaveData.rotationEvents
                .Cast<Version3CustomBeatmapSaveData.RotationEventSaveData>(), 
                BeatmapDataModelsLoader.CreateEventEditorDataFromRotation_v3)
                .ToList());
            
            var customEvents = LevelDataLoaderUtil.GetEditorData(
                beatmapSaveData.customEvents, 
                CreateCustomEventEditorData)
                .ToList();

            CustomDataRepository.SetCustomBeatmapSaveData(beatmapSaveData);
            CustomDataRepository.SetCustomEvents(customEvents);

            eventBoxGroups = new List<BeatmapEditorEventBoxGroupInput>()
                .Concat(beatmapSaveData.lightColorEventBoxGroups.Select(new Func<LightColorEventBoxGroup, BeatmapEditorEventBoxGroupInput>(BeatmapDataModelsLoader.CreateLightColorEventBoxGroup_v3)).OrderBy((BeatmapEditorEventBoxGroupInput e) => e.eventBoxGroup.beat))
                .Concat(beatmapSaveData.lightRotationEventBoxGroups.Select(new Func<LightRotationEventBoxGroup, BeatmapEditorEventBoxGroupInput>(BeatmapDataModelsLoader.CreateLightRotationEventBoxGroup_v3)).OrderBy((BeatmapEditorEventBoxGroupInput e) => e.eventBoxGroup.beat))
                .Concat(beatmapSaveData.lightTranslationEventBoxGroups.Select(new Func<LightTranslationEventBoxGroup, BeatmapEditorEventBoxGroupInput>(BeatmapDataModelsLoader.CreateLightTranslationEventBoxGroup_v3)).OrderBy((BeatmapEditorEventBoxGroupInput e) => e.eventBoxGroup.beat))
                .Concat(beatmapSaveData.vfxEventBoxGroups.Select(new Func<FxEventBoxGroup, BeatmapEditorEventBoxGroupInput>((x) => BeatmapDataModelsLoader.CreateFxEventBoxGroupWithFxEventsCollection_v3(x, beatmapSaveData._fxEventsCollection))).OrderBy((BeatmapEditorEventBoxGroupInput e) => e.eventBoxGroup.beat))
                .ToList();
            basicEventTypesForKeywordEditorData = beatmapSaveData.basicEventTypesWithKeywords.data.Select(new Func<BasicEventTypesWithKeywords.BasicEventTypesForKeyword, BasicEventTypesForKeywordEditorData>(__instance.CreateBasicEventTypesForKeywordData_v3)).ToList<BasicEventTypesForKeywordEditorData>();
            useNormalEventsAsCompatibleEvents = beatmapSaveData.useNormalEventsAsCompatibleEvents;
        }
    }
}
