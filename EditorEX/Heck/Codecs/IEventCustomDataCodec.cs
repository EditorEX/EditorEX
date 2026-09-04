using BeatmapEditor3D.DataModels;
using CustomJSONData.CustomBeatmap;
using Heck.Deserialize;

namespace EditorEX.Heck.Codecs
{
    internal interface IEventCustomDataCodec
    {
        string Id { get; }

        IEventCustomData? Deserialize(
            BasicEventEditorData evt,
            CustomData json,
            CustomDataCodecContext ctx
        );

        void Serialize(
            BasicEventEditorData evt,
            IEventCustomData typed,
            CustomData json,
            CustomDataCodecContext ctx
        );

        void Convert(CustomData json, CustomDataCodecContext ctx);
    }
}
