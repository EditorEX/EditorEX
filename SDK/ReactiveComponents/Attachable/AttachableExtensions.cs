namespace EditorEX.SDK.ReactiveComponents.Attachable
{
    public static class AttachableExtensions
    {
        public static T Attach<T, TAttachable, TValue>(this T component, TValue value) where T : ICanAttach where TAttachable : IAttachable<TValue>, new()
        {
            var attachable = new TAttachable();
            attachable.SetValue(value);
            component.AddAttachable(attachable);
            return component;
        }

        public static T Attach<T, TAttachable>(this T component, string value) where T : ICanAttach where TAttachable : IAttachable<string>, new()
        {
            var attachable = new TAttachable();
            attachable.SetValue(value);
            component.AddAttachable(attachable);
            return component;
        }

        public static T Attach<T, TAttachable>(this T component) where T : ICanAttach where TAttachable : IAttachable, new()
        {
            var attachable = new TAttachable();
            component.AddAttachable(attachable);
            return component;
        }
    }
}