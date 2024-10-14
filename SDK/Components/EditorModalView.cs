using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using BeatmapEditor3D;

namespace EditorEX.SDK.Components
{
    public class EditorModalView : MonoBehaviour
    {
        private DiContainer _container;

        private Transform _previousParent;
        public bool isShown;
        private bool _viewIsValid;
        private Canvas _mainCanvas;
        private GameObject _blockerGO;

        public event Action blockerClickedEvent;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        private void OnDisable()
        {
            Hide();
        }

        private void OnDestroy()
        {
            if (_blockerGO)
            {
                Destroy(_blockerGO);
            }
        }

        private void SetupView(Transform screenTransform)
        {
            if (_viewIsValid)
            {
                return;
            }
            gameObject.SetActive(true);
            _mainCanvas = EssentialHelpers.GetOrAddComponent<Canvas>(gameObject);
            if (screenTransform != null)
            {
                BaseRaycaster[] components = screenTransform.GetComponents<BaseRaycaster>();
                for (int i = 0; i < components.Length; i++)
                {
                    Type type = components[i].GetType();
                    if (gameObject.GetComponent(type) == null)
                    {
                        _container.InstantiateComponent(type, gameObject);
                    }
                }
            }
            else
            {
                EssentialHelpers.GetOrAddComponent<GraphicRaycaster>(gameObject);
            }
            EssentialHelpers.GetOrAddComponent<CanvasGroup>(gameObject).ignoreParentGroups = true;
            gameObject.transform.SetParent(screenTransform, true);
            gameObject.SetActive(false);
            _viewIsValid = true;
        }

        public void Hide()
        {
            if (!isShown)
            {
                return;
            }
            Destroy(_blockerGO);
            isShown = false;

            transform.SetParent(_previousParent, true);
            gameObject.SetActive(false);
        }

        public void Show(bool useBlocker, bool moveToCenter = false)
        {
            if (isShown)
            {
                return;
            }
            Canvas canvas;
            BeatmapEditorViewController viewControllerBase;
            Transform modalRootTransform = GetModalRootTransform(transform.parent, out canvas, out viewControllerBase);
            _previousParent = transform.parent;
            if (!_viewIsValid)
            {
                SetupView(modalRootTransform);
            }
            gameObject.SetActive(true);
            gameObject.GetComponent<Canvas>().sortingLayerID = canvas.sortingLayerID;
            if (moveToCenter)
            {
                transform.SetParent(modalRootTransform, false);
                RectTransform rectTransform = (RectTransform)transform;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                Vector2 center = ((RectTransform)modalRootTransform).rect.center;
                rectTransform.localPosition = new Vector3(center.x, center.y, rectTransform.localPosition.z);
            }
            else
            {
                transform.SetParent(modalRootTransform, true);
            }
            if (useBlocker)
            {
                _blockerGO = CreateBlocker();
            }
            isShown = true;
        }

        private GameObject CreateBlocker()
        {
            GameObject gameObject = new GameObject("Blocker");
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            gameObject.AddComponent<CanvasRenderer>();
            Canvas canvas = null;
            Transform transform = _mainCanvas.transform.parent;
            while (transform != null)
            {
                canvas = transform.GetComponent<Canvas>();
                if (canvas != null)
                {
                    break;
                }
                transform = transform.parent;
            }
            rectTransform.SetParent(_mainCanvas.transform.parent, false);
            rectTransform.SetAsFirstSibling();
            //rectTransform.SetSiblingIndex(_mainCanvas.transform.GetSiblingIndex());
            rectTransform.anchorMin = Vector3.zero;
            rectTransform.anchorMax = Vector3.one;
            rectTransform.sizeDelta = Vector2.zero;
            if (canvas != null)
            {
                BaseRaycaster[] components = canvas.GetComponents<BaseRaycaster>();
                for (int i = 0; i < components.Length; i++)
                {
                    Type type = components[i].GetType();
                    if (gameObject.GetComponent(type) == null)
                    {
                        _container.InstantiateComponent(type, gameObject);
                    }
                }
            }
            else
            {
                EssentialHelpers.GetOrAddComponent<GraphicRaycaster>(gameObject);
            }
            gameObject.AddComponent<Touchable>();
            gameObject.AddComponent<Button>().onClick.AddListener(new UnityAction(HandleBlockerButtonClicked));
            return gameObject;
        }

        private void HandleBlockerButtonClicked()
        {
            Action action = blockerClickedEvent;
            if (action == null)
            {
                return;
            }
            action();
        }

        private static Transform GetModalRootTransform(Transform transform, out Canvas canvas, out BeatmapEditorViewController viewController)
        {
            BeatmapEditorScreen componentInParent = transform.GetComponentInParent<BeatmapEditorScreen>();
            canvas = componentInParent.GetComponentInChildren<Canvas>();
            viewController = componentInParent.GetComponentInChildren<BeatmapEditorViewController>();
            if (viewController != null)
            {
                return viewController.transform;
            }
            return canvas.transform;
        }
    }
}
