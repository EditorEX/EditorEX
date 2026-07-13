using System;
using System.Collections.Generic;
using System.Linq;
using EditorEX.SDK.ReactiveComponents.Table;
using EditorEX.Util;
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

                if (_tableHost != null)
                {
                    RebuildTable();

                    if (_initialized)
                    {
                        var currentKey = _key!;
                        if (ContainsKey(currentKey))
                        {
                            _keyState.Value = currentKey;
                            SynchronizeTableSelection(currentKey);
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
        private bool _suppressTableSelectionChanged;
        private IReadOnlyDictionary<T, BsDropdownItem>? _items;
        private T? _key;
        private State<T?> _keyState = null!;
        private State<bool> _modalOpened = null!;
        private ScrollContext _scrollContext = null!;
        private Layout _tableHost = null!;
        private Reactive.Components.Basic.Table<T, IReactiveComponent> _table = null!;
        private EditorScrollbar _scrollbar = null!;
        private EditorLabel _previewLabel = null!;
        private EditorImage _previewIcon = null!;

        private void DoInitialUpdate()
        {
            if (
                !_initialized
                && _keyAssigned
                && _items != null
                && _keyState != null
                && _table != null
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
                SynchronizeTableSelection(value);
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
            ClearTableSelection();
        }

        private void ClearTableSelection()
        {
            _suppressTableSelectionChanged = true;
            try
            {
                _table.SelectionMode = Reactive.Components.Basic.SelectionMode.None;
                _table.SelectionMode = Reactive.Components.Basic.SelectionMode.Single;
            }
            finally
            {
                _suppressTableSelectionChanged = false;
            }
        }

        private void SynchronizeTableSelection(T value)
        {
            _suppressTableSelectionChanged = true;
            try
            {
                _table.SelectionMode = Reactive.Components.Basic.SelectionMode.None;
                _table.SelectionMode = Reactive.Components.Basic.SelectionMode.Single;
                _table.SelectedItems = [value];
            }
            finally
            {
                _suppressTableSelectionChanged = false;
            }
        }

        private Reactive.Components.Basic.Table<T, IReactiveComponent> CreateTable()
        {
            return new Reactive.Components.Basic.Table<T, IReactiveComponent>
            {
                ScrollContext = _scrollContext,
                Items = _items?.Keys.ToArray() ?? Array.Empty<T>(),
                OnSelectedItemsChanged = HandleSelectedItemsChanged,
                ConstructCell = CreateCell,
            }.AsFlexItem(size: new() { x = 200f, y = 200f });
        }

        private void RebuildTable()
        {
            var oldTable = _table;
            _tableHost.Children.Remove(oldTable);
            oldTable.OnSelectedItemsChanged = null;
            oldTable.Destroy();

            _scrollContext = new ScrollContext();
            var replacementTable = CreateTable().Bind(ref _table);
            _scrollbar.ScrollContext = _scrollContext;
            _tableHost.Children.Add(replacementTable);
        }

        protected override GameObject Construct()
        {
            _keyState = Remember<T?>(default);
            _modalOpened = Remember(false);

            var anchor = Remember<RectTransform?>(null);
            _scrollContext = new ScrollContext();
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
                new Modal
                {
                    sIsPushed = _modalOpened,
                    sPlacementAnchor = anchor,
                    OnClickOutside = () => _modalOpened.Value = false,
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
                            new Layout { Children = { CreateTable().Bind(ref _table) } }
                                .AsFlexGroup()
                                .AsFlexItem(size: new() { x = 200f, y = 200f })
                                .Bind(ref _tableHost),
                            new EditorScrollbar
                            {
                                ScrollContext = _scrollContext,
                                HideIfNothingToScroll = true,
                            }
                                .AsFlexItem(size: new() { x = 7f, y = "auto" })
                                .Bind(ref _scrollbar),
                        }
                            .As<EditorBackground>(x =>
                            {
                                x.Source = "#Background4px";
                                x.ImageType = UnityEngine.UI.Image.Type.Sliced;
                                x.Color = new Color(0.11f, 0.13f, 0.14f, 1f);
                            })
                            .AsFlexGroup(
                                direction: FlexDirection.Row,
                                alignItems: Align.Stretch,
                                gap: 1f,
                                padding: 2f
                            ),
                    },
                },
            }
                .As<EditorBackgroundButton>(x =>
                {
                    x.OnClick = () =>
                    {
                        if (_items?.Count > 0)
                        {
                            _modalOpened.Value = true;
                        }
                    };
                })
                .AsFlexGroup(alignItems: Align.Center, padding: 8f)
                .With(x => anchor.Value = x.ContentTransform);

            var gameObject = root.Use();
            DoInitialUpdate();
            return gameObject;
        }

        private IReactiveComponent CreateCell(Reactive.Components.CellContext<T> context)
        {
            var label = new EditorLabel
            {
                Alignment = TextAlignmentOptions.Left,
                FontSize = 18f,
                RaycastTarget = false,
            };

            var icon = new EditorImage { PreserveAspect = true, RaycastTarget = false };

            var background = new LayoutChildren
            {
                label.AsFlexItem(flexGrow: 1f),
                icon.AsFlexItem(size: 20f, aspectRatio: 1f),
            }
                .As<EditorBackground>(x =>
                {
                    x.Source = "#Background4px";
                    x.ImageType = UnityEngine.UI.Image.Type.Sliced;
                    x.RaycastTarget = true;
                })
                .AsFlexGroup(alignItems: Align.Center, gap: 4f, padding: 8f)
                .AsFlexItem(size: new() { x = 200f, y = 40f });

            void Update(Reactive.Components.CellContext<T> value)
            {
                var dropdownItem = Items[value.Item];
                label.Text = dropdownItem.Text ?? string.Empty;
                label.Enabled = dropdownItem.Text != null;
                icon.Sprite = dropdownItem.Icon;
                icon.Enabled = dropdownItem.Icon != null;
                background.Color = value.Selected ? new Color(0.2f, 0.28f, 0.32f, 1f) : Color.clear;
            }

            context.ValueChangedEvent += Update;
            Update(context);
            background.WrappedImage.WithPointerEvents(onDown: _ => context.Selected = true);

            return background;
        }

        private void HandleSelectedItemsChanged(IReadOnlyCollection<T> items)
        {
            if (_suppressTableSelectionChanged || items.Count == 0)
            {
                return;
            }

            var selectedKey = items.First();
            if (_initialized && EqualityComparer<T>.Default.Equals(selectedKey, _key!))
            {
                _modalOpened.Value = false;
                return;
            }

            SetKey(selectedKey, false);
            _modalOpened.Value = false;
        }
    }
}
