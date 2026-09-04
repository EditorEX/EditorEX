namespace EditorEX.Heck.Codecs
{
    internal interface IEarlyCustomDataCodec
    {
        string Id { get; }

        void DeserializeEarly(CustomDataCodecContext ctx);
    }
}
