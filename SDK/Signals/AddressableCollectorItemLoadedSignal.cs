namespace EditorEX.SDK.Signals
{
    public class AddressableCollectorItemLoadedSignal<T> : IAddressableCollectorItemLoadedSignal where T : UnityEngine.Object
    {
        public string Label { get; }

        public T Object { get; }

        public AddressableCollectorItemLoadedSignal(string label, T _object)
        {
            Label = label;
            Object = _object;
        }

        public U GetValue<U>() where U : UnityEngine.Object
        {
            if (typeof(T) != typeof(U))
            {
                throw new System.ArgumentException("Value is not of type " + typeof(T).Name);
            }
            return Object as U;
        }

        public T GetValue()
        {
            return Object;
        }
    }
}