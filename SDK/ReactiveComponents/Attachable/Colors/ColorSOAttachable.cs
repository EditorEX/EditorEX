using EditorEX.SDK.ReactiveComponents.Native;
using Reactive;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents.Attachable
{
    public class ColorSOAttachable : IAttachable<string>
    {
        public ColorSOAttachable()
        {
        }

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
            Debug.Log($"Attaching ColorSOAttachable to {component.GetType().Name}");
            if (component is IColorSOAttachable colorSOAttachable)
            {
                var container = component.Content.transform.GetComponentInParent<ReactiveContainerHolder>().ReactiveContainer;
                colorSOAttachable.ColorSO = container.ColorCollector.GetColor(ColorSource);
            }
        }
    }
}