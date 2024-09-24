using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterEditor.CustomJSONData.VersionedSaveData
{
    public class CustomBeatmapSaveDataVersioned
    {
        public string version;
        public List<BeatmapSaveData.BpmChangeEventData> bpmEvents;
        public List<BeatmapSaveData.RotationEventData> rotationEvents;
        public List<BeatmapSaveData.ColorNoteData> colorNotes;
        public List<BeatmapSaveData.BombNoteData> bombNotes;
        public List<BeatmapSaveData.ObstacleData> obstacles;
        public List<BeatmapSaveData.SliderData> sliders;
        public List<BeatmapSaveData.BurstSliderData> burstSliders;
        public List<BeatmapSaveData.WaypointData> waypoints;
        public List<BeatmapSaveData.BasicEventData> basicBeatmapEvents;
        public List<BeatmapSaveData.ColorBoostEventData> colorBoostBeatmapEvents;
        public List<BeatmapSaveData.LightColorEventBoxGroup> lightColorEventBoxGroups;
        public List<BeatmapSaveData.LightRotationEventBoxGroup> lightRotationEventBoxGroups;
        public List<BeatmapSaveData.LightTranslationEventBoxGroup> lightTranslationEventBoxGroups;
        public List<BeatmapSaveData.FxEventBoxGroup> vfxEventBoxGroups;
        public BeatmapSaveData.FxEventsCollection _fxEventsCollection;
        public BeatmapSaveData.BasicEventTypesWithKeywords basicEventTypesWithKeywords;
        public bool useNormalEventsAsCompatibleEvents;
        public CustomData customData;

        public CustomBeatmapSaveDataVersioned(string version, List<BeatmapSaveData.BpmChangeEventData> bpmEvents, List<BeatmapSaveData.RotationEventData> rotationEvents, List<BeatmapSaveData.ColorNoteData> colorNotes, List<BeatmapSaveData.BombNoteData> bombNotes, List<BeatmapSaveData.ObstacleData> obstacles, List<BeatmapSaveData.SliderData> sliders, List<BeatmapSaveData.BurstSliderData> burstSliders, List<BeatmapSaveData.WaypointData> waypoints, List<BeatmapSaveData.BasicEventData> basicBeatmapEvents, List<BeatmapSaveData.ColorBoostEventData> colorBoostBeatmapEvents, List<BeatmapSaveData.LightColorEventBoxGroup> lightColorEventBoxGroups, List<BeatmapSaveData.LightRotationEventBoxGroup> lightRotationEventBoxGroups, List<BeatmapSaveData.LightTranslationEventBoxGroup> lightTranslationEventBoxGroups, List<BeatmapSaveData.FxEventBoxGroup> vfxEventBoxGroups, BeatmapSaveData.FxEventsCollection fxEventsCollection, BeatmapSaveData.BasicEventTypesWithKeywords basicEventTypesWithKeywords, bool useNormalEventsAsCompatibleEvents, CustomData customData)
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
