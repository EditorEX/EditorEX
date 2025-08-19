using EditorEX.SDK.Collectors;
using Zenject;

namespace EditorEX.SDK.ReactiveComponents
{
    public class ReactiveContainer
    {
        public ReactiveContainer(
            ColorCollector colorCollector,
            FontCollector fontCollector,
            TransitionCollector transitionCollector,
            IInstantiator instantiator
        )
        {
            ColorCollector = colorCollector;
            FontCollector = fontCollector;
            TransitionCollector = transitionCollector;
            Instantiator = instantiator;
        }

        public ColorCollector ColorCollector { get; private set; }
        public FontCollector FontCollector { get; private set; }
        public TransitionCollector TransitionCollector { get; private set; }
        public IInstantiator Instantiator { get; private set; }
    }
}
