using System;
using Reactive.BeatSaber.Components;

namespace EditorEX.SDK.ReactiveComponents
{
    /// <summary>
    /// A ModalBase helper that handles outside-click behavior with optional auto-close.
    /// </summary>
    public abstract class BlockingModalBase : EditorModalBase
    {
        /// <summary>Whether a blocker should be created when the modal opens.</summary>
        public bool UseBlocker { get; set; } = true;

        /// <summary>If true (default) the modal closes when the blocker is clicked.</summary>
        public bool CloseOnBlockerClick { get; set; } = true;

        /// <summary>Raised when the blocker is clicked (before optional auto-close).</summary>
        public event Action? BlockerClickedEvent;

        protected BlockingModalBase()
        {
            OnClickOutside = HandleBlockerClicked;
        }

        private void HandleBlockerClicked()
        {
            if (!UseBlocker)
            {
                return;
            }

            BlockerClickedEvent?.Invoke();
            if (CloseOnBlockerClick)
            {
                IsPushed = false;
            }
        }
    }
}
