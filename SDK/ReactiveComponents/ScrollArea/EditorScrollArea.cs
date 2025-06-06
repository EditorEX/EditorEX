using System;
using Reactive;
using Reactive.Components;
using Reactive.Components.Basic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EditorEX.SDK.ReactiveComponents.ScrollArea
{
    public class EditorScrollArea : ReactiveComponent
    {
        #region Events

        public event Action<float>? ScrollDestinationPosChangedEvent;
        public event Action<float>? ScrollPosChangedEvent;
        public event Action? ScrollWithJoystickFinishedEvent;

        #endregion

        #region Props

        public IReactiveComponent? ScrollContent
        {
            get => _scrollContent;
            set
            {
                _scrollContent?.Use(null);
                _scrollContent = value;
                if (_scrollContent != null)
                {
                    _scrollContent.Use(_viewport);
                    _contentTransform = _scrollContent.ContentTransform;
                    ReloadContent();
                }
            }
        }

        public ScrollOrientation ScrollOrientation
        {
            get => _scrollOrientation;
            set
            {
                _scrollOrientation = value;
                ReloadContent();
            }
        }

        public float ScrollSize { get; set; } = 10f;
        public float? ScrollbarScrollSize { get; set; }

        private ScrollOrientation _scrollOrientation = ScrollOrientation.Vertical;
        private IReactiveComponent? _scrollContent;

        #endregion

        #region Setup

        private RectTransform? _contentTransform;
        private float _lastScrollDeltaTime;

        protected override void OnUpdate()
        {
            if (_contentTransform == null) return;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_lastScrollDeltaTime != -1f && _lastScrollDeltaTime != Time.deltaTime)
            {
                ScrollWithJoystickFinishedEvent?.Invoke();
                _lastScrollDeltaTime = -1f;
            }
            RefreshContentPos(false);
            RefreshScrollbar();
        }

        #endregion

        #region Scroll

        public void ScrollTo(float pos, bool immediately = false)
        {
            SetDestinationPos(pos, immediately);
        }

        public void ScrollToStart(bool immediately = false)
        {
            SetDestinationPos(0f, immediately);
        }

        public void ScrollToEnd(bool immediately = false)
        {
            if (_contentTransform == null) return;
            SetDestinationPos(_contentTransform.rect.height, immediately);
        }

        #endregion

        #region Scrollbar

        public IScrollbar? Scrollbar
        {
            get => _scrollbar;
            set
            {
                if (_scrollbar != null)
                {
                    _scrollbar.ScrollBackwardButtonPressedEvent -= HandleUpButtonClicked;
                    _scrollbar.ScrollForwardButtonPressedEvent -= HandleDownButtonClicked;
                    _scrollbar.SetActive(false);
                }
                _scrollbar = value;
                if (_scrollbar != null)
                {
                    _scrollbar.ScrollBackwardButtonPressedEvent += HandleUpButtonClicked;
                    _scrollbar.ScrollForwardButtonPressedEvent += HandleDownButtonClicked;
                    _scrollbar.SetActive(true);
                    RefreshScrollbar();
                }
            }
        }

        public bool HideScrollbarWhenNothingToScroll
        {
            get => _hideScrollbarWhenNothingToScroll;
            set
            {
                _hideScrollbarWhenNothingToScroll = value;
                RefreshScrollbar();
            }
        }

        private bool _hideScrollbarWhenNothingToScroll = true;
        private IScrollbar? _scrollbar;

        private void RefreshScrollbar()
        {
            if (_scrollbar == null || _contentTransform == null)
            {
                return;
            }

            _scrollbar.PageHeight = ScrollPageSize / ContentSize;
            _scrollbar.Progress = ContentPos / ScrollMaxSize;

            _scrollbar.CanScrollDown = Math.Abs(_destinationPos - ScrollMaxSize) > 0.01f;
            _scrollbar.CanScrollUp = _destinationPos > 0.01f;

            if (_hideScrollbarWhenNothingToScroll)
            {
                _scrollbar.SetActive(ContentSize > ScrollPageSize);
            }
        }

        #endregion

        #region Content

        private float ContentPos => Translate(_contentTransform!.localPosition);
        private float ContentSize => Translate(_contentTransform!.rect);
        private float ScrollPageSize => Translate(_viewport.rect);
        private float ScrollMaxSize => ScrollPageSize <= 0f ? 0f : ContentSize - ScrollPageSize;
        private Vector2 DestinationPos => Translate(_destinationPos);

        private float _destinationPos;

        private void ReloadContent()
        {
            if (_contentTransform == null) return;
            //
            if (ScrollOrientation is ScrollOrientation.Vertical)
            {
                _contentTransform.anchorMin = new(0f, 0f);
                _contentTransform.anchorMax = new(1f, 0f);
                _contentTransform.sizeDelta = new(0f, _contentTransform.sizeDelta.y);
                _contentTransform.pivot = new(1f, 1f);
            }
            else
            {
                _contentTransform.anchorMin = new(0f, 0f);
                _contentTransform.anchorMax = new(0f, 1f);
                _contentTransform.sizeDelta = new(_contentTransform.sizeDelta.x, 0f);
                _contentTransform.pivot = new(1f, 1f);
            }
        }

        private void SetDestinationPos(float pos, bool immediately = false)
        {
            if (_contentTransform == null || Math.Abs(_destinationPos - pos) < 0.01f) return;
            _destinationPos = ScrollMaxSize <= 0f ? 0f : Mathf.Clamp(pos, 0f, ScrollMaxSize);
            ScrollDestinationPosChangedEvent?.Invoke(_destinationPos);
            //applying immediately if needed
            if (immediately) RefreshContentPos(true);
        }

        private void RefreshContentPos(bool immediately)
        {
            //calculating pos
            var sourcePos = (Vector2)_contentTransform!.localPosition;
            var destinationPos = immediately switch
            {
                false => Vector2.Lerp(sourcePos, DestinationPos, Time.deltaTime * 4f),
                _ => DestinationPos
            };
            //returning if equal
            if (sourcePos == destinationPos) return;
            _contentTransform!.localPosition = destinationPos;
            //notifying listeners
            ScrollPosChangedEvent?.Invoke(Translate(destinationPos));
        }

        #endregion

        #region Translation

        private Vector2 Translate(float value)
        {
            return ScrollOrientation switch
            {
                ScrollOrientation.Vertical => new Vector2(0f, value),
                ScrollOrientation.Horizontal => new Vector2(value, 0f),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private float Translate(Vector2 vector)
        {
            return vector[(int)ScrollOrientation];
        }

        private float Translate(Rect rect)
        {
            return Translate(rect.size);
        }

        #endregion

        #region Construct

        private PointerEventsHandler _pointerEventsHandler = null!;
        private RectTransform _viewport = null!;

        protected override GameObject Construct()
        {
            //container
            return new Background
            {
                Sprite = ReactiveResources.TransparentPixel,
                Children = {
                    //viewport
                    new ReactiveComponent {
                        Name = "Viewport",
                        ContentTransform = {
                            pivot = new(1f, 1f)
                        }
                    }.WithNativeComponent(out RectMask2D _).WithRectExpand().Bind(ref _viewport)
                }
            }.WithNativeComponent(out _pointerEventsHandler).With(
                _ => _pointerEventsHandler.PointerScrollEvent += HandlePointerScroll
            ).Use();
        }

        protected override void OnInitialize()
        {
            SetDestinationPos(0f, true);
        }

        protected override void OnRectDimensionsChanged()
        {
            if (_scrollContent == null) return;
            RefreshContentPos(true);
        }

        #endregion

        #region Callbacks

        private void HandlePointerScroll(PointerEventsHandler handler, PointerEventData eventData)
        {
            var destinationPos = _destinationPos;
            var mul = ScrollSize * 0.1f;
            if (ScrollOrientation is ScrollOrientation.Vertical)
            {
                destinationPos -= eventData.scrollDelta.y * mul;
            }
            else
            {
                destinationPos += eventData.scrollDelta.y * mul;
            }
            _lastScrollDeltaTime = Time.deltaTime;
            SetDestinationPos(destinationPos);
        }

        private void HandleUpButtonClicked()
        {
            var destinationPos = _destinationPos;
            destinationPos -= ScrollbarScrollSize ?? ScrollSize;
            SetDestinationPos(destinationPos);
        }

        private void HandleDownButtonClicked()
        {
            var destinationPos = _destinationPos;
            destinationPos += ScrollbarScrollSize ?? ScrollSize;
            SetDestinationPos(destinationPos);
        }

        #endregion
    }
}