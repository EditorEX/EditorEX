using System;
using BeatmapEditor3D;
using HMUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.SDK.Components
{
    public class EditorModalView : MonoBehaviour
    {
        private DiContainer _container = null!;

        private Transform _previousParent;
        public bool isShown;
        private bool _viewIsValid;
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
            Transform modalRootTransform = GetModalRootTransform(transform.parent);
            _previousParent = transform.parent;
            if (!_viewIsValid)
            {
                SetupView(modalRootTransform);
            }
            gameObject.SetActive(true);
            if (moveToCenter)
            {
                transform.SetParent(modalRootTransform, false);
                RectTransform rectTransform = (RectTransform)transform;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                Vector2 center = ((RectTransform)modalRootTransform).rect.center;
                rectTransform.localPosition = new Vector3(
                    center.x,
                    center.y,
                    rectTransform.localPosition.z
                );
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
            Transform ftransform = transform.parent;
            while (ftransform != null)
            {
                canvas = ftransform.GetComponent<Canvas>();
                if (canvas != null)
                {
                    break;
                }
                ftransform = ftransform.parent;
            }
            rectTransform.SetParent(transform.parent, false);
            rectTransform.SetAsFirstSibling();
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
            gameObject
                .AddComponent<Button>()
                .onClick.AddListener(new UnityAction(HandleBlockerButtonClicked));
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

        private static Transform GetModalRootTransform(Transform transform)
        {
            BeatmapEditorScreen componentInParent =
                transform.GetComponentInParent<BeatmapEditorScreen>();
            var canvas = componentInParent.GetComponentInChildren<Canvas>();
            var viewController =
                componentInParent.GetComponentInChildren<BeatmapEditorViewController>();
            if (viewController != null)
            {
                return viewController.transform;
            }
            return canvas.transform;
        }
    }
}
