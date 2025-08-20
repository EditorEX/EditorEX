using System.Collections.Generic;
using System.Linq;
using EditorEX.SDK.Components;
using EditorEX.SDK.ContextMenu;
using EditorEX.SDK.Factories;
using EditorEX.SDK.ReactiveComponents;
using EditorEX.SDK.ReactiveComponents.Native;
using EditorEX.Util;
using Reactive;
using Reactive.Components;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.SDK.Components
{
    public class ContextMenuComponent : MonoBehaviour
    {
        public SharedModal<EditorContextMenu> Modal => _modal;
        private List<IContextMenuProvider> _contextMenuProviders = null!;
        private SharedModal<EditorContextMenu> _modal = null!;
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

            _modal = new SharedModal<EditorContextMenu>();

            mainScreen.WithReactiveContainer(_reactiveContainer!);
        }

        private void CreateButtons<T>(IEnumerable<IContextOption> contextOptions, T data)
            where T : IContextMenuObject
        {
            var layout = _modal.Modal.Layout;

            layout.Children.Clear();

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
            var providers = _contextMenuProviders.OfType<ContextMenuProvider<T>>();
            if (providers == null || providers.Count() == 0)
            {
                return;
            }

            var contextOptions = providers.SelectMany(x => x.GetIContextOptions());

            _modal.PresentEditor(transform, true);

            CreateButtons(contextOptions, data);

            var rectTransform = _modal.Modal.ContentTransform.GetComponent<RectTransform>();

            //position.x += rectTransform.sizeDelta.x / 2f;
            //position.y -= rectTransform.sizeDelta.y / 2f;
            rectTransform.position = position;

            _linkedObject = linkedObject;
        }

        public void TryInvalidate(object instance)
        {
            if (_linkedObject == instance)
            {
                _modal.Close(false);
                _linkedObject = null;
            }
        }
    }
}
