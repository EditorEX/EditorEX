using System.Collections.Generic;
using EditorEX.SDK.ReactiveComponents.Native;
using Reactive;

namespace EditorEX.SDK.ReactiveComponents.Attachable
{
    public abstract class AttachableReactiveComponent : ReactiveComponent
    {
        public IReadOnlyList<IAttachable> Attachables => _attachables;

        private readonly List<IAttachable> _attachables = new List<IAttachable>();
        private bool _isAttached;

        public void AttachAll()
        {
            if (_isAttached)
            {
                return;
            }
            foreach (var attachable in _attachables)
            {
                attachable.Attach(this);
            }
            _isAttached = true;
        }

        public void AddAttachable(IAttachable attachable)
        {
            if (_attachables.Contains(attachable))
            {
                return;
            }
            _attachables.Add(attachable);
            if (_isAttached)
            {
                attachable.Attach(this);
            }
        }

        protected override void OnStart()
        {
            AttachAll();
        }
    }
}