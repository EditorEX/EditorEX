using EditorEX.CustomJSONData.CustomEvents;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using Heck.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using Heck.Deserialize;
using SiraUtil.Logging;

namespace EditorEX.Heck.EventData
{
    public class EditorCoroutineEventData : ICustomEventCustomData
    {
        private readonly SiraLog _siraLog;

        public EditorCoroutineEventData(
            SiraLog siraLog,
            CustomEventEditorData customEventData,
            Dictionary<string, List<object>> pointDefinitions,
            Dictionary<string, Track> beatmapTracks,
            bool v2)
        {
            _siraLog = siraLog;
            CustomData data = customEventData.customData;
            IEnumerable<Track> tracks = data.GetTrackArray(beatmapTracks, v2);

            string[] excludedStrings = { Constants.V2_TRACK, Constants.V2_DURATION, Constants.V2_EASING, Constants.TRACK, Constants.DURATION, Constants.EASING, Constants.REPEAT };
            IEnumerable<string> propertyKeys = data.Keys.Where(n => excludedStrings.All(m => m != n)).ToList();
            List<CoroutineInfo> coroutineInfos = new List<CoroutineInfo>();
            foreach (Track track in tracks)
            {
                bool path;
                switch (customEventData.eventType)
                {
                    case Constants.ANIMATE_TRACK:
                        path = false;
                        break;
                    case Constants.ASSIGN_PATH_ANIMATION:
                        path = true;
                        break;
                    default:
                        throw new InvalidOperationException("Custom event was not of correct type.");
                };

                foreach (string propertyKey in propertyKeys)
                {
                    if (!v2)
                    {
                        if (path)
                        {
                            HandlePathProperty(propertyKey);
                        }
                        else
                        {
                            HandleProperty(propertyKey);
                        }
                    }
                    else
                    {
                        if (path)
                        {
                            Track.GetPathAliases(propertyKey).Do(n => HandlePathProperty(n, propertyKey));
                        }
                        else
                        {
                            Track.GetAliases(propertyKey).Do(n => HandleProperty(n, propertyKey));
                        }
                    }

                    continue;

                    void CreateInfo(BaseProperty property, IPropertyBuilder builder, string name, string alias)
                    {
                        CoroutineInfo coroutineInfo = new CoroutineInfo(builder.GetPointData(data, alias ?? name, pointDefinitions), property, track);
                        coroutineInfos.Add(coroutineInfo);
                    }

                    void HandlePathProperty(string name, string alias = null)
                    {
                        IPropertyBuilder builder = Track.GetPathBuilder(name);
                        if (builder == null)
                        {
                            _siraLog.Error($"Could not find path property [{name}]");
                            return;
                        }

                        CreateInfo(track.GetOrCreatePathProperty(name, builder), builder, name, alias);
                    }

                    void HandleProperty(string name, string alias = null)
                    {
                        IPropertyBuilder builder = Track.GetBuilder(name);
                        if (builder == null)
                        {
                            _siraLog.Error($"Could not find property [{name}]");
                            return;
                        }

                        CreateInfo(track.GetOrCreateProperty(name, builder), builder, name, alias);
                    }
                }
            }

            Duration = data.Get<float?>(v2 ? Constants.V2_DURATION : Constants.DURATION) ?? 0f;
            Easing = data.GetStringToEnum<Functions?>(v2 ? Constants.V2_EASING : Constants.EASING) ?? Functions.easeLinear;
            CoroutineInfos = coroutineInfos;

            if (!v2)
            {
                Repeat = data.Get<int?>(Constants.REPEAT) ?? 0;
            }
        }

        internal float Duration { get; }

        internal Functions Easing { get; }

        internal int Repeat { get; }

        internal List<CoroutineInfo> CoroutineInfos { get; }

        internal readonly struct CoroutineInfo
        {
            internal CoroutineInfo(IPointDefinition pointDefinition, BaseProperty property, Track track)
            {
                PointDefinition = pointDefinition;
                Property = property;
                Track = track;
            }

            internal IPointDefinition PointDefinition { get; }

            internal BaseProperty Property { get; }

            internal Track Track { get; }
        }
    }
}
