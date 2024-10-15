using EditorEX.Util;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EditorEX.SDK.Components
{
    public class TabbingSegmentedControlController : MonoBehaviour
    {
        private KeyboardBinder _keyboardBinder;
        private SegmentedControl _segmentedControl;

        public void Setup(SegmentedControl segmentedControl)
        {   
            _segmentedControl = segmentedControl;

            AddBindings();
        }

        public void AddBindings()
        {
            if (_keyboardBinder == null)
                _keyboardBinder = new KeyboardBinder();

            for (int i = 0; i < _segmentedControl.dataSource.NumberOfCells(); i++)
            {
                int b = i;
                _keyboardBinder.AddBinding(KeyCode.Alpha1 + i, KeyboardBinder.KeyBindingType.KeyDown, x =>
                {
                    ClickCell(b);
                });
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
