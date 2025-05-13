using System.Linq;
using EditorEX.SDK.Components;
using EditorEX.SDK.ReactiveComponents.SegmentedControl;
using EditorEX.Util;
using HMUI;
using Reactive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EditorEX.SDK.ReactiveComponents
{
    public class EditorSegmentedControl : ReactiveComponent
    {
        public string[] Values
        {
            get => _values;
            set
            {
                _values = value;
                _layout.Children.Clear();
                _layout.Children.AddRange(_values.Select((value, index) => new EditorSegmentedControlButton(GetPosition(index, _values.Length)) {
                    Text = value,
                    Position = index,
                    SelectedIndex = _selectedIndex,
                }.AsFlexItem(size: "max-content", minSize: "max-content", flex: 1)));
                NotifyPropertyChanged();
            }
        }

        private int GetPosition(int index, int count)
        {
            return index switch
            {
                0 => 0,
                _ when count == 1 => 3,
                _ when index == count - 1 => 2,
                _ => 1,
            };
        }

        public ObservableValue<int> SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                foreach (var child in _layout.Children.OfType<EditorSegmentedControlButton>())
                {
                    child.SelectedIndex = value;
                }
                NotifyPropertyChanged();
            }
        }

        public TabbingType TabbingType
        {
            get => _tabbingType;
            set
            {
                _tabbingType = value;
                foreach (var child in _layout.Children.OfType<EditorSegmentedControlButton>())
                {
                    child.TabbingType = value;
                }
                _keyboardBinder.ClearBindings();
                AddBindings();
                
                NotifyPropertyChanged();
            }
        }

        private string[] _values = [];
        private TabbingType _tabbingType = TabbingType.None;
        private ObservableValue<int> _selectedIndex = new ObservableValue<int>(0);
        private Layout _layout = null!;
        private KeyboardBinder _keyboardBinder = new KeyboardBinder();
        protected override GameObject Construct()
        {
            return new Layout()
                .AsFlexGroup(Reactive.Yoga.FlexDirection.Row, justifyContent: Reactive.Yoga.Justify.SpaceAround)
                .AsFlexItem(size: "max-content")
                .Export(out _layout)
                .Use();
        }

        protected override void OnUpdate()
        {
            _keyboardBinder?.ManualUpdate();
            base.OnUpdate();
        }

        private void AddBindings()
        {
            bool qwerty = TabbingType == TabbingType.Qwerty;
            for (int i = 0; i < _layout.Children.OfType<EditorSegmentedControlButton>().Count(); i++)
            {
                int b = i;
                _keyboardBinder.AddBinding(qwerty ? TabbingSegmentedControlController._qwertyKeyBinds[i] : KeyCode.Alpha1 + i, KeyboardBinder.KeyBindingType.KeyDown, x =>
                {
                    ClickCell(b);
                });
            }
        }

        private void ClickCell(int number)
        {
            if (InputUtils.IsInputFieldActive())
                return;
            foreach (var child in _layout.Children.OfType<EditorSegmentedControlButton>())
            {
                if (child.Position == number)
                {
                    child.SelectableCell.OnPointerClick(new PointerEventData(null));
                    break;
                }
            }
        }
    }
}