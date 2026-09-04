using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using Heck.Deserialize;

namespace EditorEX.Heck.Codecs
{
    internal interface IObjectCustomDataCodec
    {
        string Id { get; }

        IObjectCustomData? Deserialize(
            BaseEditorData obj,
            CustomData json,
            CustomDataCodecContext ctx
        );

        void Serialize(
            BaseEditorData obj,
            IObjectCustomData typed,
            CustomData json,
            CustomDataCodecContext ctx
        );

        void Convert(CustomData json, CustomDataCodecContext ctx);
    }
}
