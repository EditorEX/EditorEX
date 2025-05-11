using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EditorEX.SDK.Components;
using EditorEX.SDK.ReactiveComponents.Native;
using HMUI;
using Microsoft.Build.Tasks;
using Reactive;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorEX.SDK.ReactiveComponents.SegmentedControl
{
    public class EditorSegmentedControlButton(int position) : ReactiveComponent, ILeafLayoutItem
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
        public ObservableValue<int> SelectedIndex {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                _selectedIndex.ValueChangedEvent += (value) =>
                {
                    _button.SetSelected(value == Position, SelectableCell.TransitionType.Instant, null, false);
                };
                _button.SetSelected(_selectedIndex.Value == Position, SelectableCell.TransitionType.Instant, null, false);
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
        private SelectableCell _button = null!;
        private SelectableCellSelectableStateController _selectableStateController = null!;

        protected override void Construct(RectTransform rect)
        {
            string source = position switch
            {
                0 => "#Background8pxLeft",
                1 => "#WhitePixel",
                2 => "#Background8pxLeft",
                3 => "#Background8px",
                _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
            };
            new Layout() {
                Children = {
                    new EditorBackground() {
                            Source = source,
                            ImageType = Image.Type.Sliced,
                            Children = {
                                new EditorLabel() {
                                    FontSize = 12,
                                }
                                .AsFlexItem()
                                .Bind(ref _label)
                            }
                        }
                        .AsFlexGroup(justifyContent: Justify.Center)
                        .Bind(ref _background)
                }
            }.AsFlexGroup().Use(rect);

            if (position == 2)
                _background.ContentTransform.localScale = new Vector3(-1, 1, 1);

            if (position == 2)
                _label.ContentTransform.localScale = new Vector3(-1, 1, 1);

            _button = rect.gameObject.AddComponent<SegmentedControlCell>();

            base.Construct(rect);
        }

        private void ApplyLabel()
        {
            var sb = new StringBuilder(_text);
            if (_tabbingType != TabbingType.None)
            {
                var append = _tabbingType switch
                {
                    TabbingType.Qwerty => TabbingSegmentedControlController._qwertyKeyBinds[Position].ToString(),
                    TabbingType.Alpha => (Position + 1).ToString(),
                    _ => ""
                };

                sb.Append($" | {append}");
            }
            _label.Text = sb.ToString();
        }

        protected override void OnStart()
        {
            var container = Content.transform.GetComponentInParent<ReactiveContainerHolder>().ReactiveContainer;
            Content.SetActive(false);
            _selectableStateController = container.Instantiator.InstantiateComponent<SelectableCellSelectableStateController>(Content);
            _selectableStateController._component = _button;

            var stateTransition = _background.Content.gameObject.AddComponent<ColorGraphicStateTransition>();
            stateTransition._transition = container.TransitionCollector.GetTransition<ColorTransitionSO>("SelectableCell/Background");
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

        public event Action<ILeafLayoutItem>? LeafLayoutUpdatedEvent;

        public Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            var measuredWidth = widthMode == MeasureMode.Undefined ? Mathf.Infinity : width;
            var measuredHeight = heightMode == MeasureMode.Undefined ? Mathf.Infinity : height;

            var textSize = _label.TextMesh.GetPreferredValues(measuredWidth, measuredHeight);

            return new()
            {
                x = widthMode == MeasureMode.Exactly ? width : Mathf.Min(textSize.x+40, measuredWidth),
                y = heightMode == MeasureMode.Exactly ? height : Mathf.Min(textSize.y, measuredHeight)
            };
        }

        private void RequestLeafRecalculation()
        {
            LeafLayoutUpdatedEvent?.Invoke(this);
            ScheduleLayoutRecalculation();
        }
    }
}