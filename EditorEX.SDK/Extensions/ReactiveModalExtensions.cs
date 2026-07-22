using System;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Native;
using Reactive;
using UnityEngine;

namespace EditorEX.SDK.Extensions
{
    public static class ReactiveModalExtensions
    {
        /// <summary>
        /// Presents a modal by attaching it to the nearest composition and pushing it.
        /// </summary>
        /// <param name="modal">The modal component to present</param>
        /// <param name="child">The child transform used to find the parent container</param>
        /// <param name="animated">Unused legacy parameter kept for call-site compatibility</param>
        public static void PresentEditor(
            this EditorModalBase modal,
            Transform child,
            bool animated = true
        )
        {
            var holder = child.GetComponentInParent<ReactiveContainerHolder>();
            if (holder == null)
            {
                throw new InvalidOperationException(
                    "Unable to present modal: no ReactiveContainerHolder found in parent hierarchy."
                );
            }

            var composition = holder.GetComponentInParent<Composition>();
            if (composition == null)
            {
                composition = holder.gameObject.AddComponent<Composition>();
            }

            if (!modal.IsInitialized)
            {
                modal.Use(composition.transform);
            }
            else if (modal.ContentTransform.parent != composition.transform)
            {
                modal.ContentTransform.SetParent(composition.transform, false);
            }

            modal.IsPushed = true;
        }
    }
}
