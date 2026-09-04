using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Heck.Codecs;
using EditorEX.NoodleExtensions.ObjectData;
using Heck;
using Heck.Deserialize;
using NoodleExtensions;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

namespace EditorEX.NoodleExtensions.Codecs
{
    internal class NoodleCustomDataCodec
        : IEarlyCustomDataCodec,
            IObjectCustomDataCodec,
            ICustomEventCustomDataCodec
    {
        private static readonly Dictionary<string, string> ObjectV2ToV3 = new()
        {
            [V2_POSITION] = NOTE_OFFSET,
            [V2_ROTATION] = WORLD_ROTATION,
            [V2_LOCAL_ROTATION] = LOCAL_ROTATION,
            [V2_ANIMATION] = ANIMATION,
            [V2_TRACK] = TRACK,
            [V2_FAKE_NOTE] = INTERNAL_FAKE_NOTE,
            [V2_NOTE_JUMP_SPEED] = NOTE_JUMP_SPEED,
            [V2_NOTE_SPAWN_OFFSET] = NOTE_SPAWN_OFFSET,
            [V2_FLIP] = FLIP,
            [V2_NOTE_GRAVITY_DISABLE] = NOTE_GRAVITY_DISABLE,
            [V2_NOTE_LOOK_DISABLE] = NOTE_LOOK_DISABLE,
            [V2_TIME] = TIME,
            ["_scale"] = OBSTACLE_SIZE,
        };

        private static readonly Dictionary<string, string> AnimationV2ToV3 = new()
        {
            [V2_POSITION] = OFFSET_POSITION,
            [V2_ROTATION] = OFFSET_ROTATION,
            [V2_SCALE] = SCALE,
            [V2_LOCAL_ROTATION] = LOCAL_ROTATION,
            [V2_DISSOLVE] = DISSOLVE,
            [V2_DISSOLVE_ARROW] = DISSOLVE_ARROW,
            [V2_CUTTABLE] = INTERACTABLE,
            [V2_DEFINITE_POSITION] = DEFINITE_POSITION,
        };

        private static readonly Dictionary<string, string> CustomEventV2ToV3 = new()
        {
            [V2_PARENT_TRACK] = PARENT_TRACK,
            [V2_CHILDREN_TRACKS] = CHILDREN_TRACKS,
            [V2_TRACK] = TRACK,
            [V2_PLAYER_TRACK_OBJECT] = PLAYER_TRACK_OBJECT,
        };

        private static readonly Dictionary<string, string> ObjectV3ToV2 =
            CustomDataKeyMapper.InvertMap(ObjectV2ToV3);
        private static readonly Dictionary<string, string> AnimationV3ToV2 =
            CustomDataKeyMapper.InvertMap(AnimationV2ToV3);
        private static readonly Dictionary<string, string> CustomEventV3ToV2 =
            CustomDataKeyMapper.InvertMap(CustomEventV2ToV3);

        public string Id => NoodleController.ID;

        public void DeserializeEarly(CustomDataCodecContext ctx)
        {
            if (ctx.Repository == null)
            {
                return;
            }

            foreach (
                CustomEventEditorData customEventEditorData in ctx.Repository.GetCustomEvents()
            )
            {
                bool v2 = customEventEditorData.version2_6_0AndEarlier;
                string eventType = customEventEditorData.eventType;
                CustomData data = customEventEditorData.customData;
                if (eventType == ASSIGN_TRACK_PARENT)
                {
                    ctx.TrackBuilder.AddFromCustomData(data, v2 ? V2_PARENT_TRACK : PARENT_TRACK);
                }
                else if (eventType == ASSIGN_PLAYER_TO_TRACK)
                {
                    ctx.TrackBuilder.AddFromCustomData(data, v2);
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
                ObstacleEditorData obstacle => new EditorNoodleObstacleData(
                    obstacle,
                    json,
                    ctx.PointDefinitions,
                    ctx.Tracks,
                    v2,
                    ctx.LeftHanded
                ),
                NoteEditorData note => new EditorNoodleNoteData(
                    note,
                    json,
                    ctx.PointDefinitions,
                    ctx.Tracks,
                    v2,
                    ctx.LeftHanded
                ),
                ChainEditorData chain => new EditorNoodleSliderData(
                    chain,
                    json,
                    ctx.PointDefinitions,
                    ctx.Tracks,
                    v2,
                    ctx.LeftHanded
                ),
                ArcEditorData arc => new EditorNoodleSliderData(
                    arc,
                    json,
                    ctx.PointDefinitions,
                    ctx.Tracks,
                    v2,
                    ctx.LeftHanded
                ),
                _ => new EditorNoodleObjectData(
                    obj,
                    json,
                    ctx.PointDefinitions,
                    ctx.Tracks,
                    v2,
                    ctx.LeftHanded
                ),
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
            if (ctx.SourceIsV2 == ctx.TargetIsV2)
            {
                return;
            }

            if (ctx.SourceIsV2 && !ctx.TargetIsV2)
            {
                CustomDataKeyMapper.RemapKeys(json, ObjectV2ToV3);
                CustomDataKeyMapper.InvertBoolean(json, V2_CUTTABLE, UNINTERACTABLE, invert: true);
                CustomDataKeyMapper.RemapNested(json, ANIMATION, AnimationV2ToV3);
            }
            else
            {
                CustomDataKeyMapper.RemapNested(json, ANIMATION, AnimationV3ToV2);
                CustomDataKeyMapper.RemapKeys(json, ObjectV3ToV2);
                CustomDataKeyMapper.InvertBoolean(json, UNINTERACTABLE, V2_CUTTABLE, invert: true);
            }
        }

        public ICustomEventCustomData? Deserialize(
            CustomEventEditorData evt,
            CustomData json,
            CustomDataCodecContext ctx
        )
        {
            bool v2 = evt.version2_6_0AndEarlier;
            return evt.eventType switch
            {
                ASSIGN_TRACK_PARENT => new NoodleParentTrackEventData(json, ctx.Tracks, v2),
                ASSIGN_PLAYER_TO_TRACK => new NoodlePlayerTrackEventData(json, ctx.Tracks, v2),
                _ => null,
            };
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

        internal static void ConvertCustomEvent(CustomData json, CustomDataCodecContext ctx)
        {
            if (ctx.SourceIsV2 == ctx.TargetIsV2)
            {
                return;
            }

            CustomDataKeyMapper.RemapKeys(
                json,
                ctx.SourceIsV2 ? CustomEventV2ToV3 : CustomEventV3ToV2
            );
        }
    }
}
