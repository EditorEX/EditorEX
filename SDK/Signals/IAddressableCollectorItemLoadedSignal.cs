namespace EditorEX.SDK.Signals
{
    public interface IAddressableCollectorItemLoadedSignal
    {
        public string Label { get; }

        public T GetValue<T>()
            where T : UnityEngine.Object;
    }
}
