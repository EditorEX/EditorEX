using System;
using System.Collections.Generic;
using System.Linq;
using EditorEX.SDK.ReactiveComponents.Table;
using EditorEX.Util;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace EditorEX.SDK.ReactiveComponents.Dropdown
{
    public partial class EditorDropdown<TKey, TParam, TCell> : ReactiveComponent, ISkewedComponent, IKeyedControl<TKey, TParam>
        where TCell : IReactiveComponent, ILayoutItem, ISkewedComponent, IPreviewableCell, IKeyedControlCell<TKey, TParam>, new()
    {
        private struct DropdownOption : IEquatable<DropdownOption>
        {
            public TKey key;
            public TParam param;

            public override int GetHashCode()
            {
                return key?.GetHashCode() ?? 0;
            }

            public override bool Equals(object? obj)
            {
                return obj is DropdownOption opt && opt.key!.Equals(key);
            }

            public bool Equals(DropdownOption other)
            {
                return key?.Equals(other.key) ?? false;
            }
        }

        public IDictionary<TKey, TParam> Items => _items;

        public TKey SelectedKey
        {
            get => _selectedKey.Value ?? throw new InvalidOperationException("Key cannot be acquired when Items is empty");
            private set
            {
                _selectedKey = value;

                SelectedKeyChangedEvent?.Invoke(value);
                NotifyPropertyChanged();
            }
        }

        public event Action<TKey>? SelectedKeyChangedEvent;

        private readonly ObservableDictionary<TKey, TParam> _items = new();
        private readonly HashSet<DropdownOption> _options = new();
        private Optional<TKey> _selectedKey;

        public void Select(TKey key)
        {
            if (_modalOpened)
            {
                Table.ClearSelection();
                Table.Select(new DropdownOption { key = key });
            }

            SelectedKey = key;

            _previewCell.Init(_selectedKey!, _items[_selectedKey!]);
        }

        private void RefreshSelection()
        {
            if (_selectedKey.HasValue || Items.Count <= 0)
            {
                return;
            }

            Select(Items.Keys.First());
        }

        public float Skew
        {
            get => _skew;
            set
            {
                _skew = value;
                _button.Skew = value;
                _previewCell.Skew = value;
            }
        }

        public bool Interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                _canvasGroup.alpha = value ? 1f : 0.25f;
                _button.Interactable = value;
            }
        }

        private float _skew;
        private bool _interactable = true;

        private EditorTable<DropdownOption, EditorDropdownCellWrapper> Table => _modal.Modal.Table;

        private bool _modalOpened;
        private SharedDropdownOptionsModal _modal = null!;

        private EditorBackgroundButton _button = null!;
        private TCell _previewCell = default!;
        private CanvasGroup _canvasGroup = null!;

        protected override GameObject Construct()
        {
            new SharedDropdownOptionsModal()
                .With(x => x.BuildImmediate())
                .Bind(ref _modal)
                .WithOpenListener(HandleModalOpened)
                .WithCloseListener(HandleModalClosed)
                .WithBeforeOpenListener(HandleBeforeModalOpened);

            return new EditorBackgroundButton
            {
                OnClick = () =>
                {
                    if (Items.Count == 0)
                    {
                        return;
                    }

                    _modal.PresentEditor(ContentTransform);
                },

                Children = {
                        new TCell {
                                UsedAsPreview = true
                            }
                            .AsFlexItem(flexGrow: 1f)
                            .Bind(ref _previewCell),

                        // Icon
                        new EditorImage {
                            Source = "#IconDropdown",
                            Color = new Color(0.55f, 0.6f, 0.6f, 1f), //Use a color collector later, its annoying to use in Construct atm
                            PreserveAspect = true
                        }.AsFlexItem(size: 20f, aspectRatio: 1f)
                    }
            }
                .WithNativeComponent(out _canvasGroup)
                .AsFlexGroup(alignContent: Reactive.Yoga.Align.Center)
                .Bind(ref _button)
                .Use();
        }

        protected override void OnInitialize()
        {
            this.AsFlexItem(size: new() { x = 36f, y = 40f });

            _items.ItemAddedEvent += HandleItemAdded;
            _items.ItemRemovedEvent += HandleItemRemoved;
            _items.AllItemsRemovedEvent += HandleAllItemsRemoved;
        }

        private void HandleBeforeModalOpened(IModal modal)
        {
            var key = new DropdownOption { key = _selectedKey.Value! };
            Table.Items.Clear();
            Table.Items.AddRange(_options);

            Table.Refresh();
            Table.Select(key);

            _modal.Modal.ApplyLayout(ContentTransform);
        }

        private void HandleModalOpened(IModal modal, bool finished)
        {
            if (finished)
            {
                return;
            }

            Table.WithListener(x => x.SelectedIndexes, HandleSelectedIndexesUpdated);
            _modalOpened = true;
        }

        private void HandleModalClosed(IModal modal, bool finished)
        {
            if (finished)
            {
                return;
            }

            Table.WithoutListener(x => x.SelectedIndexes, HandleSelectedIndexesUpdated);
            _modalOpened = false;
        }

        private void HandleSelectedIndexesUpdated(IReadOnlyCollection<int> indexes)
        {
            if (indexes.Count == 0)
            {
                return;
            }

            var index = indexes.First();
            var item = Table.FilteredItems[index];

            SelectedKey = item.key;

            _previewCell.Init(item.key, item.param);
        }

        private void HandleItemAdded(TKey key, TParam param)
        {
            var option = new DropdownOption { key = key, param = param };

            _options.Add(option);

            if (_modalOpened)
            {
                Table.Items.Add(option);
                Table.Refresh(false);
            }

            NotifyPropertyChanged(nameof(Items));
            RefreshSelection();
        }

        private void HandleItemRemoved(TKey key, TParam param)
        {
            var option = new DropdownOption { key = key };

            _options.Remove(option);

            if (_modalOpened)
            {
                Table.Items.Remove(option);
                Table.Refresh();
            }

            NotifyPropertyChanged(nameof(Items));
            RefreshSelection();
        }

        private void HandleAllItemsRemoved()
        {
            _options.Clear();

            if (_modalOpened)
            {
                Table.Items.Clear();
                Table.Refresh();
            }

            NotifyPropertyChanged(nameof(Items));
            RefreshSelection();
        }
    }
}