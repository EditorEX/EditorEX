namespace EditorEX.SDK.AddressableHelpers
{
    public interface IAddressableCollectorItem
    {
        string Label { get; }

        string Key { get; }

        UnityEngine.Object InternalValue { get; set; }
    }
}
