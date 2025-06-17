using System;
using System.Text;
using EditorEX.SDK.Components;
using EditorEX.SDK.ReactiveComponents.Native;
using HMUI;
using Reactive;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.SDK.ReactiveComponents.SegmentedControl
{
    public class EditorSegmentedControlButton(int position) : ReactiveComponent
    {
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                ApplyLabel();
                NotifyPropertyChanged();
            }
        }

        public int Position { get; set; }
        public ObservableValue<int> SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                _selectedIndex.ValueChangedEvent += (value) =>
                {
                    _button.SetSelected(
                        value == Position,
                        SelectableCell.TransitionType.Instant,
                        null,
                        false
                    );
                };
                _button.SetSelected(
                    _selectedIndex.Value == Position,
                    SelectableCell.TransitionType.Instant,
                    null,
                    false
                );
                NotifyPropertyChanged();
            }
        }

        public TabbingType TabbingType
        {
            get => _tabbingType;
            set
            {
                _tabbingType = value;
                ApplyLabel();
                NotifyPropertyChanged();
            }
        }

        public SelectableCell SelectableCell => _button;

        private string _text = string.Empty;
        private TabbingType _tabbingType = TabbingType.None;
        private ObservableValue<int> _selectedIndex = new ObservableValue<int>(0);
        private EditorLabel _label = null!;
        private EditorBackground _background = null!;
        private SegmentedControlCell _button = null!;
        private SelectableCellSelectableStateController _selectableStateController = null!;

        protected override GameObject Construct()
        {
            string source = position switch
            {
                0 => "#Background8pxLeft",
                1 => "#WhitePixel",
                2 => "#Background8pxLeft",
                3 => "#Background8px",
                _ => throw new ArgumentOutOfRangeException(nameof(position), position, null),
            };
            return new EditorBackground()
            {
                Source = source,
                ImageType = Image.Type.Sliced,
                ContentTransform =
                {
                    localScale = position == 2 ? new Vector3(-1, 1, 1) : Vector3.one,
                },
                Children =
                {
                    new EditorLabel()
                    {
                        FontSize = 12,
                        ContentTransform =
                        {
                            localScale = position == 2 ? new Vector3(-1, 1, 1) : Vector3.one,
                        },
                    }
                        .AsFlexItem()
                        .Bind(ref _label),
                },
            }
                .AsFlexItem(maxSize: "fit-content")
                .AsFlexGroup(
                    justifyContent: Justify.Center,
                    padding: new YogaFrame(5, 15),
                    constrainHorizontal: false
                )
                .Bind(ref _background)
                .WithNativeComponent(out _button)
                .Use();
        }

        private void ApplyLabel()
        {
            var sb = new StringBuilder(_text);
            if (_tabbingType != TabbingType.None)
            {
                var append = _tabbingType switch
                {
                    TabbingType.Qwerty => TabbingSegmentedControlController
                        ._qwertyKeyBinds[Position]
                        .ToString(),
                    TabbingType.Alpha => (Position + 1).ToString(),
                    _ => "",
                };

                sb.Append($" | {append}");
            }
            _label.Text = sb.ToString();
        }

        protected override void OnStart()
        {
            var container = Content
                .transform.GetComponentInParent<ReactiveContainerHolder>()
                .ReactiveContainer;
            Content.SetActive(false);
            _selectableStateController =
                container.Instantiator.InstantiateComponent<SelectableCellSelectableStateController>(
                    Content
                );
            _selectableStateController._component = _button;

            var stateTransition =
                _background.Content.gameObject.AddComponent<ColorGraphicStateTransition>();
            stateTransition._transition =
                container.TransitionCollector.GetTransition<ColorTransitionSO>(
                    "SelectableCell/Background"
                );
            stateTransition._selectableStateController = _selectableStateController;
            stateTransition._component = _background.WrappedImage.ImageView;

            _button.selectionDidChangeEvent += (cell, transition, changeOwner) =>
            {
                if (cell.selected)
                {
                    SelectedIndex.Value = Position;
                }
            };

            Content.SetActive(true);
            base.OnStart();
        }
    }
}
