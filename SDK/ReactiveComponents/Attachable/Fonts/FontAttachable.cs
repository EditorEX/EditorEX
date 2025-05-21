using EditorEX.SDK.ReactiveComponents.Native;
using Reactive;

namespace EditorEX.SDK.ReactiveComponents.Attachable
{
    public class FontAttachable : IAttachable
    {
        public void Attach(ReactiveComponent component)
        {
            if (component is IFontAttachable fontAttachable)
            {
                var container = component.Content.transform.GetComponentInParent<ReactiveContainerHolder>().ReactiveContainer;
                fontAttachable.Material = container.FontCollector.GetMaterial();
                fontAttachable.Font = container.FontCollector.GetFontAsset();
            }
        }
    }
}