using System.Collections.Generic;
using System.Linq;
using EditorEX.SDK.ContextMenu;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.Util;
using Reactive;
using Reactive.Components;
using UnityEngine;
using Zenject;

namespace EditorEX.SDK.Components
{
    public class ContextMenuComponent : MonoBehaviour
    {
        public EditorContextMenu? Modal => _modal;
        private List<IContextMenuProvider> _contextMenuProviders = null!;
        private EditorContextMenu? _modal;
        private ReactiveContainer? _reactiveContainer;
        private object? _linkedObject;

        [Inject]
        private void Construct(
            List<IContextMenuProvider> contextMenuProviders,
            ReactiveContainer reactiveContainer
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

            _modal.IsPushed = true;

            _modal.Layout.Children.Clear();
            CreateButtons(contextOptions, data);

            var rectTransform = _modal.ViewTransform.GetComponent<RectTransform>();

            position.x += rectTransform.sizeDelta.x / 2f;
            position.y -= rectTransform.sizeDelta.y / 2f;
            rectTransform.position = position;

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
