namespace EditorEX.SDK.ReactiveComponents.Attachable
{
    public static class AttachableExtensions
    {
        public static AttachableReactiveComponent Attach<TAttachable, T>(this AttachableReactiveComponent component, T value) where TAttachable : IAttachable<T>, new()
        {
            var attachable = new TAttachable();
            attachable.SetValue(value);
            component.AddAttachable(attachable);
            return component;
        }

        public static AttachableReactiveComponent Attach<TAttachable>(this AttachableReactiveComponent component, string value) where TAttachable : IAttachable<string>, new()
        {
            var attachable = new TAttachable();
            attachable.SetValue(value);
            component.AddAttachable(attachable);
            return component;
        }

        public static AttachableReactiveComponent Attach<TAttachable>(this AttachableReactiveComponent component) where TAttachable : IAttachable, new()
        {
            var attachable = new TAttachable();
            component.AddAttachable(attachable);
            return component;
        }
    }
}