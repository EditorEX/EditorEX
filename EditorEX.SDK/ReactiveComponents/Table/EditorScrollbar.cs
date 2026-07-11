using System;
using EditorEX.SDK.ReactiveComponents.Attachable;
using EditorEX.Util;
using Reactive;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.SDK.ReactiveComponents.Table
{
    public enum EditorScrollbarScrollMode
    {
        Page,
        Line,
        Fixed,
    }

    public partial class EditorScrollbar : ReactiveComponent
    {
        [Required, RawState]
        public ScrollContext ScrollContext
        {
            get => _scrollContext!;
            set
            {
                if (_scrollContext != null)
                {
                    _scrollContext.ValueChangedEvent -= HandleContextUpdated;
                }

                _scrollContext = value;
                _scrollContext.ValueChangedEvent += HandleContextUpdated;

                HandleContextUpdated(value);
            }
        }

        /// <summary>
        /// How buttons should affect the target scroll controller.
        /// </summary>
        public EditorScrollbarScrollMode ScrollMode { get; set; } = EditorScrollbarScrollMode.Page;

        /// <summary>
        /// Determines the scroll size when <see cref="ScrollMode"/> is set to Fixed.
        /// </summary>
        public float FixedScrollSize { get; set; } = 10f;

        /// <summary>
        /// Whether the scrollbar should be hidden when a controller has nothing to scroll.
        /// </summary>
        public bool HideIfNothingToScroll
        {
            get => _hideIfNothingToScroll;
            set
            {
                _hideIfNothingToScroll = value;
                UpdateVisibility();
            }
        }

        private bool _hideIfNothingToScroll;
        private ScrollContext? _scrollContext;
        private float _padding = 0.25f;

        private void RefreshHandle()
        {
            if (_handleContainerRect == null || _handleRect == null)
            {
                return;
            }

            var pageHeight = _scrollContext?.NormalizedPageHeight ?? 1;

            var areaHeight = _handleContainerRect.rect.size.y - 2f * _padding;
            var pos = _scrollContext?.NormalizedScrollPos ?? 0;
            _handleRect.sizeDelta = new Vector2(0f, pageHeight * areaHeight);
            _handleRect.anchoredPosition = new Vector2(
                0f,
                -pos * (1f - pageHeight) * areaHeight - _padding
            );
        }

        private void HandleContextUpdated(ScrollContext context)
        {
            UpdateVisibility(context);
            RefreshHandle();
        }

        private void UpdateVisibility(ScrollContext? context = null)
        {
            context ??= _scrollContext;
            Enabled = !HideIfNothingToScroll || (context?.CanScroll ?? true);
        }

        private RectTransform _handleContainerRect = null!;
        private RectTransform _handleRect = null!;

        protected override GameObject Construct()
        {
            return new LayoutChildren
            {
                new LayoutChildren
                {
                    new EditorImage
                    {
                        ContentTransform =
                        {
                            anchorMin = new(0f, 1f),
                            anchorMax = new(1f, 1f),
                            pivot = new(0.5f, 1f),
                        },
                        Source = "#Background3px",
                        ImageType = Image.Type.Sliced,
                    }
                        .Attach<ColorSOAttachable>("Scrollbar/Handle/Normal")
                        .Bind(ref _handleRect),
                }
                    .As<EditorBackground>(x =>
                    {
                        x.Source = "#Background3px";
                        x.ImageType = Image.Type.Sliced;
                    })
                    .With(x =>
                        x.WrappedImage.Attach<ColorSOAttachable>("Scrollbar/Background/Normal")
                    )
                    .AsFlexItem(
                        flexGrow: 1f,
                        size: new() { x = 7f },
                        margin: new() { top = 4f, bottom = 4f }
                    )
                    .Bind(ref _handleContainerRect),
            }
                .AsLayout()
                .AsFlexGroup(direction: FlexDirection.Column, alignItems: Align.Center)
                .Use();
        }

        protected override void OnInitialize()
        {
            this.AsFlexItem(size: new() { x = 7f });
            WithinLayoutIfDisabled = true;
            RefreshHandle();
        }
    }
}
