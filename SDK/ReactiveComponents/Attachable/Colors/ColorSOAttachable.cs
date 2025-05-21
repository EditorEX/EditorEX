using EditorEX.SDK.ReactiveComponents.Native;
using Reactive;

namespace EditorEX.SDK.ReactiveComponents.Attachable
{
    public class ColorSOAttachable : IAttachable<string>
    {
        public virtual string ColorSource
        {
            get => _colorSource;
            set
            {
                _colorSource = value;
            }
        }

        private string _colorSource = string.Empty;

        public void SetValue(string value)
        {
            ColorSource = value;
        }

        public void Attach(ReactiveComponent component)
        {
            if (component is IColorSOAttachable colorSOAttachable)
            {
                var container = component.Content.transform.GetComponentInParent<ReactiveContainerHolder>().ReactiveContainer;
                colorSOAttachable.ColorSO = container.ColorCollector.GetColor(ColorSource);
            }
        }
    }
}