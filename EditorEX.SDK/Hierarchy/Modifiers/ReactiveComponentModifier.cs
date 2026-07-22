using Reactive;

namespace EditorEX.SDK.Hierarchy.Modifiers
{
    public class ReactiveComponentModifier(ReactiveComponent component) : IModifier
    {
        public void Apply(ITraversable node)
        {
            component.Use(node.Get());
        }
    }
}
