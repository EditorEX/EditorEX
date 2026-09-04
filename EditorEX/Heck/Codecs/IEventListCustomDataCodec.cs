using System.Collections.Generic;
using BeatmapEditor3D.DataModels;
using EditorEX.Heck.Deserialize;

namespace EditorEX.Heck.Codecs
{
    internal interface IEventListCustomDataCodec
    {
        string Id { get; }

        void PrepareEvents(IReadOnlyList<BasicEventEditorData> events, CustomDataCodecContext ctx);

        void LinkEvents(IReadOnlyList<BasicEventEditorData> events, EditorDeserializedData cache);
    }
}
