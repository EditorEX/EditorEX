using System.Collections.Generic;
using System.Linq;
using EditorEX.SDK.ContextMenu;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents;
using Reactive;
using Reactive.Components;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Components
{
    public class ContextMenuComponent : MonoBehaviour, IContextMenuService
    {
        public EditorContextMenu? Modal => _modal;
        private List<IContextMenuProvider> _contextMenuProviders = null!;
        private EditorContextMenu? _modal;
        private IReactiveContainer? _reactiveContainer;
        private object? _linkedObject;
        private RectTransform _clickAnchor = null!;

        [Inject]
        private void Construct(
            List<IContextMenuProvider> contextMenuProviders,
            IReactiveContainer reactiveContainer
        )
        {
            _contextMenuProviders = contextMenuProviders;
            _reactiveContainer = reactiveContainer;
        }

        private void Start()
        {
            var mainScreen = GameObject.Find("MainScreen");
            transform.SetParent(mainScreen.transform, false);

            _modal = new EditorContextMenu();

            var anchorObject = new GameObject("ContextMenuClickAnchor");
            _clickAnchor = anchorObject.AddComponent<RectTransform>();
            _clickAnchor.SetParent(mainScreen.transform, false);
            _clickAnchor.sizeDelta = Vector2.zero;

            mainScreen.WithReactiveContainer(_reactiveContainer!);
        }

        private void CreateButtons<T>(IEnumerable<IContextOption> contextOptions, T data)
            where T : IContextMenuObject
        {
            if (_modal == null)
            {
                return;
            }

            var layout = _modal.Layout;

            foreach (var contextOption in contextOptions)
            {
                layout.Children.Add(
                    new EditorClickableLabel
                    {
                        Text = contextOption.GetText(),
                        FontSize = 18f,
                        OnClick = () =>
                        {
                            contextOption.Invoke(data);
                        },
                    }.AsFlexItem()
                );
            }
        }

        public void ShowContextMenu<T>(T data, Vector2 position, object? linkedObject)
            where T : IContextMenuObject
        {
            var providers = _contextMenuProviders.OfType<ContextMenuProvider<T>>().ToArray();
            if (!providers.Any())
            {
                return;
            }

            var contextOptions = providers.SelectMany(x => x.GetIContextOptions()).ToArray();

            if (_modal == null)
            {
                return;
            }

            var parentRect = (RectTransform)_clickAnchor.parent;
            var canvas = parentRect.GetComponentInParent<Canvas>();
            var camera =
                canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? canvas.worldCamera
                    : null;

            if (
                RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    parentRect,
                    position,
                    camera,
                    out var world
                )
            )
            {
                _clickAnchor.position = world;
            }

            // IsPushed=true is a no-op when already open, so pop first to re-run PlacementTool.
            _modal.IsPushed = false;
            _modal.PlacementAnchor = _clickAnchor;
            _modal.PlacementData = new PlacementData
            {
                Placement = RelativePlacement.BottomLeft,
                Clip = true,
            };
            _modal.PresentEditor(transform);

            _modal.Layout.Children.Clear();
            CreateButtons(contextOptions, data);

            _linkedObject = linkedObject;
        }

        public void TryInvalidate(object instance)
        {
            if (_linkedObject == instance)
            {
                if (_modal != null)
                {
                    _modal.IsPushed = false;
                }
                _linkedObject = null;
            }
        }
    }
}
