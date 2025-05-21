using System;
using EditorEX.SDK.ReactiveComponents.Attachable;
using Reactive;
using Reactive.BeatSaber;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents.Table
{
    public class EditorScrollbar : ReactiveComponent, IScrollbar {
        float IScrollbar.PageHeight {
            set {
                _normalizedPageHeight = Mathf.Clamp01(value);
                RefreshHandle();
            }
        }

        float IScrollbar.Progress {
            set {
                _progress = Mathf.Clamp01(value);
                RefreshHandle();
            }
        }

        bool IScrollbar.CanScrollUp {
            set => _ = value;
        }

        bool IScrollbar.CanScrollDown {
            set => _ = value;
        }

        public event Action? ScrollBackwardButtonPressedEvent;
        public event Action? ScrollForwardButtonPressedEvent;

        void IScrollbar.SetActive(bool active) {
            Enabled = active;
        }

        private float _padding = 0.25f;
        private float _progress;
        private float _normalizedPageHeight = 1f;

        private void RefreshHandle() {
            var num = _handleContainerRect.rect.size.y - 2f * _padding;
            _handleRect.sizeDelta = new Vector2(0f, _normalizedPageHeight * num);
            _handleRect.anchoredPosition = new Vector2(0f, -_progress * (1f - _normalizedPageHeight) * num - _padding);
        }

        private RectTransform _handleContainerRect = null!;
        private RectTransform _handleRect = null!;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    //handle container
                    new EditorBackground {
                        Source = "#Background3px",
                        ImageType = UnityEngine.UI.Image.Type.Sliced,

                        Children = {
                            //handle
                            new EditorImage {
                                ContentTransform = {
                                    anchorMin = new(0f, 1f),
                                    anchorMax = new(1f, 1f),
                                    pivot = new(0.5f, 1f)
                                },
                                Source = "#Background3px",
                                ImageType = UnityEngine.UI.Image.Type.Sliced
                            }.Attach<ColorSOAttachable>("Scrollbar/Handle/Normal")
                            .Bind(ref _handleRect)
                        }
                    }.With(x => x.WrappedImage.Attach<ColorSOAttachable>("Scrollbar/Background/Normal"))
                    .AsFlexItem(
                        flexGrow: 1f,
                        size: new() { x = 7f },
                        margin: new() { top = 4f, bottom = 4f }
                    ).Bind(ref _handleContainerRect)
                }
            }.AsFlexGroup(direction: FlexDirection.Column, alignItems: Align.Center).Use();
        }

        protected override void OnInitialize() {
            this.AsFlexItem(size: new() { x = 7f });
            WithinLayoutIfDisabled = true;
            RefreshHandle();
        }
    }
}