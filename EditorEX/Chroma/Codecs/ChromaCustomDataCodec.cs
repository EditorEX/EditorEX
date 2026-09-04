using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapEditor3D.DataModels;
using Chroma;
using CustomJSONData.CustomBeatmap;
using EditorEX.Chroma.Events;
using EditorEX.Chroma.Lighting;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Heck.Codecs;
using EditorEX.Heck.Deserialize;
using Heck.Animation;
using Heck.Deserialize;
using static EditorEX.Chroma.Constants;

namespace EditorEX.Chroma.Codecs
{
    internal class ChromaCustomDataCodec
        : IEarlyCustomDataCodec,
            IObjectCustomDataCodec,
            IEventCustomDataCodec,
            ICustomEventCustomDataCodec,
            IEventListCustomDataCodec
    {
        private static readonly Dictionary<string, string> V2ToV3 = new()
        {
            [V2_COLOR] = COLOR,
            [V2_DIRECTION] = DIRECTION,
            [V2_LERP_TYPE] = LERP_TYPE,
            [V2_LIGHT_ID] = LIGHT_ID,
            [V2_LOCK_POSITION] = LOCK_POSITION,
            [V2_NAME_FILTER] = NAME_FILTER,
            [V2_PROP] = PROP,
            [V2_SPEED] = SPEED,
            [V2_STEP] = STEP,
            [V2_ENVIRONMENT] = ENVIRONMENT,
            [V2_GEOMETRY] = GEOMETRY,
            [V2_MATERIAL] = MATERIAL,
            [V2_MATERIALS] = MATERIALS,
            [V2_GAMEOBJECT_ID] = GAMEOBJECT_ID,
            [V2_LOOKUP_METHOD] = LOOKUP_METHOD,
            [V2_DUPLICATION_AMOUNT] = DUPLICATION_AMOUNT,
            [V2_ACTIVE] = ACTIVE,
            [V2_GEOMETRY_TYPE] = GEOMETRY_TYPE,
            [V2_SHADER_PRESET] = SHADER_PRESET,
            [V2_SHADER_KEYWORDS] = SHADER_KEYWORDS,
            [V2_COLLISION] = COLLISION,
        };

        private static readonly Dictionary<string, string> V3ToV2 = CustomDataKeyMapper.InvertMap(
            V2ToV3
        );

        public string Id => "Chroma";

        public void DeserializeEarly(CustomDataCodecContext ctx)
        {
            if (ctx.Repository == null || ctx.TrackBuilder == null)
            {
                return;
            }

            bool v2 = ctx.SourceIsV2;
            CustomData? beatmapData = ctx.Repository.GetBeatmapData()?.customData;
            if (beatmapData == null)
            {
                return;
            }

            IEnumerable<CustomData>? environmentData = beatmapData
                .Get<List<object>>(v2 ? V2_ENVIRONMENT : ENVIRONMENT)
                ?.Cast<CustomData>();
            if (environmentData != null)
            {
                foreach (CustomData gameObjectData in environmentData)
                {
                    ctx.TrackBuilder.AddManyFromCustomData(gameObjectData, v2, false);
                    CustomData? geometryData = gameObjectData.Get<CustomData>(
                        v2 ? V2_GEOMETRY : GEOMETRY
                    );
                    object? materialData = geometryData?.Get<object>(v2 ? V2_MATERIAL : MATERIAL);
                    if (materialData is CustomData materialCustomData)
                    {
                        ctx.TrackBuilder.AddFromCustomData(materialCustomData, v2, false);
                    }
                }
            }

            CustomData? materialsData = beatmapData.Get<CustomData>(v2 ? V2_MATERIALS : MATERIALS);
            if (materialsData != null)
            {
                foreach ((string _, object? value) in materialsData)
                {
                    if (value is CustomData material)
                    {
                        ctx.TrackBuilder.AddFromCustomData(material, v2, false);
                    }
                }
            }

            if (!v2)
            {
                return;
            }

            foreach (CustomEventEditorData customEventData in ctx.Repository.GetCustomEvents())
            {
                if (customEventData.eventType == ASSIGN_FOG_TRACK)
                {
                    ctx.TrackBuilder.AddFromCustomData(customEventData.customData, v2);
                }
            }
        }

        public IObjectCustomData? Deserialize(
            BaseEditorData obj,
            CustomData json,
            CustomDataCodecContext ctx
        )
        {
            bool v2 = ctx.SourceIsV2;
            return obj switch
            {
                NoteEditorData or ChainEditorData or ArcEditorData => new ChromaNoteData(
                    json,
                    ctx.Tracks,
                    ctx.PointDefinitions,
                    v2
                ),
                ObstacleEditorData => new ChromaObjectData(
                    json,
                    ctx.Tracks,
                    ctx.PointDefinitions,
                    v2
                ),
                _ => null,
            };
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
            ConvertJson(json, ctx);
        }

        public IEventCustomData? Deserialize(
            BasicEventEditorData evt,
            CustomData json,
            CustomDataCodecContext ctx
        )
        {
            if (ctx.Repository == null)
            {
                return null;
            }

            var helper = ctx.Extra as EditorLegacyLightHelper;
            return new EditorChromaEventData(evt, helper, ctx.SourceIsV2, ctx.Repository);
        }

        public void Serialize(
            BasicEventEditorData evt,
            IEventCustomData typed,
            CustomData json,
            CustomDataCodecContext ctx
        )
        {
            ConvertJson(json, ctx);
        }

        void IEventCustomDataCodec.Convert(CustomData json, CustomDataCodecContext ctx)
        {
            ConvertJson(json, ctx);
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
                case ASSIGN_FOG_TRACK:
                    return v2 ? new ChromaAssignFogEventData(json.GetTrack(ctx.Tracks, v2)) : null;
                case ANIMATE_COMPONENT:
                    return v2
                        ? null
                        : new ChromaAnimateComponentData(json, ctx.Tracks, ctx.PointDefinitions);
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
            ConvertJson(json, ctx);
        }

        void ICustomEventCustomDataCodec.Convert(CustomData json, CustomDataCodecContext ctx)
        {
            ConvertJson(json, ctx);
        }

        public void PrepareEvents(
            IReadOnlyList<BasicEventEditorData> events,
            CustomDataCodecContext ctx
        )
        {
            if (ctx.SourceIsV2)
            {
                ctx.Extra = new EditorLegacyLightHelper(events);
            }
        }

        public void LinkEvents(
            IReadOnlyList<BasicEventEditorData> events,
            EditorDeserializedData cache
        )
        {
            var allNextSameTypes = new Dictionary<int, Dictionary<int, BasicEventEditorData>>();
            for (int i = events.Count - 1; i >= 0; i--)
            {
                BasicEventEditorData beatmapEventData = events[i];
                if (
                    !cache.Resolve(beatmapEventData, out EditorChromaEventData? currentEventData)
                    || currentEventData == null
                )
                {
                    continue;
                }

                int type = (int)beatmapEventData.type;
                if (
                    !allNextSameTypes.TryGetValue(
                        type,
                        out Dictionary<int, BasicEventEditorData> nextSameTypes
                    )
                )
                {
                    allNextSameTypes[type] = nextSameTypes =
                        new Dictionary<int, BasicEventEditorData>();
                }

                currentEventData.NextSameTypeEvent =
                    currentEventData.NextSameTypeEvent
                    ?? new Dictionary<int, BasicEventEditorData>(nextSameTypes);
                IEnumerable<int>? ids = currentEventData.LightID;
                if (ids == null)
                {
                    nextSameTypes[-1] = beatmapEventData;
                    foreach (int key in nextSameTypes.Keys.ToArray())
                    {
                        nextSameTypes[key] = beatmapEventData;
                    }
                }
                else
                {
                    foreach (int id in ids)
                    {
                        nextSameTypes[id] = beatmapEventData;
                    }
                }
            }
        }

        internal void ConvertBeatmapCustomData(CustomData beatmap, CustomDataCodecContext ctx)
        {
            ConvertJson(beatmap, ctx);
            CustomDataKeyMapper.RemapNested(
                beatmap,
                ctx.TargetIsV2 ? V2_ENVIRONMENT : ENVIRONMENT,
                ctx.SourceIsV2 ? V2ToV3 : V3ToV2
            );
        }

        private static void ConvertJson(CustomData json, CustomDataCodecContext ctx)
        {
            if (ctx.SourceIsV2 == ctx.TargetIsV2)
            {
                return;
            }

            CustomDataKeyMapper.RemapKeys(json, ctx.SourceIsV2 ? V2ToV3 : V3ToV2);
        }
    }
}
