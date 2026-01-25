using System.Collections.Generic;
using BeatmapSaveDataCommon;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using V3 = BeatmapSaveDataVersion3;

namespace EditorEX.CustomJSONData.VersionedSaveData
{
    public class CustomBeatmapSaveDataVersioned
    {
        public string version;
        public List<BpmChangeEventData> bpmEvents;
        public List<RotationEventData> rotationEvents;
        public List<ColorNoteData> colorNotes;
        public List<BombNoteData> bombNotes;
        public List<V3.ObstacleData> obstacles;
        public List<V3.SliderData> sliders;
        public List<BurstSliderData> burstSliders;
        public List<V3.WaypointData> waypoints;
        public List<BasicEventData> basicBeatmapEvents;
        public List<ColorBoostEventData> colorBoostBeatmapEvents;
        public List<LightColorEventBoxGroup> lightColorEventBoxGroups;
        public List<LightRotationEventBoxGroup> lightRotationEventBoxGroups;
        public List<LightTranslationEventBoxGroup> lightTranslationEventBoxGroups;
        public List<FxEventBoxGroup> vfxEventBoxGroups;
        public FxEventsCollection _fxEventsCollection;
        public BasicEventTypesWithKeywords basicEventTypesWithKeywords;
        public bool useNormalEventsAsCompatibleEvents;
        public CustomData customData;

        public CustomBeatmapSaveDataVersioned(
            string version,
            List<BpmChangeEventData> bpmEvents,
            List<RotationEventData> rotationEvents,
            List<ColorNoteData> colorNotes,
            List<BombNoteData> bombNotes,
            List<V3.ObstacleData> obstacles,
            List<V3.SliderData> sliders,
            List<BurstSliderData> burstSliders,
            List<V3.WaypointData> waypoints,
            List<BasicEventData> basicBeatmapEvents,
            List<ColorBoostEventData> colorBoostBeatmapEvents,
            List<LightColorEventBoxGroup> lightColorEventBoxGroups,
            List<LightRotationEventBoxGroup> lightRotationEventBoxGroups,
            List<LightTranslationEventBoxGroup> lightTranslationEventBoxGroups,
            List<FxEventBoxGroup> vfxEventBoxGroups,
            FxEventsCollection fxEventsCollection,
            BasicEventTypesWithKeywords basicEventTypesWithKeywords,
            bool useNormalEventsAsCompatibleEvents,
            CustomData customData
        )
        {
            this.version = version;
            this.customData = customData;
            this.useNormalEventsAsCompatibleEvents = useNormalEventsAsCompatibleEvents;
            this.bpmEvents = bpmEvents;
            this.rotationEvents = rotationEvents;
            this.colorNotes = colorNotes;
            this.bombNotes = bombNotes;
            this.obstacles = obstacles;
            this.sliders = sliders;
            this.burstSliders = burstSliders;
            this.waypoints = waypoints;
            this.basicBeatmapEvents = basicBeatmapEvents;
            this.colorBoostBeatmapEvents = colorBoostBeatmapEvents;
            this.lightColorEventBoxGroups = lightColorEventBoxGroups;
            this.lightRotationEventBoxGroups = lightRotationEventBoxGroups;
            this.lightTranslationEventBoxGroups = lightTranslationEventBoxGroups;
            this.vfxEventBoxGroups = vfxEventBoxGroups;
            _fxEventsCollection = fxEventsCollection;
            this.basicEventTypesWithKeywords = basicEventTypesWithKeywords;
        }
    }
}
