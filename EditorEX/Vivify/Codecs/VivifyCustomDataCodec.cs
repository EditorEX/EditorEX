using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData.CustomEvents;
using EditorEX.Heck.Codecs;
using Heck.Deserialize;
using Vivify;
using static Vivify.VivifyController;

namespace EditorEX.Vivify.Codecs
{
    internal class VivifyCustomDataCodec
        : IEarlyCustomDataCodec,
            IObjectCustomDataCodec,
            ICustomEventCustomDataCodec
    {
        public string Id => "Vivify";

        public void DeserializeEarly(CustomDataCodecContext ctx)
        {
            if (ctx.Repository == null || ctx.TrackBuilder == null)
            {
                return;
            }

            foreach (CustomEventEditorData customEventData in ctx.Repository.GetCustomEvents())
            {
                if (customEventData.eventType == INSTANTIATE_PREFAB)
                {
                    ctx.TrackBuilder.AddFromCustomData(customEventData.customData, false, false);
                }
            }
        }

        public IObjectCustomData? Deserialize(
            BaseEditorData obj,
            CustomData json,
            CustomDataCodecContext ctx
        )
        {
            return new VivifyObjectData(json, ctx.Tracks);
        }

        public void Serialize(
            BaseEditorData obj,
            IObjectCustomData typed,
            CustomData json,
            CustomDataCodecContext ctx
        ) { }

        public void Convert(CustomData json, CustomDataCodecContext ctx) { }

        public ICustomEventCustomData? Deserialize(
            CustomEventEditorData evt,
            CustomData json,
            CustomDataCodecContext ctx
        )
        {
            return evt.eventType switch
            {
                APPLY_POST_PROCESSING => new ApplyPostProcessingData(json, ctx.PointDefinitions),
                ASSIGN_OBJECT_PREFAB => new AssignObjectPrefabData(json, ctx.Tracks),
                DECLARE_CULLING_TEXTURE => new CreateCameraData(json, ctx.Tracks),
                DECLARE_TEXTURE => new CreateScreenTextureData(json),
                DESTROY_PREFAB => new DestroyObjectData(json),
                INSTANTIATE_PREFAB => new InstantiatePrefabData(json, ctx.Tracks),
                SET_MATERIAL_PROPERTY => new SetMaterialPropertyData(json, ctx.PointDefinitions),
                SET_GLOBAL_PROPERTY => new SetGlobalPropertyData(json, ctx.PointDefinitions),
                SET_CAMERA_PROPERTY => new SetCameraPropertyData(json, ctx.Tracks),
                SET_ANIMATOR_PROPERTY => new SetAnimatorPropertyData(json, ctx.PointDefinitions),
                SET_RENDERING_SETTINGS => new SetRenderingSettingsData(json, ctx.PointDefinitions),
                _ => null,
            };
        }

        public void Serialize(
            CustomEventEditorData evt,
            ICustomEventCustomData typed,
            CustomData json,
            CustomDataCodecContext ctx
        ) { }

        void ICustomEventCustomDataCodec.Convert(CustomData json, CustomDataCodecContext ctx) { }
    }
}
