using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Heck.EventData;
using EditorEX.Heck.ObjectData;
using Heck.Deserialize;
using SiraUtil.Logging;
using Zenject;
using static EditorEX.Heck.Constants;

namespace EditorEX.Heck.Codecs
{
    internal class HeckCustomDataCodec
        : IEarlyCustomDataCodec,
            IObjectCustomDataCodec,
            ICustomEventCustomDataCodec
    {
        private static readonly Dictionary<string, string> ObjectV2ToV3 = new()
        {
            [V2_TRACK] = TRACK,
        };

        private static readonly Dictionary<string, string> EventV2ToV3 = new()
        {
            [V2_TRACK] = TRACK,
            [V2_DURATION] = DURATION,
            [V2_EASING] = EASING,
            [V2_ANIMATION] = ANIMATION,
        };

        private static readonly Dictionary<string, string> ObjectV3ToV2 =
            CustomDataKeyMapper.InvertMap(ObjectV2ToV3);
        private static readonly Dictionary<string, string> EventV3ToV2 =
            CustomDataKeyMapper.InvertMap(EventV2ToV3);

        private readonly SiraLog? _log;

        internal HeckCustomDataCodec() { }

        [Inject]
        internal HeckCustomDataCodec(SiraLog log)
        {
            _log = log;
        }

        public string Id => "Heck";

        public void DeserializeEarly(CustomDataCodecContext ctx)
        {
            CustomData? beatmapCustomData =
                ctx.Repository?.GetCustomBeatmapSaveData()?.customData
                ?? ctx.Repository?.GetBeatmapData()?.customData;
            if (beatmapCustomData == null)
            {
                return;
            }

            LoadPointDefinitions(beatmapCustomData, ctx);
            LoadEventDefinitions(beatmapCustomData, ctx);
        }

        public void ConvertPointDefinitions(CustomData beatmap, CustomDataCodecContext ctx)
        {
            if (ctx.SourceIsV2 == ctx.TargetIsV2)
            {
                return;
            }

            if (ctx.SourceIsV2)
            {
                var entries = beatmap.Get<List<object>>(V2_POINT_DEFINITIONS);
                if (entries != null)
                {
                    var map = new CustomData();
                    foreach (object raw in entries)
                    {
                        if (raw is not CustomData entry)
                        {
                            continue;
                        }

                        string? name = entry.Get<string>(V2_NAME);
                        object? points = entry.Get<object>(V2_POINTS);
                        if (name == null || points == null)
                        {
                            continue;
                        }

                        map[name] = points;
                    }

                    beatmap.TryRemove(V2_POINT_DEFINITIONS, out _);
                    beatmap[POINT_DEFINITIONS] = map;
                }
            }
            else
            {
                CustomData? map = beatmap.Get<CustomData>(POINT_DEFINITIONS);
                if (map != null)
                {
                    var list = new List<object>();
                    foreach (var pair in map)
                    {
                        list.Add(new CustomData { [V2_NAME] = pair.Key, [V2_POINTS] = pair.Value });
                    }

                    beatmap.TryRemove(POINT_DEFINITIONS, out _);
                    beatmap[V2_POINT_DEFINITIONS] = list;
                }
            }
        }

        public IObjectCustomData? Deserialize(
            BaseEditorData obj,
            CustomData json,
            CustomDataCodecContext ctx
        )
        {
            if (json == null)
            {
                return null;
            }

            return new EditorHeckObjectData(json, ctx.Tracks, ctx.SourceIsV2);
        }

        public void Serialize(
            BaseEditorData obj,
            IObjectCustomData typed,
            CustomData json,
            CustomDataCodecContext ctx
        )
        {
            Convert(json, ctx);
        }

        public void Convert(CustomData json, CustomDataCodecContext ctx)
        {
            if (ctx.SourceIsV2 == ctx.TargetIsV2)
            {
                return;
            }

            CustomDataKeyMapper.RemapKeys(json, ctx.SourceIsV2 ? ObjectV2ToV3 : ObjectV3ToV2);
        }

        public ICustomEventCustomData? Deserialize(
            CustomEventEditorData evt,
            CustomData json,
            CustomDataCodecContext ctx
        )
        {
            bool v2 = evt.version2_6_0AndEarlier;
            switch (evt.eventType)
            {
                case ANIMATE_TRACK:
                case ASSIGN_PATH_ANIMATION:
                    return _log == null
                        ? null
                        : new EditorCoroutineEventData(
                            _log,
                            evt,
                            ctx.PointDefinitions,
                            ctx.Tracks,
                            v2
                        );
                case INVOKE_EVENT:
                    return v2 ? null : new EditorInvokeEventData(evt);
                default:
                    return null;
            }
        }

        public void Serialize(
            CustomEventEditorData evt,
            ICustomEventCustomData typed,
            CustomData json,
            CustomDataCodecContext ctx
        )
        {
            ConvertCustomEvent(json, ctx);
        }

        void ICustomEventCustomDataCodec.Convert(CustomData json, CustomDataCodecContext ctx)
        {
            ConvertCustomEvent(json, ctx);
        }

        private static void ConvertCustomEvent(CustomData json, CustomDataCodecContext ctx)
        {
            if (ctx.SourceIsV2 == ctx.TargetIsV2)
            {
                return;
            }

            CustomDataKeyMapper.RemapKeys(json, ctx.SourceIsV2 ? EventV2ToV3 : EventV3ToV2);
        }

        private static void LoadPointDefinitions(
            CustomData beatmapCustomData,
            CustomDataCodecContext ctx
        )
        {
            if (ctx.SourceIsV2)
            {
                var raw = beatmapCustomData.Get<List<object>>(V2_POINT_DEFINITIONS);
                if (raw == null)
                {
                    return;
                }

                foreach (object item in raw)
                {
                    if (item is not CustomData def)
                    {
                        continue;
                    }

                    string? name = def.Get<string>(V2_NAME);
                    var points = def.Get<List<object>>(V2_POINTS);
                    if (name == null || points == null || ctx.PointDefinitions.ContainsKey(name))
                    {
                        continue;
                    }

                    ctx.PointDefinitions.Add(name, points);
                }
            }
            else
            {
                CustomData? map = beatmapCustomData.Get<CustomData>(POINT_DEFINITIONS);
                if (map == null)
                {
                    return;
                }

                foreach ((string key, object? value) in map)
                {
                    if (value is not List<object> points || ctx.PointDefinitions.ContainsKey(key))
                    {
                        continue;
                    }

                    ctx.PointDefinitions.Add(key, points);
                }
            }
        }

        private static void LoadEventDefinitions(
            CustomData beatmapCustomData,
            CustomDataCodecContext ctx
        )
        {
            if (ctx.SourceIsV2)
            {
                return;
            }

            var raw = beatmapCustomData.Get<List<object>>(EVENT_DEFINITIONS);
            if (raw == null)
            {
                return;
            }

            foreach (object item in raw)
            {
                if (item is not CustomData def)
                {
                    continue;
                }

                string? name = def.Get<string>(NAME);
                string? type = def.Get<string>(TYPE);
                CustomData? data = def.Get<CustomData>("data");
                if (
                    name == null
                    || type == null
                    || data == null
                    || ctx.EventDefinitions.ContainsKey(name)
                )
                {
                    continue;
                }

                ctx.EventDefinitions.Add(name, new CustomEventData(-1, type, data, null));
            }
        }
    }
}
