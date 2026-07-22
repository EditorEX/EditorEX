namespace EditorEX.SDK.Collectors
{
    public interface ITransitionCollector
    {
        T GetTransition<T>(string name)
            where T : BaseTransitionSO;
    }
}
