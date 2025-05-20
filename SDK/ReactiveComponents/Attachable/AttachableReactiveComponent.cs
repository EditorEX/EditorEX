using System.Collections.Generic;
using Reactive;

namespace EditorEX.SDK.ReactiveComponents.Attachable
{
    public abstract class AttachableReactiveComponent<T> : ReactiveComponent, ICanAttach where T : AttachableReactiveComponent<T>
    {
        public IReadOnlyList<IAttachable> Attachables => _attachables;

        private readonly List<IAttachable> _attachables = new List<IAttachable>();
        private bool _isAttached;

        public T Attach<TAttachable, TValue>(TValue value) where TAttachable : IAttachable<TValue>, new()
        {
            return (this as T)!.Attach<T, TAttachable, TValue>(value);
        }

        public T Attach<TAttachable>(string value) where TAttachable : IAttachable<string>, new()
        {
            return (this as T)!.Attach<T, TAttachable, string>(value);
        }

        public T Attach<TAttachable>() where TAttachable : IAttachable, new()
        {
            return (this as T)!.Attach<T, TAttachable>();
        }

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