using EditorEX.SDK.Components;
using EditorEX.SDK.ContextMenu;
using EditorEX.SDK.Factories;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.UI.ContextMenu
{
    public class ContextMenuComponent : MonoBehaviour
    {
        private List<IContextMenuProvider> _contextMenuProviders;
        private ModalFactory _modalFactory;
        private ClickableTextFactory _clickableTextFactory;

        private VerticalLayoutGroup _verticalLayoutGroup;
        private object? _linkedObject;

        public EditorModalView modal { get; private set; }

        [Inject]
        private void Construct(
            List<IContextMenuProvider> contextMenuProviders,
            ModalFactory modalFactory,
            ClickableTextFactory clickableTextFactory)
        {
            _contextMenuProviders = contextMenuProviders;
            _modalFactory = modalFactory;
            _clickableTextFactory = clickableTextFactory;
        }

        private void Start()
        {
            var mainScreen = GameObject.Find("MainScreen");
            transform.SetParent(mainScreen.transform, false);

            modal = _modalFactory.Create(transform);

            modal.gameObject.SetActive(false);
        }

        private void CreateButtons<T>(IEnumerable<IContextOption> contextOptions, T data) where T : IContextMenuObject
        {
            if (_verticalLayoutGroup != null)
            {
                Destroy(_verticalLayoutGroup.gameObject);
            }

            _verticalLayoutGroup = new GameObject().AddComponent<VerticalLayoutGroup>();
            _verticalLayoutGroup.transform.SetParent(modal.transform);
            _verticalLayoutGroup.spacing = 5;
            _verticalLayoutGroup.padding = new RectOffset(15, 15, 8, 8);

            var fitter = _verticalLayoutGroup.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            foreach (var contextOption in contextOptions)
            {
                var clickable = _clickableTextFactory.Create(_verticalLayoutGroup.transform, contextOption.GetText(), 18f, () =>
                {
                    contextOption.Invoke(data);
                });
            }
        }

        public void ShowContextMenu<T>(T data, Vector2 position, object? linkedObject) where T : IContextMenuObject
        {
            var providers = _contextMenuProviders.OfType<ContextMenuProvider<T>>();
            if (providers == null || providers.Count() == 0)
            {
                return;
            }

            var contextOptions = providers.SelectMany(x => x.GetIContextOptions());

            CreateButtons(contextOptions, data);

            var rectTransform = modal.GetComponent<RectTransform>();

            modal.gameObject.SetActive(false);
            modal.gameObject.SetActive(true);

            position.x += rectTransform.sizeDelta.x / 2f;
            position.y -= rectTransform.sizeDelta.y / 2f;

            if (modal.isShown)
            {
                modal.Hide();
            }
            transform.position = position;
            modal.Show(true);

            _linkedObject = linkedObject;
        }

        public void TryInvalidate(object instance)
        {
            if (_linkedObject == instance)
            {
                modal.Hide();
                _linkedObject = null;
            }
        }
    }
}
