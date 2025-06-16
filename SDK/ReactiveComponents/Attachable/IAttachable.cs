using Reactive;

namespace EditorEX.SDK.ReactiveComponents.Attachable
{
    public interface IAttachable<T> : IAttachable
    {
        void SetValue(T value);
    }

    public interface IAttachable
    {
        void Attach(ReactiveComponent component);
    }
}
