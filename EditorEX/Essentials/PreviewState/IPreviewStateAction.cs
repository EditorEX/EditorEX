namespace EditorEX.Essentials.PreviewState
{
    internal interface IPreviewStateAction
    {
        void Execute();

        void Reverse();

        void Tick(float beat);
    }
}
