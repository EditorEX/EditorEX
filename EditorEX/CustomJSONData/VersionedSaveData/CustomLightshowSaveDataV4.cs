using System;
using System.Collections.Generic;
using BeatmapSaveDataCommon;
using CustomJSONData.CustomBeatmap;
using V4 = BeatmapSaveDataVersion4;

namespace EditorEX.CustomJSONData.VersionedSaveData
{
    public class CustomLightshowSaveDataV4
    {
        public const string CurrentVersion = "4.0.0";

        public string version = CurrentVersion;

        public CustomBeatmapBeatIndex[] waypoints = Array.Empty<CustomBeatmapBeatIndex>();

        public V4.Waypoint[] waypointsData = Array.Empty<V4.Waypoint>();

        public CustomBeatIndex[] basicEvents = Array.Empty<CustomBeatIndex>();

        public V4.BasicEvent[] basicEventsData = Array.Empty<V4.BasicEvent>();

        public CustomBeatIndex[] colorBoostEvents = Array.Empty<CustomBeatIndex>();

        public V4.ColorBoostEvent[] colorBoostEventsData = Array.Empty<V4.ColorBoostEvent>();

        public V4.EventBoxGroup[] eventBoxGroups = Array.Empty<V4.EventBoxGroup>();

        public V4.IndexFilter[] indexFilters = Array.Empty<V4.IndexFilter>();

        public V4.LightColorEventBox[] lightColorEventBoxes = Array.Empty<V4.LightColorEventBox>();

        public V4.LightColorEvent[] lightColorEvents = Array.Empty<V4.LightColorEvent>();

        public V4.LightRotationEventBox[] lightRotationEventBoxes =
            Array.Empty<V4.LightRotationEventBox>();

        public V4.LightRotationEvent[] lightRotationEvents = Array.Empty<V4.LightRotationEvent>();

        public V4.LightTranslationEventBox[] lightTranslationEventBoxes =
            Array.Empty<V4.LightTranslationEventBox>();

        public V4.LightTranslationEvent[] lightTranslationEvents =
            Array.Empty<V4.LightTranslationEvent>();

        public V4.FxEventBox[] fxEventBoxes = Array.Empty<V4.FxEventBox>();

        public V4.FloatFxEvent[] floatFxEvents = Array.Empty<V4.FloatFxEvent>();

        public BasicEventTypesWithKeywords basicEventTypesWithKeywords =
            new BasicEventTypesWithKeywords(
                new List<BasicEventTypesWithKeywords.BasicEventTypesForKeyword>()
            );

        public bool useNormalEventsAsCompatibleEvents;

        public CustomData customData;

        public V4.LightshowSaveData ToVanilla()
        {
            return new V4.LightshowSaveData
            {
                version = version,
                waypoints = waypoints,
                waypointsData = waypointsData,
                basicEvents = basicEvents,
                basicEventsData = basicEventsData,
                colorBoostEvents = colorBoostEvents,
                colorBoostEventsData = colorBoostEventsData,
                eventBoxGroups = eventBoxGroups,
                indexFilters = indexFilters,
                lightColorEventBoxes = lightColorEventBoxes,
                lightColorEvents = lightColorEvents,
                lightRotationEventBoxes = lightRotationEventBoxes,
                lightRotationEvents = lightRotationEvents,
                lightTranslationEventBoxes = lightTranslationEventBoxes,
                lightTranslationEvents = lightTranslationEvents,
                fxEventBoxes = fxEventBoxes,
                floatFxEvents = floatFxEvents,
                basicEventTypesWithKeywords = basicEventTypesWithKeywords,
                useNormalEventsAsCompatibleEvents = useNormalEventsAsCompatibleEvents,
            };
        }
    }
}
