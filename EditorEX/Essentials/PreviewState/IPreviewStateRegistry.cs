namespace EditorEX.Essentials.PreviewState
{
    internal interface IPreviewStateRegistry
    {
        void Add(
            float fromBeat,
            float toBeat,
            IPreviewStateAction action,
            bool alreadyExecuted = false
        );

        void Refresh();
    }
}
