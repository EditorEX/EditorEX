using CustomJSONData.CustomBeatmap;
using EditorEX.CustomJSONData.CustomEvents;
using Heck.Deserialize;

namespace EditorEX.Heck.Codecs
{
    internal interface ICustomEventCustomDataCodec
    {
        string Id { get; }

        ICustomEventCustomData? Deserialize(
            CustomEventEditorData evt,
            CustomData json,
            CustomDataCodecContext ctx
        );

        void Serialize(
            CustomEventEditorData evt,
            ICustomEventCustomData typed,
            CustomData json,
            CustomDataCodecContext ctx
        );

        void Convert(CustomData json, CustomDataCodecContext ctx);
    }
}
