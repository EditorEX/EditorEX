using EditorEX.SDK.Collectors;
using Zenject;

namespace EditorEX.SDK.ReactiveComponents
{
    public interface IReactiveContainer
    {
        IColorCollector ColorCollector { get; }
        IFontCollector FontCollector { get; }
        ITransitionCollector TransitionCollector { get; }
        IInstantiator Instantiator { get; }
    }
}
