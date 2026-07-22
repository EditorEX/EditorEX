using System;
using System.Collections;
using EditorEX.Essentials.Features.HideUI;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents;
using Reactive;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace EditorEX.Essentials.Features.ViewMode
{
    internal class ViewModeToastHost : MonoBehaviour, IInitializable, IDisposable
    {
        private const float AutoHideSeconds = 1.2f;

        private ActiveViewMode _activeViewMode = null!;
        private IReactiveContainer _reactiveContainer = null!;
        private ViewModeToast? _toast;
        private GameObject? _canvasObject;
        private Coroutine? _hideCoroutine;
        private RectTransform? _rootRect;
        private Action? _onModeChanged;
        private Action? _onUiHiddenChanged;
        private HideUIImplementation _hideUiImplementation;

        [Inject]
        private void Construct(
            ActiveViewMode activeViewMode,
            IReactiveContainer reactiveContainer,
            HideUIImplementation hideUiImplementation
        )
        {
            _activeViewMode = activeViewMode;
            _reactiveContainer = reactiveContainer;
            _hideUiImplementation = hideUiImplementation;
        }

        public void Initialize()
        {
            _canvasObject = new GameObject("EditorEXViewModeToastCanvas");

            var canvas = _canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;

            var scaler = _canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            // No GraphicRaycaster — toast must not steal input.

            _canvasObject.WithReactiveContainer(_reactiveContainer);

            var composition = _canvasObject.GetComponent<Composition>();
            if (composition == null)
            {
                composition = _canvasObject.AddComponent<Composition>();
            }

            var root = new GameObject("ToastRoot", typeof(RectTransform));
            _rootRect = root.GetComponent<RectTransform>();
            _rootRect.SetParent(composition.transform, false);
            // ~20% up from the bottom of the screen
            _rootRect.anchorMin = new Vector2(0.5f, 0.35f);
            _rootRect.anchorMax = new Vector2(0.5f, 0.35f);
            _rootRect.pivot = new Vector2(0.5f, 0.5f);
            _rootRect.anchoredPosition = Vector2.zero;
            _rootRect.sizeDelta = Vector2.zero;

            _toast = new ViewModeToast();
            _toast.Use(root.transform);

            _onModeChanged = HandleModeChanged;
            _activeViewMode.ModeChanged += _onModeChanged;

            _onUiHiddenChanged = HandleUiHiddenChanged;
            _hideUiImplementation.OnUiHiddenChanged += _onUiHiddenChanged;
        }

        private void HandleUiHiddenChanged()
        {
            if (_rootRect == null)
            {
                return;
            }

            float offset = _hideUiImplementation.IsUiHidden ? 0.05f : 0.28f;
            _rootRect?.anchorMin = new Vector2(0.5f, offset);
            _rootRect?.anchorMax = new Vector2(0.5f, offset);
        }

        public void Dispose()
        {
            if (_onModeChanged != null)
            {
                _activeViewMode.ModeChanged -= _onModeChanged;
                _onModeChanged = null;
            }

            if (_onUiHiddenChanged != null)
            {
                _hideUiImplementation.OnUiHiddenChanged -= _onUiHiddenChanged;
                _onUiHiddenChanged = null;
            }

            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }

            if (_canvasObject != null)
            {
                Destroy(_canvasObject);
                _canvasObject = null;
            }

            _toast = null;
        }

        private void HandleModeChanged()
        {
            if (_toast == null || _activeViewMode.Mode == null)
            {
                return;
            }

            _toast.ShowOrUpdate(_activeViewMode.Mode, _activeViewMode.LastMode);

            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
            }

            float offset = _hideUiImplementation.IsUiHidden ? 0.05f : 0.28f;

            _rootRect?.anchorMin = new Vector2(0.5f, offset);
            _rootRect?.anchorMax = new Vector2(0.5f, offset);

            _hideCoroutine = StartCoroutine(AutoHideCoroutine());
        }

        private IEnumerator AutoHideCoroutine()
        {
            yield return new WaitForSecondsRealtime(AutoHideSeconds);
            _toast?.BeginHide();
            _hideCoroutine = null;
        }
    }
}
