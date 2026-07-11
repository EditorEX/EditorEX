using System;
using EditorEX.SDK.ReactiveComponents.Native;
using HMUI;
using Reactive;
using Reactive.Components;
using Reactive.Components.Basic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.SDK.ReactiveComponents
{
    /// <summary>
    /// A ModalBase that creates a full-screen (within parent canvas) blocker behind itself
    /// to intercept clicks and optionally close when the blocker is pressed.
    /// </summary>
    public abstract class BlockingModalBase : ModalBase
    {
        /// <summary>Whether a blocker should be created when the modal opens.</summary>
        public bool UseBlocker { get; set; } = true;

        /// <summary>If true (default) the modal closes when the blocker is clicked.</summary>
        public bool CloseOnBlockerClick { get; set; } = true;

        /// <summary>Raised when the blocker is clicked (before optional auto-close).</summary>
        public event Action? BlockerClickedEvent;

        private GameObject? _blockerGO;
        private ReactiveContainerHolder? _containerHolder;

        protected override void OnStart()
        {
            _containerHolder = Content.transform.GetComponentInParent<ReactiveContainerHolder>();
            base.OnStart();
        }

        protected override void OnOpen(bool opened)
        {
            // The Reactive ModalBase appears to invoke OnOpen twice (before + after); replicate dropdown pattern.
            if (!opened)
            {
                if (UseBlocker && _blockerGO == null)
                {
                    _blockerGO = CreateBlocker();
                }
            }
        }

        protected override void OnClose(bool closed)
        {
            if (!closed)
            {
                if (_blockerGO != null)
                {
                    GameObject.Destroy(_blockerGO);
                    _blockerGO = null;
                }
            }
        }

        private GameObject CreateBlocker()
        {
            var go = new GameObject("Blocker") { layer = Content.layer };
            var rectTransform = go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();

            // Find nearest Canvas in parents.
            Canvas? canvas = null;
            Transform? t = Content.transform.parent;
            while (t != null)
            {
                canvas = t.GetComponent<Canvas>();
                if (canvas != null)
                {
                    break;
                }
                t = t.parent;
            }

            rectTransform.SetParent(Content.transform.parent, false);
            rectTransform.SetAsFirstSibling();
            rectTransform.anchorMin = Vector3.zero;
            rectTransform.anchorMax = Vector3.one;
            rectTransform.sizeDelta = Vector2.zero;

            // Ensure raycasters exist on blocker
            if (canvas != null)
            {
                var components = canvas.GetComponents<BaseRaycaster>();
                foreach (var comp in components)
                {
                    var type = comp.GetType();
                    if (go.GetComponent(type) == null)
                    {
                        // Try Zenject instantiation so dependencies inject; fallback to AddComponent.
                        try
                        {
                            var instantiator = _containerHolder?.ReactiveContainer?.Instantiator;
                            if (instantiator != null)
                            {
                                instantiator.InstantiateComponent(type, go);
                            }
                            else
                            {
                                go.AddComponent(type);
                            }
                        }
                        catch
                        {
                            go.AddComponent(type);
                        }
                    }
                }
            }
            else
            {
                EssentialHelpers.GetOrAddComponent<GraphicRaycaster>(go);
            }

            // A single Graphic both blocks raycasts and dims the background behind the modal.
            // (Do NOT also add a Touchable: a GameObject may only hold one Graphic, so the
            // Image add would fail and throw.)
            go.AddComponent<UnityEngine.UI.Image>().color = new Color(0f, 0f, 0f, 0.5f);
            go.AddComponent<Button>()
                .onClick.AddListener(new UnityAction(HandleBlockerButtonClicked));
            return go;
        }

        private void HandleBlockerButtonClicked()
        {
            BlockerClickedEvent?.Invoke();
            if (CloseOnBlockerClick)
            {
                // Close immediately (immediate: true) so the modal disappears in sync with the
                // blocker, which is destroyed synchronously in OnClose. A non-immediate close
                // animates out over the close-animation duration, lagging behind the blocker.
                Close(true);
            }
        }
    }
}
