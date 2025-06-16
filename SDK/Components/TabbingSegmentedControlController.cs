using EditorEX.Util;
using HMUI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EditorEX.SDK.Components
{
    public class TabbingSegmentedControlController : MonoBehaviour
    {
        internal static readonly KeyCode[] _qwertyKeyBinds =
        [
            KeyCode.Q,
            KeyCode.W,
            KeyCode.E,
            KeyCode.R,
            KeyCode.T,
            KeyCode.Y,
            KeyCode.U,
            KeyCode.I,
            KeyCode.O,
            KeyCode.P,
        ];
        private KeyboardBinder _keyboardBinder;
        private SegmentedControl _segmentedControl;

        public void Setup(SegmentedControl segmentedControl, bool qwerty)
        {
            _segmentedControl = segmentedControl;

            AddBindings(qwerty);
        }

        public void AddBindings(bool qwerty)
        {
            if (_keyboardBinder == null)
                _keyboardBinder = new KeyboardBinder();

            for (int i = 0; i < _segmentedControl.dataSource.NumberOfCells(); i++)
            {
                int b = i;
                _keyboardBinder.AddBinding(
                    qwerty ? _qwertyKeyBinds[i] : KeyCode.Alpha1 + i,
                    KeyboardBinder.KeyBindingType.KeyDown,
                    x =>
                    {
                        ClickCell(b);
                    }
                );
            }
        }

        public void ClearBindings()
        {
            _keyboardBinder?.ClearBindings();
        }

        private void Update()
        {
            _keyboardBinder?.ManualUpdate();
        }

        public void ClickCell(int number)
        {
            if (InputUtils.IsInputFieldActive())
                return;
            _segmentedControl.cells[number].OnPointerClick(new PointerEventData(null));
        }
    }
}
