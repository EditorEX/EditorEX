using EditorEX.SDK.Components;
using EditorEX.SDK.ContextMenu;
using EditorEX.SDK.Factories;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private EditorModalView _modal;
        private VerticalLayoutGroup _verticalLayoutGroup;

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

            _modal = _modalFactory.Create(transform);
            _modal.gameObject.AddComponent<StackLayoutGroup>();

            var fitter = _modal.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void CreateButtons<T>(IEnumerable<IContextOption> contextOptions, T data) where T : IContextMenuObject
        {
            if (_verticalLayoutGroup != null)
            {
                Destroy(_verticalLayoutGroup.gameObject);
            }

            _verticalLayoutGroup = new GameObject().AddComponent<VerticalLayoutGroup>();
            _verticalLayoutGroup.transform.SetParent(_modal.transform);
            _verticalLayoutGroup.spacing = 5;
            _verticalLayoutGroup.padding = new RectOffset(5, 5, 5, 5);

            var fitter = _verticalLayoutGroup.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            foreach (var contextOption in contextOptions)
            {
                var clickable = _clickableTextFactory.Create(_verticalLayoutGroup.transform, contextOption.GetText(), 18f, () =>
                {
                    contextOption.Invoke(data);
                });
                //clickable.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
        }

        public void ShowContextMenu<T>(T data, Vector2 position) where T: IContextMenuObject
        {
            var providers = _contextMenuProviders.OfType<ContextMenuProvider<T>>();
            if (providers == null || providers.Count() == 0)
            {
                return;
            }

            var contextOptions = providers.SelectMany(x=>x.GetIContextOptions());

            CreateButtons(contextOptions, data);

            var rectTransform = _modal.GetComponent<RectTransform>();

            _modal.gameObject.SetActive(false);
            _modal.gameObject.SetActive(true);

            position.x += rectTransform.sizeDelta.x / 2f;
            position.y -= rectTransform.sizeDelta.y / 2f;

            if (_modal.isShown)
            {
                _modal.Hide();
            }
            transform.position = position;
            _modal.Show(true);
        }
    }
}
