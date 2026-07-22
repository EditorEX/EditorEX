using EditorEX.SDK.Signals;

namespace EditorEX.SDK.Collectors
{
    public interface IAddressableCollector
    {
        bool CheckAvailability(string label);
        T GetObject<T>(string label)
            where T : UnityEngine.Object;
        IAddressableCollectorItemLoadedSignal GetSignal(string label);
    }
}
