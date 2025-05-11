namespace EditorEX.HierarchyTraverser.Modifiers
{
    public interface IModifier
    {
        void Apply(ITraversable node);
    }
}