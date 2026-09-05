using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Heck.Deserialize;
using EditorEX.Util;
using Heck.Animation;
using Heck.Deserialize;
using SiraUtil.Logging;
using Zenject;

namespace EditorEX.Heck.Codecs
{
    internal class CustomDataCodecRegistry
    {
        private readonly List<IEarlyCustomDataCodec> _early;
        private readonly List<IObjectCustomDataCodec> _objects;
        private readonly List<IEventCustomDataCodec> _events;
        private readonly List<ICustomEventCustomDataCodec> _customEvents;
        private readonly List<IEventListCustomDataCodec> _eventLists;
        private readonly Dictionary<string, EditorDeserializedData> _caches;
        private readonly Dictionary<string, Track> _tracks;
        private SiraLog? _log;

        internal CustomDataCodecRegistry(
            List<IEarlyCustomDataCodec> early,
            List<IObjectCustomDataCodec> objects,
            List<IEventCustomDataCodec> events,
            List<ICustomEventCustomDataCodec> customEvents,
            List<IEventListCustomDataCodec> eventLists,
            [Inject(Id = "Heck")] EditorDeserializedData heck,
            [Inject(Id = "NoodleExtensions")] EditorDeserializedData noodle,
            [Inject(Id = "Chroma")] EditorDeserializedData chroma,
            [Inject(Id = "Vivify")] EditorDeserializedData vivify,
            Dictionary<string, Track> tracks
        )
        {
            _early = early;
            _objects = objects;
            _events = events;
            _customEvents = customEvents;
            _eventLists = eventLists;
            _tracks = tracks;
            _caches = new Dictionary<string, EditorDeserializedData>
            {
                ["Heck"] = heck,
                ["NoodleExtensions"] = noodle,
                ["Chroma"] = chroma,
                ["Vivify"] = vivify,
            };
        }

        [Inject]
        private void InjectLog(SiraLog log)
        {
            _log = log;
        }

        internal void Clear()
        {
            foreach (EditorDeserializedData cache in _caches.Values)
            {
                cache.Clear();
            }

            _tracks.Clear();
        }

        internal void ConvertJson(CustomData json, CustomDataCodecContext ctx)
        {
            foreach (IObjectCustomDataCodec codec in _objects)
            {
                codec.Convert(json, ctx);
            }
        }

        internal void LoadMap(
            BeatmapObjectsDataModel objectsModel,
            BeatmapBasicEventsDataModel eventsModel,
            CustomDataCodecContext ctx
        )
        {
            foreach (EditorDeserializedData cache in _caches.Values)
            {
                cache.Clear();
            }

            _tracks.Clear();
            ctx.Tracks = _tracks;
            ctx.TrackBuilder ??= new TrackBuilder();
            ctx.PointDefinitions ??= new Dictionary<string, List<object>>();

            CollectTracks(objectsModel, eventsModel, ctx);
            foreach (IEarlyCustomDataCodec codec in _early)
            {
                try
                {
                    codec.DeserializeEarly(ctx);
                }
                catch (Exception e)
                {
                    Log(e);
                }
            }

            foreach (KeyValuePair<string, Track> pair in ctx.TrackBuilder.Tracks)
            {
                _tracks[pair.Key] = pair.Value;
            }

            IReadOnlyList<BasicEventEditorData> eventList = eventsModel.GetAllEventsAsList();
            foreach (IEventListCustomDataCodec codec in _eventLists)
            {
                codec.PrepareEvents(eventList, ctx);
            }

            foreach (BaseEditorData obj in objectsModel.allBeatmapObjects)
            {
                DeserializeObject(obj, ctx);
            }

            foreach (BasicEventEditorData evt in eventList)
            {
                DeserializeEvent(evt, ctx);
            }

            foreach (IEventListCustomDataCodec codec in _eventLists)
            {
                if (_caches.TryGetValue(codec.Id, out EditorDeserializedData cache))
                {
                    codec.LinkEvents(eventList, cache);
                }
            }

            if (ctx.Repository == null)
            {
                return;
            }

            foreach (CustomEventEditorData customEvent in ctx.Repository.GetCustomEvents())
            {
                DeserializeCustomEvent(customEvent, ctx);
            }

            Log(
                $"Custom data codecs loaded. Heck={CountObjects("Heck")} Noodle={CountObjects("NoodleExtensions")} Chroma={CountObjects("Chroma")} Vivify={CountObjects("Vivify")} tracks={_tracks.Count} pointDefs={ctx.PointDefinitions.Count}."
            );
        }

        private int CountObjects(string id)
        {
            return _caches.TryGetValue(id, out EditorDeserializedData cache)
                ? cache.ObjectCount
                : 0;
        }

        internal void DeserializeObject(BaseEditorData obj, CustomDataCodecContext ctx)
        {
            CustomData json = obj.GetCustomData(ctx.Repository);
            if (json == null)
            {
                return;
            }

            foreach (IObjectCustomDataCodec codec in _objects)
            {
                try
                {
                    IObjectCustomData? typed = codec.Deserialize(obj, json, ctx);
                    if (
                        typed != null
                        && _caches.TryGetValue(codec.Id, out EditorDeserializedData cache)
                    )
                    {
                        cache.SetObject(obj, typed);
                    }
                }
                catch (Exception e)
                {
                    Log(e);
                }
            }
        }

        internal void DeserializeEvent(BasicEventEditorData evt, CustomDataCodecContext ctx)
        {
            CustomData json = evt.GetCustomData(ctx.Repository);
            foreach (IEventCustomDataCodec codec in _events)
            {
                try
                {
                    IEventCustomData? typed = codec.Deserialize(evt, json, ctx);
                    if (
                        typed != null
                        && _caches.TryGetValue(codec.Id, out EditorDeserializedData cache)
                    )
                    {
                        cache.SetEvent(evt, typed);
                    }
                }
                catch (Exception e)
                {
                    Log(e);
                }
            }
        }

        internal void DeserializeCustomEvent(CustomEventEditorData evt, CustomDataCodecContext ctx)
        {
            CustomData json = evt.customData;
            foreach (ICustomEventCustomDataCodec codec in _customEvents)
            {
                try
                {
                    ICustomEventCustomData? typed = codec.Deserialize(evt, json, ctx);
                    if (
                        typed != null
                        && _caches.TryGetValue(codec.Id, out EditorDeserializedData cache)
                    )
                    {
                        cache.SetCustomEvent(evt, typed);
                    }
                }
                catch (Exception e)
                {
                    Log(e);
                }
            }
        }

        internal void ConvertMap(
            BeatmapObjectsDataModel objectsModel,
            BeatmapBasicEventsDataModel eventsModel,
            CustomDataCodecContext ctx
        )
        {
            CustomData? beatmap =
                ctx.Repository?.GetBeatmapData()?.customData
                ?? ctx.Repository?.GetCustomBeatmapSaveData()?.customData;
            if (beatmap != null)
            {
                foreach (IEarlyCustomDataCodec codec in _early)
                {
                    if (codec is HeckCustomDataCodec heck)
                    {
                        heck.ConvertPointDefinitions(beatmap, ctx);
                    }
                }
            }

            foreach (BaseEditorData obj in objectsModel.allBeatmapObjects)
            {
                CustomData json = obj.GetCustomData(ctx.Repository);
                if (json == null)
                {
                    continue;
                }

                ConvertJson(json, ctx);
                foreach (IEventCustomDataCodec codec in _events)
                {
                    codec.Convert(json, ctx);
                }
            }

            foreach (BasicEventEditorData evt in eventsModel.GetAllEventsAsList())
            {
                CustomData json = evt.GetCustomData(ctx.Repository);
                if (json == null)
                {
                    continue;
                }

                foreach (IEventCustomDataCodec codec in _events)
                {
                    codec.Convert(json, ctx);
                }
            }

            if (ctx.Repository != null)
            {
                foreach (CustomEventEditorData customEvent in ctx.Repository.GetCustomEvents())
                {
                    foreach (ICustomEventCustomDataCodec codec in _customEvents)
                    {
                        codec.Convert(customEvent.customData, ctx);
                    }
                }
            }

            ctx.SourceVersion = ctx.TargetVersion;
            LoadMap(objectsModel, eventsModel, ctx);
        }

        internal void ConvertObject(BaseEditorData obj, CustomDataCodecContext ctx)
        {
            CustomData json = obj.GetCustomData(ctx.Repository);
            if (json == null)
            {
                return;
            }

            ConvertJson(json, ctx);
            var loadCtx = ctx;
            loadCtx.SourceVersion = ctx.TargetVersion;
            DeserializeObject(obj, loadCtx);
        }

        private void CollectTracks(
            BeatmapObjectsDataModel objectsModel,
            BeatmapBasicEventsDataModel eventsModel,
            CustomDataCodecContext ctx
        )
        {
            if (ctx.Repository == null)
            {
                return;
            }

            IEnumerable<BaseEditorData?> datas = objectsModel
                .allBeatmapObjects.Cast<BaseEditorData>()
                .Concat(eventsModel.GetAllEventsAsList().Cast<BaseEditorData>())
                .Concat(ctx.Repository.GetCustomEvents());

            foreach (BaseEditorData? baseEditorData in datas)
            {
                try
                {
                    CustomData customData = ctx.Repository.GetCustomData(baseEditorData);
                    if (customData == null && baseEditorData is CustomEventEditorData customEvent)
                    {
                        customData = customEvent.customData;
                    }

                    if (customData == null)
                    {
                        continue;
                    }

                    object? trackNameRaw = customData.Get<object>(
                        ctx.SourceIsV2
                            ? EditorEX.Heck.Constants.V2_TRACK
                            : EditorEX.Heck.Constants.TRACK
                    );
                    if (trackNameRaw == null)
                    {
                        continue;
                    }

                    IEnumerable<string> trackNames;
                    if (trackNameRaw is List<object> listTrack)
                    {
                        trackNames = listTrack.Select(x => (string)x);
                    }
                    else
                    {
                        trackNames = new[] { (string)trackNameRaw };
                    }

                    foreach (string trackName in trackNames)
                    {
                        ctx.TrackBuilder!.AddTrack(trackName);
                    }
                }
                catch (Exception e)
                {
                    Log(e);
                }
            }
        }

        private void Log(Exception e)
        {
            if (_log != null)
            {
                _log.Error(e);
                return;
            }

            Plugin.Logger?.Error(e.ToString());
        }

        private void Log(string message)
        {
            if (_log != null)
            {
                _log.Info(message);
                return;
            }

            Plugin.Logger?.Info(message);
        }
    }
}
