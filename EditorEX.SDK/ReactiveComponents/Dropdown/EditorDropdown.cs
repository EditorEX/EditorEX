using System;
using System.Collections.Generic;
using EditorEX.SDK.Extensions;
using EditorEX.SDK.ReactiveComponents.Native;
using EditorEX.SDK.ReactiveComponents.Table;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents.Dropdown
{
    public partial class EditorDropdown<T> : ReactiveComponent
    {
        [Required]
        public IReadOnlyDictionary<T, BsDropdownItem> Items
        {
            get => _items!;
            set
            {
                _items = value ?? throw new ArgumentNullException(nameof(value));

                if (_cellList != null)
                {
                    RebuildCells();
                    ApplyListLayout();

                    if (_initialized)
                    {
                        var currentKey = _key!;
                        if (ContainsKey(currentKey))
                        {
                            _keyState.Value = currentKey;
                        }
                        else
                        {
                            ResetSelection();
                        }
                    }
                }

                DoInitialUpdate();
            }
        }

        [Required]
        public T Key
        {
            get => _key!;
            set
            {
                if (_items != null)
                {
                    ValidateKey(value);
                }

                if (_initialized)
                {
                    SetKey(value, true);
                }
                else
                {
                    _key = value;
                    _keyAssigned = true;
                    DoInitialUpdate();
                }
            }
        }

        public Action<T>? OnKeyChanged { get; set; }

        private bool _initialized;
        private bool _keyAssigned;
        private IReadOnlyDictionary<T, BsDropdownItem>? _items;
        private T? _key;
        private State<T?> _keyState = null!;
        private State<bool> _modalOpened = null!;
        private AnimatedState<float> _openProgress = null!;
        private ScrollContext _scrollContext = null!;
        private Layout _cellList = null!;
        private EditorBackground _listPanel = null!;
        private Reactive.Components.Basic.ScrollArea _scrollArea = null!;
        private EditorScrollbar _scrollbar = null!;
        private EditorLabel _previewLabel = null!;
        private EditorImage _previewIcon = null!;
        private CanvasGroup _listCanvasGroup = null!;
        private readonly List<EditorDropdownCell> _cells = new();

        private const int MaxDisplayedItems = 5;
        private const float ItemHeight = 40f;
        private const float PanelPadX = 2f;
        private const float PanelPadY = 4f;
        private const float ScrollbarWidth = 7f;
        private const float ListGap = 1f;
        private const float OpenAnimationSeconds = 0.2f;

        private void DoInitialUpdate()
        {
            if (
                !_initialized
                && _keyAssigned
                && _items != null
                && _keyState != null
                && _cellList != null
            )
            {
                SetKey(_key!, true);
            }
        }

        private void SetKey(T value, bool updateTable)
        {
            ValidateKey(value);

            _key = value;
            _keyAssigned = true;
            _initialized = true;
            _keyState.Value = value;

            if (updateTable)
            {
                RebuildCells();
            }
            else
            {
                ApplySelectedStates();
            }

            OnKeyChanged?.Invoke(value);
        }

        private void ValidateKey(T value)
        {
            if (!ContainsKey(value))
            {
                throw new ArgumentException("Dropdown key must exist in Items.", nameof(value));
            }
        }

        private bool ContainsKey(T value)
        {
            if (_items == null)
            {
                return false;
            }

            try
            {
                return _items.ContainsKey(value);
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        private void ResetSelection()
        {
            _initialized = false;
            _keyAssigned = false;
            _key = default;
            _keyState.Value = default;
            RebuildCells();
        }

        private float ButtonWidth()
        {
            if (!IsInitialized)
            {
                return 200f;
            }

            var width = ContentTransform.rect.width;
            return width > 0f ? width : 200f;
        }

        private int VisibleItemCount()
        {
            return Mathf.Clamp(_items?.Count ?? 1, 1, MaxDisplayedItems);
        }

        private YogaVector ListPanelSize()
        {
            return new()
            {
                x = ButtonWidth(),
                y = VisibleItemCount() * ItemHeight + PanelPadY * 2f,
            };
        }

        private YogaVector ListViewportSize()
        {
            return new()
            {
                x = Mathf.Max(0f, ButtonWidth() - ScrollbarWidth - ListGap - PanelPadX * 2f),
                y = VisibleItemCount() * ItemHeight,
            };
        }

        private void ApplyListLayout()
        {
            if (_scrollArea == null || _listPanel == null)
            {
                return;
            }

            var viewport = ListViewportSize();
            var panel = ListPanelSize();
            _scrollArea.AsFlexItem(flexGrow: 1f, size: viewport);
            _scrollArea.ContentTransform.sizeDelta = new Vector2(
                viewport.x.value,
                viewport.y.value
            );
            _listPanel.AsFlexItem(size: panel);
            ApplyCellSizes(viewport.x.value);
            RestoreScrollContentRect();
        }

        private void ApplyCellSizes(float width)
        {
            foreach (var cell in _cells)
            {
                cell.AsFlexItem(size: new() { x = width, y = ItemHeight });
            }
        }

        private void RestoreScrollContentRect()
        {
            if (_cellList == null)
            {
                return;
            }

            var contentHeight = Mathf.Max(_cells.Count, 1) * ItemHeight;
            var transform = _cellList.ContentTransform;
            transform.anchorMin = new Vector2(0f, 0f);
            transform.anchorMax = new Vector2(1f, 0f);
            transform.pivot = new Vector2(1f, 1f);
            transform.sizeDelta = new Vector2(0f, contentHeight);
        }

        private void OpenList()
        {
            if (_items == null || _items.Count == 0)
            {
                return;
            }

            ApplyListLayout();
            _openProgress.OnFinish = HandleOpenProgressFinished;
            if (!_modalOpened.Value)
            {
                _openProgress.SetValueImmediate(0f, true);
                ApplyOpenVisual(0f);
                _modalOpened.Value = true;
                _listPanel.RecalculateLayoutImmediate();
                RestoreScrollContentRect();
            }

            _openProgress.TargetValue = 1f;
        }

        private void CloseList()
        {
            if (!_modalOpened.Value && _openProgress.TargetValue <= 0f)
            {
                return;
            }

            _openProgress.OnFinish = HandleOpenProgressFinished;
            _openProgress.TargetValue = 0f;
        }

        private void HandleOpenProgressFinished(float value)
        {
            if (_openProgress.TargetValue <= 0f)
            {
                _modalOpened.Value = false;
            }
        }

        private void ApplyOpenVisual(float t)
        {
            EvaluateJumpCurve(t, out var x, out var y);
            if (_listCanvasGroup != null)
            {
                _listCanvasGroup.alpha = t;
            }

            if (_listPanel == null)
            {
                return;
            }

            var transform = _listPanel.ContentTransform;
            transform.pivot = new Vector2(0.5f, 1f);
            transform.localScale = new Vector3(x, y, 1f);
        }

        private static void EvaluateJumpCurve(float t, out float x, out float y)
        {
            x =
                t <= 0.3f
                    ? Mathf.Lerp(0.85f, 1.065f, t / 0.3f)
                    : Mathf.Lerp(1.065f, 1f, (t - 0.3f) / 0.7f);
            y =
                t <= 0.47f
                    ? Mathf.Lerp(0f, 0.95f, t / 0.47f)
                    : Mathf.Lerp(0.95f, 1f, (t - 0.47f) / 0.53f);
        }

        private void RebuildCells()
        {
            if (_cellList == null)
            {
                return;
            }

            var existing = new List<ILayoutItem>(_cellList.Children);
            foreach (var child in existing)
            {
                _cellList.Children.Remove(child);
                if (child is ReactiveComponent component)
                {
                    component.Destroy();
                }
            }

            _cells.Clear();
            if (_items == null)
            {
                return;
            }

            foreach (var pair in _items)
            {
                var key = pair.Key;
                var item = pair.Value;
                var cell = new EditorDropdownCell
                {
                    Text = item.Text ?? string.Empty,
                    Icon = item.Icon,
                    Selected = _initialized && EqualityComparer<T>.Default.Equals(key, _key!),
                    OnClick = () => SelectKey(key),
                };
                _cells.Add(cell);
                _cellList.Children.Add(
                    cell.AsFlexItem(size: new() { x = ListViewportSize().x, y = ItemHeight })
                );
            }
        }

        private void ApplySelectedStates()
        {
            if (_items == null)
            {
                return;
            }

            var index = 0;
            foreach (var pair in _items)
            {
                if (index >= _cells.Count)
                {
                    break;
                }

                _cells[index].Selected =
                    _initialized && EqualityComparer<T>.Default.Equals(pair.Key, _key!);
                index++;
            }
        }

        private void SelectKey(T key)
        {
            if (_initialized && EqualityComparer<T>.Default.Equals(key, _key!))
            {
                CloseList();
                return;
            }

            SetKey(key, false);
            CloseList();
        }

        protected override GameObject Construct()
        {
            _keyState = Remember<T?>(default);
            _modalOpened = Remember(false);
            _openProgress = RememberAnimated(
                0f,
                new AnimationDuration(OpenAnimationSeconds, DurationUnit.Seconds)
            );
            _openProgress.ValueChangedEvent += ApplyOpenVisual;

            var anchor = Remember<RectTransform?>(null);
            _scrollContext = new ScrollContext();
            _cellList = new Layout();
            _previewLabel = new EditorLabel
            {
                Alignment = TextAlignmentOptions.Left,
                FontSize = 18f,
            };
            _previewIcon = new EditorImage { PreserveAspect = true };

            void RefreshPreview(T? value)
            {
                if (
                    _initialized
                    && _items != null
                    && _items.TryGetValue(value!, out var dropdownItem)
                )
                {
                    _previewLabel.Text = dropdownItem.Text ?? string.Empty;
                    _previewLabel.Enabled = dropdownItem.Text != null;
                    _previewIcon.Sprite = dropdownItem.Icon;
                    _previewIcon.Enabled = dropdownItem.Icon != null;
                    return;
                }

                _previewLabel.Text = string.Empty;
                _previewLabel.Enabled = false;
                _previewIcon.Sprite = null;
                _previewIcon.Enabled = false;
            }

            _keyState.ValueChangedEvent += RefreshPreview;
            RefreshPreview(_keyState.Value);

            var root = new LayoutChildren
            {
                _previewLabel.AsFlexItem(flexGrow: 1f),
                _previewIcon.AsFlexItem(size: 20f, aspectRatio: 1f),
                new EditorImage
                {
                    Source = "#IconDropdown",
                    Color = new Color(0.55f, 0.6f, 0.6f, 1f),
                    PreserveAspect = true,
                }.AsFlexItem(size: 20f, aspectRatio: 1f),
                new EditorModal
                {
                    sIsPushed = _modalOpened,
                    sPlacementAnchor = anchor,
                    OnClickOutside = CloseList,
                    PlacementData = new()
                    {
                        Placement = RelativePlacement.BottomCenter,
                        Clip = true,
                    },
                    FlexController = { FlexDirection = FlexDirection.Row, Gap = 1f },
                    Children =
                    {
                        new LayoutChildren
                        {
                            new Reactive.Components.Basic.ScrollArea
                            {
                                ScrollContext = _scrollContext,
                                ScrollContent = _cellList.AsFlexGroup(
                                    direction: FlexDirection.Column,
                                    constrainHorizontal: true,
                                    constrainVertical: false
                                ),
                                LineSize = ItemHeight,
                            }
                                .AsFlexItem(size: ListViewportSize())
                                .Bind(ref _scrollArea),
                            new EditorScrollbar
                            {
                                ScrollContext = _scrollContext,
                                HideIfNothingToScroll = true,
                            }
                                .AsFlexItem(size: new() { x = ScrollbarWidth, y = "auto" })
                                .Bind(ref _scrollbar),
                        }
                            .As<EditorBackground>(x =>
                            {
                                x.Source = "#WhitePixel";
                                x.ImageType = UnityEngine.UI.Image.Type.Simple;
                            })
                            .AsFlexGroup(
                                direction: FlexDirection.Row,
                                alignItems: Align.Stretch,
                                gap: ListGap,
                                padding: new YogaFrame(PanelPadY, PanelPadX),
                                constrainHorizontal: false,
                                constrainVertical: false
                            )
                            .AsFlexItem(size: ListPanelSize())
                            .Bind(ref _listPanel),
                    },
                },
            }
                .As<EditorBackgroundButton>(x =>
                {
                    x.OnClick = () =>
                    {
                        if (_items?.Count > 0)
                        {
                            OpenList();
                        }
                    };
                })
                .AsFlexGroup(alignItems: Align.Center, padding: 8f)
                .With(x => anchor.Value = x.ContentTransform);

            var gameObject = root.Use();
            _listCanvasGroup =
                _listPanel.Content.GetComponent<CanvasGroup>()
                ?? _listPanel.Content.AddComponent<CanvasGroup>();
            ApplyOpenVisual(0f);
            RebuildCells();
            DoInitialUpdate();
            return gameObject;
        }

        protected override void OnStart()
        {
            var container = Content
                .transform.GetComponentInParent<ReactiveContainerHolder>()
                ?.ReactiveContainer;
            if (container != null)
            {
                var transition = container.TransitionCollector.GetTransition<ColorTransitionSO>(
                    "SelectableCell/Background"
                );
                _listPanel.ColorSO = transition._normalColor;
            }

            base.OnStart();
        }
    }
}
