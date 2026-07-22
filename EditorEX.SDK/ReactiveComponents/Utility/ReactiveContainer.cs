using EditorEX.SDK.Collectors;
using Zenject;

namespace EditorEX.SDK.ReactiveComponents
{
    public class ReactiveContainer : IReactiveContainer
    {
        public ReactiveContainer(
            IColorCollector colorCollector,
            IFontCollector fontCollector,
            ITransitionCollector transitionCollector,
            IInstantiator instantiator
        )
        {
            ColorCollector = colorCollector;
            FontCollector = fontCollector;
            TransitionCollector = transitionCollector;
            Instantiator = instantiator;
        }

        public IColorCollector ColorCollector { get; }
        public IFontCollector FontCollector { get; }
        public ITransitionCollector TransitionCollector { get; }
        public IInstantiator Instantiator { get; }
    }
}
